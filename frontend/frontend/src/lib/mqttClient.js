'use client';
import mqtt from 'mqtt';

// Cliente MQTT singleton para el navegador (vía WebSockets).
// Se conecta perezosamente la primera vez que alguien se suscribe a eventos.

const MQTT_URL = process.env.NEXT_PUBLIC_MQTT_URL || 'ws://localhost:9001';

let client = null;
const listeners = new Set();

function ensureClient() {
  if (typeof window === 'undefined') return null; // protección SSR
  if (client) return client;

  client = mqtt.connect(MQTT_URL, {
    reconnectPeriod: 5000,
    connectTimeout: 10_000,
    clientId: `tfg-web-${Math.random().toString(16).slice(2, 8)}`,
  });

  client.on('connect', () => {
    console.log('[MQTT-WS] Conectado a', MQTT_URL);
    client.subscribe('obras/+/vendido', (err) => {
      if (err) console.error('[MQTT-WS] Error al suscribir:', err);
    });
  });

  client.on('message', (topic, payload) => {
    const match = topic.match(/^obras\/(\d+)\/vendido$/);
    if (!match) return;
    const obraId = parseInt(match[1], 10);

    let data = {};
    try {
      data = JSON.parse(payload.toString());
    } catch {
      // payload no JSON; lo ignoramos
    }

    const event = { type: 'sale', obraId, ...data };
    listeners.forEach((cb) => {
      try {
        cb(event);
      } catch (e) {
        console.error('[MQTT-WS] Error en listener:', e);
      }
    });
  });

  client.on('error', (err) => console.error('[MQTT-WS] Error:', err.message));
  client.on('reconnect', () => console.log('[MQTT-WS] Reconectando…'));
  client.on('offline', () => console.warn('[MQTT-WS] Offline'));

  return client;
}

export function subscribeMqttEvents(callback) {
  if (typeof window === 'undefined') return () => {};
  listeners.add(callback);
  ensureClient();
  return () => listeners.delete(callback);
}
