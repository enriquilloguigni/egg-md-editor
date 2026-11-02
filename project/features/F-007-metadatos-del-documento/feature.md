# F-007 — Metadatos del documento

## Objetivo

Que el usuario pueda mantener un bloque de metadatos delimitado al inicio del documento, visible como ficha en la vista WYSIWYG y editable como Markdown en la vista Markdown, con edición guiada de propiedades (título, autor, fecha, idioma y campos personalizados), de modo que el idioma declarado en el bloque gobierne la corrección ortográfica sin configuración adicional.

## Prioridad

Prioridad: P1 — must-have del MVP: los metadatos identifican el documento y gobiernan la ortografía (F-008 depende de esta Feature).

## Verificación independiente

Sí — el bloque de metadatos, su presentación como ficha y la edición guiada se verifican sobre un documento Markdown sin depender de la exportación a PDF ni de la revisión ortográfica.

## Referencias Project Spec

- `FR-006` — bloque de metadatos delimitado al inicio, visible como ficha y editable como Markdown, con edición guiada de propiedades (título, autor, fecha, idioma y campos personalizados); el idioma declarado rige la corrección ortográfica.
- `BR-005` — esquema fijo (título, autor, fecha, idioma) más campos personalizados opcionales; idioma declarado gobierna la ortografía; bloque editable a mano en el panel derecho y ficha en el izquierdo.
- `BR-007` — lectura y escritura en UTF-8 con finales de línea normalizados según el sistema operativo; el bloque se conserva.
- `BR-012` — aplicación 100 % local y offline.
- `NFR-004` — funcionamiento sin red ni servicios externos.
- `NFR-005` — ningún dato del usuario sale del equipo.
- `NFR-007` — lógica de metadatos con cobertura unitaria ≥ 70 % y desacoplada de la interfaz.
- `NFR-008` — Windows 10+, macOS 12+ y Linux soportados por .NET 10.
- `NFR-009` — interfaz y diálogos conformes con WCAG 2.1 AA.
- `NFR-011` — dependencias MIT-compatibles y atribuciones de terceros.
- `NFR-018` — soporte Unicode e internacionalización.
- `C-005` — metadatos en bloque delimitado al inicio, esquema fijo + campos personalizados opcionales.

## Dependencias

- `F-002` — Gestión de archivo: conserva el bloque de metadatos opaco al abrir y guardar y proporciona el documento único sobre el que opera esta Feature; única dependencia directa.

## Estado

planned