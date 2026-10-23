# External services — F-001 Entorno de aplicación

No requerido / Fuera de alcance — F-001 no consume servicios externos: la aplicación es 100 % local y offline (BR-012, NFR-004, NFR-005, C-013).

- No hay red ni dependencias remotas en el arranque ni en la UI de menú, barra de formato, tema e idioma (A-010, architecture.md).
- Las librerías de dominio (iconos, i18n) se resuelven localmente y ninguna requiere servicio externo (NFR-011).
- Separación con inapp-communication.md: la comunicación interna entre menú, barra, atajos y dispatch vive allí (COM-001 a COM-006); no existe un servicio externo al sistema que cubrir aquí.