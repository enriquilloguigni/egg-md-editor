# Business Rules — F-004 Formato Markdown completo

Referencia (heredada, no reproducida): reglas generales en Project Spec vigente.

Referencias heredadas:
- `FR-002` — formato Markdown completo.
- `BR-004` — exportación PDF reproduce el panel izquierdo.
- `BR-009` — CommonMark + GFM; escape con barra invertida.
- `BR-011` — fórmulas y diagramas renderizados en el panel izquierdo.
- `BR-012` — 100 % local y offline.
- `BR-016` — la barra actúa según el panel activo.
- `BR-018` — enlaces internos resueltos por encabezado.
- `NFR-013`, `NFR-016`, `NFR-017` — renderizado, límites de tabla y sanitización.

```
| id | condición | regla | consecuencia |
|---|---|---|---|
| BR-F004-001 | Énfasis | Negrita, cursiva y tachado se aplican sobre la selección; sin selección se preparan los marcadores correspondientes | Formato reversible en ambos paneles (BR-009, BR-016) |
| BR-F004-002 | Encabezados | Niveles 1–6 con promover/degradar dentro de rango; no se exige un H1 único por documento; los enlaces internos se resuelven por el ancla del encabezado | Estructura válida CommonMark (BR-009, BR-018) |
| BR-F004-003 | Listas | Viñetas, numeradas y de tareas con anidado arbitrario; las listas numeradas se renumeran automáticamente | Numeración coherente sin intervención (BR-009) |
| BR-F004-004 | Citas | Citas anidadas por niveles | Jerarquía de cita preservada (BR-009) |
| BR-F004-005 | Código | Código en línea y en bloque, con lenguaje opcional | Resaltado según lenguaje (BR-009) |
| BR-F004-006 | Enlaces | Internos resuelven por encabezado de destino; externos requieren confirmación explícita antes de abrirse | Sin ejecución de enlaces peligrosos (NFR-017) |
| BR-F004-007 | Imágenes | Texto alternativo obligatorio; solo rutas locales, sin descarga remota | Accesibilidad y privacidad (NFR-009, BR-012) |
| BR-F004-008 | Tablas | Filas, columnas y alineación por columna dentro de los límites (1000×50); por encima se informa y degrada sin bloquear la edición | Sin bloqueo ante tablas grandes (NFR-016, ERR-021) |
| BR-F004-009 | Notas al pie | Identificador único por documento más texto | Referencia estable y no ambigua (BR-009) |
| BR-F004-010 | Fórmulas | LaTeX en línea o en bloque, renderizado en el panel izquierdo; un fallo deja marcador sin alterar el Markdown | Contenido intacto ante error de render (BR-011, ERR-019) |
| BR-F004-011 | Diagramas | Mermaid en bloque, renderizado en el panel izquierdo; un fallo deja marcador sin alterar el Markdown | Contenido intacto ante error de render (BR-011, ERR-019) |
| BR-F004-012 | Escape | Barra invertida escapa caracteres literales en el panel Markdown | Sintaxis literal preservada (BR-009) |
| BR-F004-013 | Panel activo | La barra de formato actúa sobre el panel activo: efecto visual en el izquierdo, sintaxis equivalente en el derecho | Un solo comando, resultado según vista (BR-016, COM-021) |
```