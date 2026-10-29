# Logs — F-003 Doble vista sincronizada

Regla anti-sobre-registro:
- Solo las operaciones relevantes para diagnosticar generan `LOG-xxx`: sincronización de vistas, alternancia de visibilidad y aplicación del resaltado pasajero. Ediciones individuales, desplazamiento y cambios de foco no se registran.
- La trazabilidad funcional vive en `audit.md` (no requerida en F-003) y los errores de negocio en `error-handling.md`; este documento solo define el registro técnico que dichos fallos enlazan con `Log: Requerido. Referencia: LOG-xxx`.
- Correlación: sesión + identificador de documento + identificador de sincronización; marca de tiempo ISO 8601; registro local en la infraestructura de F-001 (salida estándar del proceso), sin salida remota (NFR-010).
- Privacidad: no se registra contenido, nombre de archivo ni ruta completa; solo tamaño del documento, latencia, resultado y duración.

LOG-009 — Sincronización de vistas

- **Registro:** fin de cada sincronización · **resultado:** éxito, degradado o fallo con tipo/código · **nivel:** `Debug` en éxito, `Warning` si supera el umbral, `Error` en fallo.
- **Datos:** hora · sesión · documento · versión del árbol · paneles afectados · latencia ms · palabras del documento · resultado.
- **No debe registrarse:** contenido, nombre/ruta, selección, pulsaciones.

LOG-010 — Alternancia de visibilidad

- **Registro:** fin de cada cambio de visibilidad · **resultado:** aplicado o fallo · **nivel:** `Info` en éxito, `Warning` en fallo.
- **Datos:** hora · sesión · documento · panel/barra afectado · estado anterior/posterior · duración ms.
- **No debe registrarse:** contenido, preferencias completas, nombre/ruta.

LOG-011 — Resaltado pasajero

- **Registro:** aplicación del resaltado · **resultado:** aplicado o sin ancla · **nivel:** `Debug` si se aplica, `Debug` si no hay ancla.
- **Datos:** hora · sesión · documento · panel origen/destino · duración ms · ¿ancla encontrada? (sí/no).
- **No debe registrarse:** contenido, rango textual, nombre/ruta.

Sin LOG — `No requerido / Fuera de alcance:` edición individual, desplazamiento, redimensionado del separador, cambio de foco y renderizado normal; no son diagnosticables ni alteran archivos. Los fallos de persistencia de preferencias se registran como `Warning` dentro de `LOG-010` (ERR-016).