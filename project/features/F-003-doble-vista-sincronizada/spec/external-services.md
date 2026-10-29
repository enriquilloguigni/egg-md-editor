# External services — F-003 Doble vista sincronizada

No requerido / Fuera de alcance — F-003 no consume servicios externos: la aplicación es 100 % local y offline (BR-012, NFR-004, NFR-005).

- El parser CommonMark+GFM, el renderizado WYSIWYG y la sincronización son componentes internos; no hay red, autenticación remota ni API de terceros.
- La sincronización, el mapa de posiciones y el resaltado operan solo sobre el modelo local y los paneles de SC-001; ningún dato sale del equipo (NFR-005).
- Sin telemetría, actualizaciones automáticas, sincronización en la nube ni comprobaciones remotas (C-013, BR-012).
- Separación con inapp-communication.md: la comunicación interna entre paneles, modelo, motor de sincronización, layout y barra de formato vive allí (COM-016–COM-021); no hay servicio externo que especificar.