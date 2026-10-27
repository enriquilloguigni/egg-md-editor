# External services — F-002 Gestión de archivo

No requerido / Fuera de alcance — F-002 no consume servicios externos: la aplicación es 100 % local y offline (BR-012, NFR-004, NFR-005).

- El sistema de archivos y los selectores nativos son servicios locales del SO, no servicios externos; no hay red, autenticación remota ni API de terceros.
- Abrir, guardar, recientes, autoguardado y recuperación operan solo sobre rutas elegidas por el usuario y el almacén local; ningún dato sale del equipo (NFR-005).
- Sin telemetría, actualizaciones automáticas, sincronización en la nube ni comprobaciones remotas (C-013, BR-012).
- Separación con inapp-communication.md: la comunicación interna entre menú, coordinador, documento, diálogos, temporizador y título vive allí (COM-007–COM-015); no hay servicio externo que especificar.
