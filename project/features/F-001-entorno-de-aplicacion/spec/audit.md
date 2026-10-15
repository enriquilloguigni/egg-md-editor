# Audit — F-001 Entorno de aplicación

No requerido / Fuera de alcance — F-001 no genera eventos de auditoría: ninguna operación de la Feature modifica información con trazabilidad funcional significativa.

- Un único actor local sin autenticación ni roles diferenciados (security-permissions.md): el «quién» es siempre el mismo, no hay responsabilidad que reconstruir.
- El área de documento está vacía, las opciones de menú y de la barra están visibles sin acción conectada (ADR-001) y nada se escribe a disco (C-001): ningún objeto sufre un cambio de estado que exija registrar antes/después.
- La conmutación de idioma es/en (`COM-001`) es estado efímero de sesión: se libera al cerrar `SC-001` y no se persiste (FR-013 → F-030); su relevancia técnica ya la cubre `LOG-002`, sin valor de trazabilidad de usuario.
- Separación con `logs.md`: el diagnóstico de arranque, conmutación y cierre vive en `LOG-001`–`LOG-003`; este documento solo cubriría quién/qué/antes-después y, por la regla anti-sobre-registro, no corresponde en esta Feature.
