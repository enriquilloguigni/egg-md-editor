# Functional Requirements — egg-md-editor

FR-001 — Doble vista sincronizada
El sistema debe presentar el mismo documento en una vista WYSIWYG y una vista Markdown estándar, ambas editables y sincronizadas en tiempo real, con correspondencia de posición entre ambas vistas.

FR-002 — Formato Markdown completo
El sistema debe soportar énfasis, encabezados 1–6 con promover/degradar, listas de viñetas, numeradas y de tareas con comportamiento al escribir, citas anidadas, código en línea y bloque, enlaces e imágenes, tablas editables, notas al pie, fórmulas en línea y en bloque, y diagramas; conforme a CommonMark, con tareas y tachado según GFM.

FR-003 — Índice e inserción rápida
El sistema debe generar un índice automático desde encabezados H1–H6 y permitir inserción rápida basada en plantillas.

FR-004 — Gestión de archivo
El sistema debe abrir, guardar y guardar como documentos Markdown, aceptar archivos .md por arrastre, mantener una lista de archivos recientes, autoguardar el documento de forma silenciosa y avisar de cambios sin guardar al crear un documento nuevo; la persistencia será UTF-8 con finales de línea consistentes por sistema.

FR-005 — Exportación a PDF
El sistema debe exportar el documento a PDF fiel a la vista WYSIWYG, permitiendo elegir carpeta, nombre y opciones de exportación.

FR-006 — Metadatos del documento
El sistema debe mantener un bloque de metadatos delimitado al inicio del documento, visible como ficha y editable como Markdown, con edición guiada de propiedades; el idioma declarado en él regirá la corrección ortográfica.

FR-007 — Edición
El sistema debe ofrecer deshacer/rehacer, cortar/copiar/pegar (texto formateado o Markdown), selección avanzada (doble y triple clic, extensión con Mayús), buscar/reemplazar con opciones e ir a línea.

FR-008 — Revisión ortográfica
El sistema debe realizar revisión ortográfica completa en ambas vistas según el idioma del documento, con sugerencias y diccionario personal ampliable.

FR-009 — Presentación y navegación
El sistema debe permitir mostrar/ocultar vistas y barra de formato, zoom con restablecimiento, ajuste de línea, desplazamiento sincronizado, pantalla completa, modo foco, tema claro/oscuro, elección de fuente del Markdown y contador de palabras y caracteres.

FR-010 — Localización y ayuda
El sistema debe ofrecer la interfaz en español e inglés sobre una arquitectura i18n extensible, guía integrada de Markdown, listado de atajos y ventana Acerca de con versión y atribuciones.
