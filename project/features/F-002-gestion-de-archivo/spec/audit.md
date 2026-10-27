# Audit — F-002 Gestión de archivo

No requerido / Fuera de alcance — F-002 no genera eventos de auditoría funcional.

- Un único actor local sin autenticación ni roles (security-permissions.md): el «quién» es siempre la cuenta del SO y no hay responsabilidad que reconstruir.
- Las operaciones de archivo son acciones directas del usuario sobre sus propios documentos; no hay objetos de negocio compartidos ni cambios que requieran trazabilidad antes/después.
- Guardar, Guardar como, abrir y descartar se diagnostican en `LOG-004`–`LOG-008`; el contenido y las rutas no se registran por privacidad (NFR-010).
- La copia de recuperación y la lista de recientes son estado local operativo, no un historial funcional; su ciclo se especifica en data-structures y security-permissions.
- Separación con logs.md: el diagnóstico técnico vive allí; por la regla anti-sobre-registro, no corresponde un audit de F-002.
