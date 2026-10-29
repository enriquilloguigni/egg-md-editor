# F-003 — Doble vista sincronizada

## Objetivo

Que el usuario pueda editar un mismo documento en dos paneles simultáneos —el izquierdo en modo WYSIWYG puro, sin sintaxis visible, y el derecho en Markdown estándar editable— sincronizados en tiempo real, con correspondencia de posición entre ambas vistas y un resaltado pasajero al pasar de una a otra; y que pueda mostrar u ocultar cada panel y la barra de formato según su preferencia de trabajo.

## Prioridad

Prioridad: P1 — must-have del MVP: sin doble vista no hay edición sincronizada que el resto de Features consuma.

## Verificación independiente

Sí — la sincronización entre dos paneles sobre un mismo documento se verifica sin depender de formato completo, edición avanzada ni exportación.

## Referencias Project Spec

- `FR-001` — doble vista sincronizada.
- `BR-001` — un único documento por instancia.
- `BR-002` — dos paneles reflejan el mismo documento en tiempo real; sin botón actualizar.
- `BR-003` — panel izquierdo WYSIWYG puro (sin sintaxis visible), derecho Markdown estándar editable.
- `BR-012` — aplicación 100 % local y offline.
- `BR-014` — resaltado de correspondencia pasajero, no modifica contenido.
- `NFR-001` — cambios reflejados en <200 ms hasta 50 000 palabras.
- `NFR-003` — hasta 100 000 palabras / 10 000 líneas sin degradación perceptible >1 s.
- `NFR-009` — accesibilidad WCAG 2.1 AA.
- `NFR-019` — acciones de menú/barra/atajos responden <100 ms.

## Dependencias

- `F-002` — Gestión de archivo: creación, apertura, guardado y persistencia del documento único que ambos paneles editan; única dependencia directa (F-001 es transitiva a través de F-002).

## Estado

Planificada — Plan v1 propuesto (pendiente de PLAN_REVIEW).