# In-app communication — F-003 Doble vista sincronizada

## COM-016 — Panel ↔ modelo único
Origen: panel izquierdo/derecho (SC-001) → Destino: modelo de documento (`ENT-008`) · Tipo: síncrono.
Información enviada/recibida: se envía el cambio de contenido (inserción/borrado/formato) y la posición; se recibe el modelo actualizado y el árbol sintáctico (`ENT-016`).
Propósito: mantener un único documento compartido por ambos paneles (BR-001, BR-F003-001) · Condición: panel instanciado y documento cargado.

## COM-017 — Evento de cambio → sincronización de vistas
Origen: modelo de documento → Destino: ambos paneles (SC-001) · Tipo: asíncrono (evento, sin sondeo).
Información enviada/recibida: se envía el delta de contenido y la versión del árbol; se reciben las vistas re-renderizadas (OP-020, OP-021).
Propósito: reflejar cambios en tiempo real en el panel opuesto (C-002, BR-F003-004, NFR-001) · Condición: cambio confirmado en el modelo.

## COM-018 — Mapa de posiciones → desplazamiento sincronizado
Origen: motor de sincronización → Destino: paneles (SC-001) · Tipo: síncrono.
Información enviada/recibida: se envían las anclas (`ENT-017`) y la posición de origen; se recibe el desplazamiento del panel destino.
Propósito: correspondencia de posición entre vistas (FR-001, BR-F003-005) · Condición: ambos paneles visibles y ancla encontrada.

## COM-019 — Resaltado pasajero → vistas
Origen: motor de sincronización → Destino: panel destino (SC-001) · Tipo: síncrono (con expiración).
Información enviada/recibida: se envía el rango a resaltar (`ENT-018`); se recibe la aplicación y la expiración automática.
Propósito: señalar la sección equivalente al cambiar de panel (BR-014, BR-F003-006) · Condición: cambio de panel activo y ancla encontrada.

## COM-020 — Conmutador de visibilidad → layout
Origen: menú Ver/atajos → Destino: layout de SC-001 y preferencias (`ENT-019`) · Tipo: síncrono.
Información enviada/recibida: se envía la preferencia {panel izq., panel der., barra}; se recibe el layout actualizado y la persistencia.
Propósito: mostrar/ocultar paneles y barra sin perder contenido (BR-F003-007, BR-019) · Condición: al menos un panel permanece visible.

## COM-021 — Barra de formato ↔ panel activo
Origen: barra de formato (F-001) → Destino: panel activo (SC-001) · Tipo: síncrono.
Información enviada/recibida: se envía el comando de formato; se recibe su aplicación como efecto visual (izq.) o sintaxis equivalente (der.).
Propósito: formato según el panel activo (BR-016, FR-012) · Condición: barra visible y panel activo disponible.

Referencias:
- ERR-016–ERR-019 (fallos de sincronización, tamaño, ancla y renderizado).
- LOG-009–LOG-011 (registro técnico de sincronía, visibilidad y resaltado).