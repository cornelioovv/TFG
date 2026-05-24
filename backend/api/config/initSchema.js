const fs = require('fs');
const path = require('path');
const { pool } = require('./database');

const SQL_PATH = path.join(
  __dirname,
  '..',
  '..',
  '..',
  'docs',
  'Scripts BD',
  'tables_creation.sql'
);

/**
 * Aplica el schema de la BD si no existe. Idempotente: las sentencias usan
 * `CREATE TABLE IF NOT EXISTS`, así que pueden re-ejecutarse sin riesgo.
 *
 * Se llama desde server.js al arrancar. Si la BD no es accesible, lanza
 * el error y el servidor no arranca (mensaje claro para el dev).
 */
async function initSchema() {
  if (!fs.existsSync(SQL_PATH)) {
    console.warn(`[DB] No se encontró el schema en ${SQL_PATH}, se omite init.`);
    return;
  }

  const sql = fs.readFileSync(SQL_PATH, 'utf8');
  try {
    await pool.query(sql);
    console.log('[DB] Schema verificado/aplicado correctamente');
  } catch (err) {
    console.error('[DB] Error aplicando schema:', err.message);
    throw err;
  }
}

module.exports = initSchema;
