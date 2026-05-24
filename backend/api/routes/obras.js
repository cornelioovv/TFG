const express = require('express');
const router = express.Router();
const { pool } = require('../config/database');

// GET todas las obras (incluye ferias vinculadas)
router.get('/', async (req, res) => {
  try {
    const result = await pool.query(`
      SELECT
        o.*,
        COALESCE(
          json_agg(
            json_build_object('id', f.id, 'nombre', f.nombre)
            ORDER BY f.nombre
          ) FILTER (WHERE f.id IS NOT NULL),
          '[]'
        ) AS ferias
      FROM obras o
      LEFT JOIN obras_ferias of2 ON of2.obra_id = o.id
      LEFT JOIN ferias f ON f.id = of2.feria_id
      GROUP BY o.id
      ORDER BY o.creado_en DESC
    `);
    res.json(result.rows);
  } catch (error) {
    console.error('Error obteniendo obras:', error);
    res.status(500).json({ error: 'Error obteniendo obras', detail: error.message });
  }
});

// GET obra por ID (incluye ferias vinculadas)
router.get('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const result = await pool.query(
      `SELECT
         o.*,
         COALESCE(
           json_agg(
             json_build_object('id', f.id, 'nombre', f.nombre)
             ORDER BY f.nombre
           ) FILTER (WHERE f.id IS NOT NULL),
           '[]'
         ) AS ferias
       FROM obras o
       LEFT JOIN obras_ferias of2 ON of2.obra_id = o.id
       LEFT JOIN ferias f ON f.id = of2.feria_id
       WHERE o.id = $1
       GROUP BY o.id`,
      [id]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Obra no encontrada' });
    }

    res.json(result.rows[0]);
  } catch (error) {
    console.error('Error obteniendo obra:', error);
    res.status(500).json({ error: 'Error obteniendo obra', detail: error.message });
  }
});

// POST crear obra
router.post('/', async (req, res) => {
  try {
    const { titulo, autor, descripcion, precio, anio, dimensiones, imagen_url } = req.body;
    
    const result = await pool.query(
      `INSERT INTO obras (titulo, autor, descripcion, precio, anio, dimensiones, imagen_url)
       VALUES ($1, $2, $3, $4, $5, $6, $7) RETURNING *`,
      [titulo, autor, descripcion, precio, anio, dimensiones, imagen_url]
    );
    
    res.status(201).json(result.rows[0]);
  } catch (error) {
    console.error('Error creando obra:', error);
    res.status(500).json({ error: 'Error creando obra' });
  }
});

// PUT actualizar obra
router.put('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const {
      titulo,
      autor,
      descripcion,
      precio,
      anio,
      dimensiones,
      imagen_url,
      vendido,
    } = req.body;

    const result = await pool.query(
      `UPDATE obras
       SET titulo = COALESCE($1, titulo),
           autor = COALESCE($2, autor),
           descripcion = COALESCE($3, descripcion),
           precio = COALESCE($4, precio),
           anio = COALESCE($5, anio),
           dimensiones = COALESCE($6, dimensiones),
           imagen_url = COALESCE($7, imagen_url),
           vendido = COALESCE($8, vendido),
           actualizado_en = CURRENT_TIMESTAMP
       WHERE id = $9 RETURNING *`,
      [titulo, autor, descripcion, precio, anio, dimensiones, imagen_url, vendido, id]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Obra no encontrada' });
    }

    res.json(result.rows[0]);
  } catch (error) {
    console.error('Error actualizando obra:', error);
    res.status(500).json({ error: 'Error actualizando obra', detail: error.message });
  }
});

// DELETE eliminar obra
router.delete('/:id', async (req, res) => {
  try {
    const { id } = req.params;

    const ventasCheck = await pool.query(
      'SELECT COUNT(*)::int AS total FROM ventas WHERE obra_id = $1',
      [id]
    );
    const ventasCount = ventasCheck.rows[0].total;

    if (ventasCount > 0) {
      return res.status(409).json({
        error: 'No se puede eliminar la obra',
        detail: `Tiene ${ventasCount} venta(s) asociada(s). Elimina las ventas primero.`,
      });
    }

    const result = await pool.query(
      'DELETE FROM obras WHERE id = $1 RETURNING *',
      [id]
    );

    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Obra no encontrada' });
    }

    res.json({ message: 'Obra eliminada', obra: result.rows[0] });
  } catch (error) {
    console.error('Error eliminando obra:', error);
    res.status(500).json({ error: 'Error eliminando obra', detail: error.message });
  }
});

module.exports = router;