# F-004 — Formato Markdown completo

## Objetivo

Que el usuario pueda aplicar, desde la barra de formato o los atajos, el conjunto completo de formato Markdown soportado: énfasis (negrita, cursiva, tachado), encabezados de nivel 1 a 6 con promover/degradar, listas de viñetas, numeradas y de tareas con anidado, citas anidadas, código en línea y en bloque, enlaces internos y externos, imágenes, tablas editables con filas, columnas y alineación, notas al pie, fórmulas en línea y en bloque (LaTeX) y diagramas (Mermaid), además del escape de sintaxis con barra invertida; conforme a CommonMark y GFM. El formato actúa sobre la selección o prepara la estructura correspondiente, sobre el panel activo.

## Prioridad

Prioridad: P1 — must-have del MVP: sin formato completo el editor no cumple su propósito central.

## Verificación independiente

Sí — el soporte de formato se verifica sin depender de exportación, edición avanzada ni metadatos.

## Referencias Project Spec

- `FR-002` — Formato Markdown completo.
- `BR-009` — CommonMark + GFM; la barra invertida escapa caracteres literales.
- `BR-011` — fórmulas (LaTeX) y diagramas (Mermaid) se renderizan en el panel izquierdo.
- `BR-016` — la barra de formato actúa sobre la selección o prepara la estructura.
- `BR-018` — los enlaces a secciones se resuelven por el encabezado de destino.
- `BR-004` — la exportación PDF reproduce fielmente el panel izquierdo.
- `BR-012` — aplicación 100 % local y offline.
- `NFR-003` — hasta 100 000 palabras / 10 000 líneas.
- `NFR-007` — cobertura unitaria ≥ 70 %, lógica desacoplada.
- `NFR-009` — accesibilidad WCAG 2.1 AA.
- `NFR-013` — renderizado de fórmula/diagrama < 1 s.
- `NFR-016` — tablas hasta 1 000 filas y 50 columnas.
- `NFR-017` — sanitización al abrir Markdown.
- `NFR-019` — respuesta < 100 ms.

## Dependencias

- `F-003` — Doble vista sincronizada: ambos paneles editan el mismo documento y el formato actúa sobre el panel activo; única dependencia directa.

## Estado

Planificada — Spec pendiente.
