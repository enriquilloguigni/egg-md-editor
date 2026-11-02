# F-005 — Edición y búsqueda

## Objetivo

Que el usuario pueda deshacer y rehacer, cortar, copiar y pegar, seleccionar con precisión, buscar y reemplazar con opciones, ir a una línea con indicador de posición, seleccionar todo y usar el menú contextual en ambas vistas, operando siempre sobre el documento único.

## Prioridad

Prioridad: P1 — must-have del MVP: sin edición y búsqueda el editor no es usable.

## Verificación independiente

Sí — la edición y la búsqueda se verifican sin depender de exportación, metadatos ni ortografía.

## Referencias Project Spec

- `FR-007` — Edición.
- `BR-001` — un único documento por instancia.
- `BR-017` — deshacer, rehacer, cortar, copiar y pegar operan sobre el documento único, independientemente del panel activo; el portapapeles entrega texto formateado o Markdown según el destino.
- `BR-012` — aplicación 100 % local y offline.
- `NFR-003` — hasta 100 000 palabras / 10 000 líneas.
- `NFR-007` — cobertura unitaria ≥ 70 %, lógica desacoplada.
- `NFR-009` — accesibilidad WCAG 2.1 AA.
- `NFR-014` — búsqueda y reemplazo < 1 s hasta 100 000 palabras.
- `NFR-019` — respuesta < 100 ms.

## Dependencias

- `F-003` — Doble vista sincronizada: ambas vistas editan el mismo documento y la edición/búsqueda actúa sobre él; única dependencia directa.

## Estado

planned
