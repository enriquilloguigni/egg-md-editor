# Roadmap — egg-md-editor

## v1

Entrega única con el alcance completo de v1; este Roadmap solo ordena la entrega, sin recortar alcance, y v1 no depende de ninguna versión anterior.

Valor utilizable: F-010 — el usuario redacta, formatea, revisa y exporta un documento completo y personaliza la vista, la ayuda y las preferencias.

| id | name | objective | dependencies | status |
|---|---|---|---|---|
| F-001 | Entorno de aplicación | Abrir una única ventana con menús persistentes, barra de formato, atajos, i18n español/inglés y tema, mostrando las opciones de menú y barra (FR-010, FR-011, FR-012 parte visual). | — | planned |
| F-002 | Gestión de archivo | Crear, abrir, guardar, guardar como, arrastrar .md, recientes, autoguardado y aviso de cambios sin guardar en UTF-8 (FR-004). | F-001 | planned |
| F-003 | Doble vista sincronizada | Editar el mismo documento en WYSIWYG y Markdown con correspondencia de posición, sincronía en tiempo real y resaltado pasajero (FR-001, NFR-001). | F-002 | planned |
| F-004 | Formato Markdown completo | Aplicar énfasis, encabezados 1–6, listas, citas, código, enlaces, imágenes, tablas, notas, fórmulas y diagramas conforme a CommonMark/GFM (FR-002). | F-003 | planned |
| F-005 | Edición y búsqueda | Deshacer/rehacer, cortar/copiar/pegar, selección, buscar/reemplazar, ir a línea y menú contextual en ambas vistas (FR-007). | F-003 | planned |
| F-006 | Índice e inserción rápida | Generar índice desde H1–H6 y insertar bloques frecuentes por plantillas (FR-003). | F-004 | planned |
| F-007 | Metadatos del documento | Mantener el bloque delimitado al inicio como ficha y Markdown, con propiedades guiadas e idioma que rige la ortografía (FR-006). | F-002 | planned |
| F-008 | Revisión ortográfica | Revisar en ambas vistas según el idioma, con sugerencias y diccionario personal (FR-008). | F-003, F-007 | planned |
| F-009 | Exportación a PDF | Exportar a PDF fiel a la vista WYSIWYG, eligiendo carpeta, nombre y opciones (FR-005). | F-004 | planned |
| F-010 | Presentación, ayuda y preferencias | Mostrar/ocultar vistas y barra, zoom, ajuste, scroll sincronizado, pantalla completa, foco, tema, fuente, plegado, contadores, ayuda/atribuciones y persistencia de preferencias (FR-009, FR-010, FR-013). | F-001, F-003 | planned |