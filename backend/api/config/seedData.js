const { pool } = require('./database');

/**
 * Obras de ejemplo para que el revisor pueda probar la funcionalidad
 * (listado, filtros, modal de detalle, compra...) sin tener que insertar
 * datos manualmente. Sin imagen — los cuadros saldrán con el placeholder.
 */
const OBRAS_SEED = [
  {
    titulo: 'La Gioconda',
    autor: 'Leonardo da Vinci',
    descripcion:
      'Retrato de Lisa Gherardini, célebre por su sonrisa enigmática y la técnica del sfumato.',
    precio: 850000000,
    anio: 1503,
    dimensiones: '77 x 53 cm',
  },
  {
    titulo: 'La noche estrellada',
    autor: 'Vincent van Gogh',
    descripcion:
      'Vista nocturna desde la ventana del sanatorio de Saint-Rémy-de-Provence.',
    precio: 100000000,
    anio: 1889,
    dimensiones: '73.7 x 92.1 cm',
  },
  {
    titulo: 'El grito',
    autor: 'Edvard Munch',
    descripcion:
      'Icónica representación de la angustia existencial moderna.',
    precio: 120000000,
    anio: 1893,
    dimensiones: '91 x 73.5 cm',
  },
  {
    titulo: 'La persistencia de la memoria',
    autor: 'Salvador Dalí',
    descripcion:
      'Surrealismo onírico con relojes blandos sobre un paisaje de Cadaqués.',
    precio: 150000000,
    anio: 1931,
    dimensiones: '24 x 33 cm',
  },
  {
    titulo: 'Las Meninas',
    autor: 'Diego Velázquez',
    descripcion:
      'Retrato de la familia real española en el Alcázar de Madrid.',
    precio: 3000000000,
    anio: 1656,
    dimensiones: '318 x 276 cm',
  },
  {
    titulo: 'Guernica',
    autor: 'Pablo Picasso',
    descripcion:
      'Mural antibélico encargado para la Exposición Internacional de París.',
    precio: 200000000,
    anio: 1937,
    dimensiones: '349 x 776 cm',
  },
  {
    titulo: 'El nacimiento de Venus',
    autor: 'Sandro Botticelli',
    descripcion:
      'Representación mitológica de Venus emergiendo del mar sobre una concha.',
    precio: 500000000,
    anio: 1485,
    dimensiones: '172.5 x 278.5 cm',
  },
  {
    titulo: 'Composición VIII',
    autor: 'Wassily Kandinsky',
    descripcion:
      'Abstracción geométrica de formas y colores; piedra angular del arte abstracto.',
    precio: 80000000,
    anio: 1923,
    dimensiones: '140 x 201 cm',
  },
];

const FERIAS_SEED = [
  {
    nombre: 'ARCOmadrid 2026',
    ubicacion: 'IFEMA, Madrid',
    fecha_inicio: '2026-02-25',
    fecha_fin: '2026-03-01',
    descripcion:
      'Feria Internacional de Arte Contemporáneo, una de las citas más relevantes del mercado del arte en Europa.',
  },
  {
    nombre: 'Art Basel 2026',
    ubicacion: 'Basilea, Suiza',
    fecha_inicio: '2026-06-18',
    fecha_fin: '2026-06-21',
    descripcion:
      'Referente mundial del arte moderno y contemporáneo.',
  },
];

/**
 * Inserta los datos de prueba SOLO si las tablas correspondientes están
 * vacías. Idempotente: en arranques posteriores no toca nada.
 */
async function seedDataIfEmpty() {
  // Obras
  const obrasCount = await pool.query('SELECT COUNT(*)::int AS total FROM obras');
  if (obrasCount.rows[0].total === 0) {
    console.log(`[DB] Insertando ${OBRAS_SEED.length} obras de ejemplo...`);
    for (const o of OBRAS_SEED) {
      await pool.query(
        `INSERT INTO obras (titulo, autor, descripcion, precio, anio, dimensiones)
         VALUES ($1, $2, $3, $4, $5, $6)`,
        [o.titulo, o.autor, o.descripcion, o.precio, o.anio, o.dimensiones]
      );
    }
    console.log('[DB] Obras de ejemplo insertadas');
  }

  // Ferias
  const feriasCount = await pool.query('SELECT COUNT(*)::int AS total FROM ferias');
  if (feriasCount.rows[0].total === 0) {
    console.log(`[DB] Insertando ${FERIAS_SEED.length} ferias de ejemplo...`);
    for (const f of FERIAS_SEED) {
      await pool.query(
        `INSERT INTO ferias (nombre, ubicacion, fecha_inicio, fecha_fin, descripcion)
         VALUES ($1, $2, $3, $4, $5)`,
        [f.nombre, f.ubicacion, f.fecha_inicio, f.fecha_fin, f.descripcion]
      );
    }
    console.log('[DB] Ferias de ejemplo insertadas');
  }
}

module.exports = seedDataIfEmpty;
