# Logs — F-001 Entorno de aplicación

Regla anti-sobre-registro:

- Solo las operaciones relevantes para diagnosticar generan `LOG-xxx`; el resto de eventos internos de `SC-001` (apertura de menús, avisos flotantes, atajos inertes) no se registran.
- La trazabilidad de usuario antes/después vive en `audit.md` y los errores de negocio en `error-handling.md`; este documento solo define el registro técnico que dichos fallos enlazan con `Log: Requerido. Referencia: LOG-xxx`.
- Correlación: proceso con una única sesión y ventana `SC-001`; todo registro comparte el identificador de sesión, lleva marca de tiempo ISO 8601 y sale por la salida estándar del proceso — nada se escribe a disco ni sale del equipo (C-001, NFR-003).

LOG-001 — Arranque y creación de `SC-001`

- **Registro:** inicio del proceso y fin de la inicialización (ventana creada, catálogo i18n cargado, idioma activo) — ambos eventos · **resultado:** éxito o fallo con tipo y mensaje de la excepción · **nivel:** `Info` en éxito, `Error` en fallo (enlazado desde `error-handling.md`).
- **Datos:** hora · identificador de sesión · versión de la aplicación y runtime .NET · idioma activo · tiempo de arranque en ms.
- **No debe registrarse:** telemetría o cualquier salida del equipo (C-001), contenido de documento, pulsaciones de teclado.

LOG-002 — Conmutación de idioma es/en (`COM-001`)

- **Registro:** fin de la operación tras el relabelizado — un registro `Debug` si es exitosa y completa; `Warning` si quedan claves i18n sin resolver (rótulos sin traducir); `Error` si el catálogo del idioma no carga · **resultado:** idioma origen → destino y estado del relabelizado.
- **Datos:** hora · sesión · idioma origen y destino · claves i18n sin resolver (si las hay).
- **No debe registrarse:** el estado antes/después de la UI como trazabilidad de usuario (eso es `audit.md`), ni cada conmutación como evento de usuario en nivel superior a `Debug`.

LOG-003 — Cierre de `SC-001` y fin de sesión (`COM-004`)

- **Registro:** inicio del cierre y fin del proceso — ambos eventos · **resultado:** cierre limpio sin procesos residuales o fallo con tipo y mensaje de la excepción de liberación · **nivel:** `Info` en éxito, `Error` en fallo (enlazado desde `error-handling.md`).
- **Datos:** hora · sesión · duración total de la sesión en ms.
- **No debe registrarse:** la elección de idioma como preferencia (no se persiste en F-001, FR-013 → F-030), estado de menú y avisos, estado antes/después de la sesión.

Sin LOG — `No requerido / Fuera de alcance:` interacciones de menú, barra de formato y atajos (`COM-002`, `COM-003`); no son operaciones diagnosticables y no alteran el documento (ADR-001).
