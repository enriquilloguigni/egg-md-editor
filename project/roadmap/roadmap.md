# Roadmap — egg-md-editor

## v1

Entrega única con el alcance completo del requerimiento sin recortes (BR-010): este Roadmap solo ordena la entrega, sin recortar alcance, y v1 no depende de ninguna versión anterior. Las 13 Features se entregan en v1 con la secuencia declarada en la columna `dependencies` (cada Feature solo depende de Features de la misma versión, sin ciclos). Base vigente: stack .NET 10 + Avalonia UI en Windows, macOS y Linux, 100% offline (C-001), licencias compatibles con MIT (C-002) y comando de prueba `dotnet test`.

```markdown
| id | name | objective | dependencies | status |
|---|---|---|---|---|
| F-001 | Entorno de aplicación | Arrancar en una sola ventana con menús, barra de formato y atajos, con la interfaz conmutable entre español e inglés sobre i18n extensible (BR-003, BR-007, FR-010, NFR-006). | — | planned |
| F-002 | Gestión de archivo | Crear, abrir, guardar, guardar como y autoguardar un único documento Markdown UTF-8, con arrastre de .md, lista de recientes y aviso de cambios sin guardar (FR-004, NFR-004). | F-001 | planned |
| F-003 | Doble vista sincronizada | Editar el mismo documento en vista WYSIWYG y vista Markdown estándar con correspondencia de posición y reflejo bidireccional en tiempo real (FR-001, NFR-001). | F-002 | planned |
| F-004 | Formato Markdown completo | Aplicar énfasis, encabezados 1–6 con promover/degradar, listas (viñetas, numeradas y tareas), citas anidadas, código en línea y bloque, enlaces, imágenes, tablas editables, notas al pie, fórmulas y diagramas, conforme a CommonMark/GFM (FR-002). | F-003 | planned |
| F-005 | Edición y búsqueda | Deshacer/rehacer, cortar/copiar/pegar, selección avanzada, buscar/reemplazar con opciones e ir a línea (FR-007). | F-003 | planned |
| F-006 | Índice automático | Generar y navegar un índice desde los encabezados H1–H6 (FR-003). | F-004 | planned |
| F-007 | Inserción rápida | Insertar contenido en el documento a partir de plantillas (FR-003). | F-004 | planned |
| F-008 | Metadatos del documento | Mantener el bloque delimitado al inicio como ficha visible y editable como Markdown, con edición guiada de propiedades y declarando el idioma que rige la ortografía (FR-006, BR-004, BR-005). | F-002 | planned |
| F-009 | Revisión ortográfica | Revisar la ortografía en ambas vistas según el idioma del documento, con sugerencias y diccionario personal por idioma compartido entre documentos (FR-008, BR-006). | F-003, F-008 | planned |
| F-010 | Exportación a PDF | Exportar el documento a PDF fiel a la vista WYSIWYG, eligiendo carpeta, nombre y opciones de exportación (FR-005). | F-004 | planned |
| F-011 | Presentación y navegación | Mostrar/ocultar vistas y barra de formato, zoom con restablecer, ajuste de línea, desplazamiento sincronizado, pantalla completa, modo foco, tema claro/oscuro y elección de fuente del Markdown (FR-009). | F-001, F-003 | planned |
| F-012 | Contadores | Contar palabras y caracteres del documento (FR-009). | F-002 | planned |
| F-013 | Ayuda y atribuciones | Ofrecer guía integrada de Markdown, listado de atajos y ventana Acerca de con versión y atribuciones MIT (FR-010, BR-002, NFR-007). | F-001, F-004 | planned |
```
