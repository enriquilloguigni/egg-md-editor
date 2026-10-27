# Logs — F-002 Gestión de archivo

Regla anti-sobre-registro:
- Solo las operaciones de archivo relevantes para diagnosticar generan `LOG-xxx`: abrir, guardar/guardar como, autoguardar, recuperar y descartar cambios. Nuevo, cancelaciones, navegación de menús y recientes sin error no se registran.
- La trazabilidad de usuario antes/después vive en `audit.md` (no requerida en F-002) y los errores de negocio en `error-handling.md`; este documento solo define el registro técnico que dichos fallos enlazan con `Log: Requerido. Referencia: LOG-xxx`.
- Correlación: sesión + identificador de documento + identificador de operación; marca de tiempo ISO 8601; registro local en la infraestructura de F-001 (salida estándar del proceso), sin salida remota (NFR-010).
- Privacidad: no se registra contenido, nombre de archivo, ruta completa ni pulsaciones; solo extensión, tamaño, codificación, fin de línea, resultado y duración.

LOG-004 — Abrir archivo

- **Registro:** inicio y fin de la lectura/decodificación · **resultado:** éxito o fallo con tipo/código · **nivel:** `Info` en éxito, `Warning` en validación rechazada, `Error` en fallo de E/S o decodificación.
- **Datos:** hora · sesión · documento · operación (Abrir/Reciente/Arrastrar) · extensión · bytes leídos · codificación detectada · fin de línea detectado · duración ms.
- **No debe registrarse:** contenido, nombre/ruta completa, metadatos del documento, rutas de recientes.

LOG-005 — Guardar / Guardar como

- **Registro:** inicio y fin de la escritura · **resultado:** éxito o fallo con tipo/código · **nivel:** `Info` en éxito, `Error` en fallo.
- **Datos:** hora · sesión · documento · modo (Guardar/Guardar como) · extensión · bytes escritos · fin de línea aplicado · duración ms · ¿cambió la ruta? (sí/no).
- **No debe registrarse:** contenido, nombre/ruta completa, bloque de metadatos, copia de recuperación.

LOG-006 — Autoguardado

- **Registro:** fin de cada ciclo · **resultado:** copia escrita, omitida por no haber cambios o fallo · **nivel:** `Debug` en éxito/omisión, `Warning` en fallo.
- **Datos:** hora · sesión · documento · duración ms · tamaño de la copia · estado {Vigente,Obsoleta}.
- **No debe registrarse:** contenido, ruta, cada tick sin cambios como evento `Info`.

LOG-007 — Recuperación al iniciar

- **Registro:** consulta al arrancar y fin de la restauración · **resultado:** restaurada, no había copia, copia obsoleta o fallo · **nivel:** `Info` si restaura, `Debug` si no hay copia, `Warning`/`Error` si falla.
- **Datos:** hora · sesión · documento · marca de tiempo de la copia · duración ms.
- **No debe registrarse:** contenido, ruta original, nombre del archivo.

LOG-008 — Descarte de cambios

- **Registro:** confirmación de «No guardar» antes de reemplazar o cerrar · **resultado:** descarte confirmado y copia invalidada · **nivel:** `Info`.
- **Datos:** hora · sesión · documento · acción pendiente (Nuevo/Abrir/Reciente/Arrastrar/Cerrar).
- **No debe registrarse:** contenido, nombre/ruta, cantidad de cambios.

Sin LOG — `No requerido / Fuera de alcance:` Nuevo sin cambios, cancelaciones de diálogo, actualización de la lista de recientes en memoria y navegación de menús; no son diagnosticables y no alteran archivos. Los fallos de persistencia de recientes o de invalidación de la recuperación se registran como `Warning` dentro de `LOG-004`, `LOG-005` o `LOG-008` (ERR-015).
