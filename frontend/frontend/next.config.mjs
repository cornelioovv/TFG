import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactCompiler: true,
  allowedDevOrigins: ['172.20.10.8'],
  // El package-lock.json de la raíz (del orquestador `concurrently`) confunde a
  // Turbopack haciéndole pensar que el workspace root es la raíz del monorepo.
  // Forzamos que el "project filesystem" sea esta carpeta.
  turbopack: {
    root: __dirname,
  },
};

export default nextConfig;
