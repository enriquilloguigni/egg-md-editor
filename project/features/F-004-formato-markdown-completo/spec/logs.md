# logs.md — F-004 Formato Markdown completo

LOG-012 — Error de parseo CommonMark/GFM
Debe registrarse:
- Fin de la operación · Resultado: fallo · Identificador de documento/solicitud · Posición aproximada (línea/columna) · Tiempo de ejecución · Código de error · Nivel: error
No debe registrarse:
- Contenido del documento · PII · tokens de autenticación
Referencias: FR-002 · NFR-010 · BR-012

LOG-013 — Error de renderizado de fórmula/diagrama
Debe registrarse:
- Fin de la operación · Resultado: fallo · Identificador de documento/solicitud · Tipo de bloque (fórmula/diagrama) · Tiempo de ejecución · Código de error · Nivel: warn/error
No debe registrarse:
- Fuente del bloque · PII · contenido del documento
Referencias: FR-002 · NFR-010

LOG-014 — Fallo de sanitización
Debe registrarse:
- Fin de la operación · Resultado: fallo · Identificador de documento/solicitud · Regla violada · Tiempo de ejecución · Código de error · Nivel: error
No debe registrarse:
- Fragmento bloqueado · PII · contenido del documento
Referencias: FR-002 · NFR-010

LOG-015 — Tabla fuera de límites (NFR-016)
Debe registrarse:
- Fin de la operación · Resultado: degradado · Identificador de documento/solicitud · Dimensiones detectadas (filas/columnas) · Límite aplicado · Nivel: warn
No debe registrarse:
- Contenido de la tabla · PII
Referencias: FR-002 · NFR-016 · NFR-010

LOG-016 — Documento grande con degradación (NFR-003)
Debe registrarse:
- Inicio y fin de la degradación · Resultado: degradado/recuperado · Identificador de documento/solicitud · Tamaño del documento · Funciones deshabilitadas · Tiempo de ejecución · Nivel: warn
No debe registrarse:
- Contenido del documento · PII
Referencias: FR-002 · NFR-003 · NFR-010 · BR-012