# Audit — F-003 Doble vista sincronizada

No requerido / Fuera de alcance — F-003 no genera eventos de auditoría funcional.

- Un único actor local sin autenticación ni roles (security-permissions.md): el «quién» es siempre la cuenta del SO y no hay responsabilidad que reconstruir.
- La sincronización, el desplazamiento y el resaltado son efectos automáticos sobre el documento del propio usuario; no hay objetos de negocio compartidos ni cambios que requieran trazabilidad antes/después.
- Las ediciones se diagnostican en `LOG-009`–`LOG-011` y los fallos en `error-handling.md`; el contenido y las rutas no se registran por privacidad (NFR-010).
- Las preferencias de visibilidad son estado local operativo, no un historial funcional; su ciclo se especifica en data-structures y security-permissions.
- Separación con logs.md: el diagnóstico técnico vive allí; por la regla anti-sobre-registro, no corresponde un audit de F-003.