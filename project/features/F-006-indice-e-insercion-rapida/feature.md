# F-006 — Índice e inserción rápida

## Objetivo

Que el usuario pueda generar automáticamente un índice a partir de los encabezados H1–H6 del documento, que se actualice al añadir, quitar o modificar encabezados, y pueda insertar rápidamente bloques frecuentes basados en plantillas, operando siempre sobre el documento único.

## Prioridad

Prioridad: P1 — must-have del MVP: el índice y la inserción rápida forman parte del alcance central de v1.

## Verificación independiente

Sí — el índice y la inserción rápida se verifican a partir de encabezados y plantillas sin depender de exportación, metadatos ni ortografía.

## Referencias Project Spec

- `FR-003` — Índice e inserción rápida.
- `BR-010` — el índice se genera automáticamente a partir de los encabezados H1–H6 y se actualiza al cambiarlos.
- `BR-018` — los enlaces a secciones del mismo documento se resuelven por el encabezado de destino.
- `BR-009` — el documento cumple CommonMark + GFM.
- `BR-012` — aplicación 100 % local y offline.
- `BR-001` — un único documento por instancia.
- `NFR-003` — hasta 100 000 palabras / 10 000 líneas.
- `NFR-007` — cobertura unitaria ≥ 70 %, lógica desacoplada.
- `NFR-009` — accesibilidad WCAG 2.1 AA.
- `NFR-015` — regeneración del índice < 500 ms.
- `NFR-019` — respuesta < 100 ms.

## Dependencias

- `F-004` — Formato Markdown completo: el índice se genera a partir de los encabezados H1–H6 y la inserción rápida reutiliza bloques de formato; única dependencia directa (F-003 es transitiva a través de F-004).

## Estado

planned
