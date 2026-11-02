# Audit — F-004 Formato Markdown completo

No requerido / Fuera de alcance — F-004 no genera eventos de auditoría funcional.

- Un único actor local sin autenticación ni roles (security-permissions.md): el «quién» es siempre la cuenta del SO y no hay responsabilidad que reconstruir.
- Las ediciones de formato son contenido del documento único (FR-002): aplicar énfasis, encabezados, listas, citas, código, enlaces, imágenes, tablas, notas al pie, fórmulas y diagramas no crea un objeto de negocio distinto ni un cambio de estado con antes/después significativo.
- El diagnóstico técnico de parseo, renderizado, sanitización y degradación vive en `LOG-012`–`LOG-016` y en error-handling.md; el contenido y las fuentes de los bloques no se registran por privacidad (NFR-010).
- La exportación a PDF y otras salidas no pertenecen a F-004 y, de existir, se tratarían en su propia Feature.
- Separación con logs.md: el diagnóstico técnico vive allí; por la regla anti-sobre-registro, no corresponde un audit de F-004.