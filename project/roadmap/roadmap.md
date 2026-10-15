# Roadmap — egg-md-editor

## v1

Entrega única con el alcance completo del requerimiento sin recortes (BR-010): este Roadmap solo ordena la entrega y v1 no depende de ninguna versión anterior.
Las 13 Features se entregan todas en v1 siguiendo la secuencia de la columna `dependencies`, sin ciclos: el entorno de aplicación con menús, barra de formato, atajos e i18n es/en es la base compartida, sobre la que se asientan la gestión de archivo y la doble vista sincronizada.
Después llegan el formato CommonMark/GFM completo y, sobre él, la edición y búsqueda, el índice, la inserción rápida, los metadatos, la revisión ortográfica, la exportación a PDF, la presentación y los contadores; Ayuda y atribuciones cierra la entrega.
Base vigente de Architecture: stack .NET 10 + Avalonia UI en Windows, macOS y Linux (BR-009), 100% offline y sin datos fuera del equipo (C-001, NFR-002, NFR-003), licencias compatibles con MIT (C-002, BR-002), distribución solo-fuente (BR-008) y comando de prueba `dotnet test`.

```markdown
| id | name | objective | dependencies | status |
|---|---|---|---|---|
| F-014 | Entorno de aplicación | Arrancar en una sola ventana con menús (Archivo, Edición, Ver, Formato, Herramientas, Ayuda), barra de formato y atajos sobre comandos compartidos, con la interfaz conmutable entre español e inglés sobre i18n extensible (BR-003, BR-007, FR-010, NFR-006). | — | planned |
| F-015 | Gestión de archivo | Crear, abrir, guardar, guardar como y autoguardar el único documento Markdown UTF-8, con arrastre de .md, lista de recientes y aviso de cambios sin guardar, preservando el contenido sin pérdida y con finales de línea consistentes por SO (FR-004, NFR-004, BR-004). | F-014 | planned |
| F-016 | Doble vista sincronizada | Editar el mismo documento en la vista WYSIWYG y la vista Markdown estándar con correspondencia de posición y reflejo bidireccional en <500 ms p95 sobre el hardware base (FR-001, NFR-001). | F-014 | planned |
| F-017 | Formato Markdown completo | Aplicar énfasis, encabezados 1–6 con promover/degradar, listas (viñetas, numeradas y tareas), citas anidadas, código en línea y bloque, enlaces, imágenes, tablas editables, notas al pie, fórmulas y diagramas en ambas vistas, conforme a CommonMark con tareas y tachado GFM (FR-002, NFR-005). | F-014, F-016 | planned |
| F-018 | Edición y búsqueda | Deshacer/rehacer, cortar/copiar/pegar (texto formateado o Markdown), selección avanzada, buscar/reemplazar con opciones e ir a línea sobre ambas vistas (FR-007). | F-014, F-016 | planned |
| F-019 | Índice automático | Generar y navegar un índice desde los encabezados H1–H6 del documento (FR-003). | F-014, F-017 | planned |
| F-020 | Inserción rápida | Insertar contenido en el documento a partir de plantillas mediante comando (FR-003). | F-014, F-017 | planned |
| F-021 | Metadatos del documento | Mantener el bloque delimitado al inicio del archivo como ficha visible en la vista WYSIWYG y editable como Markdown, con diálogo de propiedades y declarando el idioma que rige la corrección ortográfica (FR-006, BR-004, BR-005). | F-014, F-015, F-016 | planned |
| F-022 | Revisión ortográfica | Revisar la ortografía en ambas vistas según el idioma del documento, con sugerencias, menú contextual y diccionario personal por idioma compartido entre todos los documentos (FR-008, BR-005, BR-006). | F-014, F-016, F-021 | planned |
| F-023 | Exportación a PDF | Exportar el documento a PDF fiel a la vista WYSIWYG, eligiendo carpeta, nombre y opciones de exportación (FR-005). | F-014, F-017 | planned |
| F-024 | Presentación y navegación | Mostrar/ocultar vistas y barra de formato, zoom con restablecer, ajuste de línea, desplazamiento sincronizado, pantalla completa, modo foco, tema claro/oscuro y elección de fuente del Markdown (FR-009). | F-014, F-016 | planned |
| F-025 | Contadores | Contar palabras y caracteres del documento en tiempo real (FR-009). | F-014, F-016 | planned |
| F-026 | Ayuda y atribuciones | Ofrecer guía integrada de Markdown, listado de atajos y ventana Acerca de con versión y atribuciones MIT (FR-010, BR-002, NFR-007). | F-014, F-015, F-017, F-018, F-024 | planned |
```
