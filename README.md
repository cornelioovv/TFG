# TFG - Prototipo de metaverso sobre gemelo digital de feria de arte

Prototipo de metaverso desarrollado en Unity que actúa como gemelo digital de una feria física de arte plástico. Tres clientes (web admin, Unity, y un futuro microcontrolador ESP32 con LED RGB) comparten estado en tiempo real vía un broker MQTT.

**Autor:** Cornelio Velasco Egea
**Directores:** Enrique García Salcines · Juan Alfonso Lara Torralbo
**Universidad de Córdoba · EPSC**

---

## Estructura del repositorio

```
.
├── backend/api/          API REST en Node.js + Express + PostgreSQL
├── frontend/frontend/    Panel admin en Next.js + React + Bootstrap
├── unity/MetaversoTFG/   Cliente 3D en Unity 6
├── infra/mqtt/           Configuración del broker MQTT (Mosquitto)
├── docs/                 Memoria, anteproyecto y schema SQL
└── package.json          Orquestador raíz (concurrently)
```

## Arquitectura en una frase

Backend Express expone una API REST contra PostgreSQL para CRUD de obras, ventas y ferias. Al registrarse una venta, publica un mensaje MQTT en `obras/<id>/vendido`. Los clientes (web admin vía MQTT-over-WebSockets, Unity vía MQTT/TCP, ESP32 idem) están suscritos y reaccionan en vivo.

---

## Requisitos previos

- **Node.js >= 20** ([nodejs.org](https://nodejs.org))
- **PostgreSQL 14+** ([postgresql.org](https://www.postgresql.org/download/))
- **Docker Desktop** ([docker.com](https://www.docker.com/products/docker-desktop/)) - para el broker MQTT
- **Unity Hub + Unity 6** ([unity.com](https://unity.com/download)) - para el cliente 3D

---

## Puesta en marcha

### 1. Clonar e instalar dependencias

```bash
git clone https://github.com/cornelioovv/TFG
cd TFG
npm install
```

`npm install` en la raíz, gracias al hook `postinstall`, dispara la instalación de las dependencias del backend y del frontend automáticamente. Una sola orden.

### 2. Configurar la base de datos

Crea solo la base de datos vacía. El backend aplicará el schema automáticamente al arrancar:
```bash
createdb metaverso_tfg
```

Configurar el backend con tus credenciales:
```bash
cp backend/api/.env.example backend/api/.env
# editar backend/api/.env con tu usuario/contraseña de Postgres
```

> El schema se aplica de forma **idempotente** en cada arranque del backend (lee `docs/Scripts BD/tables_creation.sql` y usa `CREATE TABLE IF NOT EXISTS`). No tienes que ejecutar `psql` ni hacer migraciones manuales.

### 3. Configurar el frontend (opcional)

Los valores por defecto del frontend ya apuntan a `localhost`. Si quieres dejarlo explícito:
```bash
cp frontend/frontend/.env.example frontend/frontend/.env.local
```

### 4. Arrancar el broker MQTT

```bash
npm run mqtt:up
```

Esto levanta un contenedor Docker con Mosquitto escuchando en **puerto 1883 (TCP)** y **9001 (WebSockets)**. Para pararlo: `npm run mqtt:down`.

### 5. Arrancar backend + frontend con un solo comando

```bash
npm run dev
```

Concurrently lanza ambos servicios en paralelo:
- **Backend** → http://localhost:3000
- **Frontend** → http://localhost:3001

Ctrl+C para los dos.

### 6. Ejecutar todo con Docker

Si tu tutor prefiere probar el proyecto sin instalar Node.js ni PostgreSQL localmente, usa Docker.

```bash
docker compose up --build
```

Esto levanta:
- **PostgreSQL** en el contenedor `database`
- **Mosquitto MQTT** en `mqtt`
- **Backend Express** en http://localhost:3000
- **Frontend Next.js** en http://localhost:3001

Para detenerlo:

```bash
docker compose down
```

> El backend crea las tablas y genera datos semilla automáticamente en el primer arranque.

### 7. (Opcional) Cliente Unity

1. Abre `unity/MetaversoTFG/` desde Unity Hub.
2. Espera a que importe los assets (puede tardar la primera vez).
3. Abre `Assets/Scenes/SampleScene.unity` y dale a Play.
4. Asegúrate de tener el backend y el broker corriendo antes.

---

## Comandos disponibles desde la raíz

| Comando | Qué hace |
|---|---|
| `npm install` | Instala dependencias del backend y frontend |
| `npm run dev` | Arranca backend + frontend en paralelo |
| `npm run dev:backend` | Solo backend |
| `npm run dev:frontend` | Solo frontend |
| `npm run mqtt:up` | Arranca el broker MQTT (Docker) |
| `npm run mqtt:down` | Para el broker |
| `npm run mqtt:logs` | Muestra logs del broker en vivo |
| `npm run build` | Build de producción (backend + Next.js) |
| `npm run lint` | Linter del frontend |

---

## Demo del sistema en vivo

Con backend + frontend + broker + Unity corriendo:

1. En el navegador, ve a `http://localhost:3001/obras`.
2. En Unity, acércate a un cuadro y pulsa **E** o click → modal con detalle.
3. Rellena nombre y email del comprador y pulsa "Comprar".
4. En **menos de un segundo**: la card de la web cambia a "Vendido", el dashboard recalcula KPIs y el LED del cuadro en Unity pasa de verde a rojo.
5. A la inversa: marca un cuadro como vendido desde la web. Unity refleja el cambio sin recargar nada.

Esa sincronización en vivo entre web, mundo virtual (Unity) y (próximamente) hardware físico (LED + ESP32) es el corazón del gemelo digital descrito en el anteproyecto.
