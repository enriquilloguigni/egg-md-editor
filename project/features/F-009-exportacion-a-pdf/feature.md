# F-009 — Exportación a PDF

## Objetivo

Que el usuario pueda exportar el documento a PDF fiel a la vista WYSIWYG, eligiendo carpeta, nombre y opciones de exportación (márgenes, orientación, encabezados y pies), con texto seleccionable y metadatos básicos (título, autor), generando un PDF legible por lectores estándar en menos de 10 s para documentos de hasta 50 páginas.

## Prioridad

Prioridad: P1 — must-have del MVP: la exportación a PDF es parte del alcance central de v1.

## Verificación independiente

Sí — la exportación se verifica generando el PDF a partir del contenido ya renderizado en el panel WYSIWYG sin depender de edición avanzada, metadatos ni ortografía.

## Referencias Project Spec

- `FR-005` — Exportación a PDF.
- `BR-004` — la exportación a PDF reproduce fielmente el panel izquierdo (encabezados, listas, tablas, código, notas, fórmulas, diagramas).
- `BR-011` — las fórmulas (LaTeX) y los diagramas (Mermaid) se renderizan en el panel izquierdo y se incluyen en la exportación a PDF.
- `BR-018` — los enlaces a secciones del mismo documento se resuelven por el encabezado de destino y el salto funciona en el PDF exportado.
- `BR-021` — la exportación a PDF se configura antes de generar: el usuario elige carpeta, nombre y opciones (márgenes, orientación, encabezados y pies).
- `BR-012` — la aplicación es 100 % local y offline.
- `NFR-002` — la exportación a PDF de un documento de hasta 50 páginas debe completarse en menos de 10 s.
- `NFR-005` — ningún dato del usuario sale del equipo.
- `NFR-007` — cobertura unitaria ≥ 70 %, lógica desacoplada.
- `NFR-008` — Windows 10+, macOS 12+ y Linux soportados por .NET 10.
- `NFR-009` — interfaz conforme con WCAG 2.1 AA.
- `NFR-011` — dependencias compatibles con MIT y atribuciones de terceros.
- `NFR-021` — el PDF generado debe ser legible por lectores estándar, con texto seleccionable y metadatos básicos (título, autor).

## Dependencias

- `F-004` — Formato Markdown completo: el PDF reproduce fielmente el panel WYSIWYG con encabezados, listas, tablas, código, notas, fórmulas y diagramas; única dependencia directa.

## Estado

planned
