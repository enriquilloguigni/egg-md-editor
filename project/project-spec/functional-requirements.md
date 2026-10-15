# Functional Requirements — egg-md-editor

FR-001 — Doble vista sincronizada

El sistema debe presentar el mismo documento en una vista WYSIWYG y una vista Markdown estándar, ambas editables y sincronizadas en tiempo real, con correspondencia de posición entre ambas vistas; al navegar o mapear entre vistas, la sección activa equivalente queda resaltada pasajeramente en la vista destino, orientando sin interferir.

FR-002 — Formato Markdown completo

El sistema debe soportar énfasis, encabezados 1–6 con promover/degradar, listas de viñetas, numeradas y de tareas con comportamiento al escribir, citas anidadas, código en línea y bloque, enlaces e imágenes, tablas editables, notas al pie, fórmulas en línea y en bloque, y diagramas; conforme a CommonMark, con tareas y tachado según GFM; además, escape de sintaxis con barra invertida, abrazo de delimitadores sobre la selección, alineación de columnas y navegación con TAB en tablas, y anclas internas para enlazar secciones del propio documento.

FR-003 — Índice e inserción rápida

El sistema debe generar un índice automático desde encabezados H1–H6 y permitir inserción rápida basada en plantillas.

FR-004 — Gestión de archivo

El sistema debe abrir, guardar y guardar como documentos Markdown, aceptar archivos .md por arrastre, mantener una lista de archivos recientes, autoguardar el documento de forma silenciosa y avisar de cambios sin guardar al crear un documento nuevo; la persistencia será UTF-8 con finales de línea consistentes por sistema; la lista de recientes se limita a 10 archivos, ordenada por uso y abrible con un clic.

FR-005 — Exportación a PDF

El sistema debe exportar el documento a PDF fiel a la vista WYSIWYG, permitiendo elegir carpeta, nombre y opciones de exportación.

FR-006 — Metadatos del documento

El sistema debe mantener un bloque de metadatos delimitado al inicio del documento, visible como ficha y editable como Markdown, con edición guiada de propiedades; el idioma declarado en él regirá la corrección ortográfica.

FR-007 — Edición

El sistema debe ofrecer deshacer/rehacer, cortar/copiar/pegar (texto formateado o Markdown), selección avanzada (doble y triple clic, extensión con Mayús), buscar/reemplazar con opciones e ir a línea; además, seleccionar todo, indicador visible de línea y columna, resaltado global de coincidencias de la búsqueda y menú contextual como vehículo de las acciones de edición en ambas vistas.

FR-008 — Revisión ortográfica

El sistema debe realizar revisión ortográfica completa en ambas vistas según el idioma del documento, con sugerencias y diccionario personal ampliable.

FR-009 — Presentación y navegación

El sistema debe permitir mostrar/ocultar vistas y barra de formato, zoom con restablecimiento, ajuste de línea, desplazamiento sincronizado, pantalla completa, modo foco, tema claro/oscuro, elección de fuente del Markdown y contador de palabras y caracteres; incluye el plegado de secciones de la vista Markdown y que el contador opere también sobre la selección.

FR-010 — Localización y ayuda

El sistema debe ofrecer la interfaz en español e inglés sobre una arquitectura i18n extensible, guía integrada de Markdown, listado de atajos y ventana Acerca de con versión y atribuciones.

FR-011 — Menú principal persistente

El sistema debe ofrecer un menú principal persistente con Archivo, Edición, Ver, Formato, Herramientas y Ayuda, agrupando bajo cada entrada los elementos que el alcance declara: Archivo —nuevo, abrir (también por arrastre), guardar, guardar como, recientes, autoguardado y exportar a PDF—; Edición —las acciones de FR-007, con el menú contextual incluido—; Ver —las opciones de FR-009—; Formato —el conjunto de FR-002—; Herramientas —contador de palabras/caracteres y revisión ortográfica—; Ayuda —guía de Markdown, atajos y Acerca de—; alcance completo sin recortes.

FR-012 — Barra de formato

El sistema debe ofrecer una barra de formato con un icono y un aviso flotante por control y doble naturaleza: con el foco en la vista WYSIWYG produce el efecto visual y con el foco en la vista Markdown escribe la sintaxis equivalente, actuando sobre la selección cuando la hay y preparando la estructura cuando no; los controles siguen el orden del flujo de escritura —énfasis, encabezados 1–6 con desplegable y promover/degradar, listas, cita, código en línea y bloque, enlace, imagen, tabla, nota al pie, fórmula, diagrama e índice—, cubriendo el conjunto de FR-002 y FR-003.

FR-013 — Persistencia de preferencias

El sistema debe persistir las preferencias del usuario entre sesiones: tema, zoom, visibilidad de paneles y barra de formato, y las demás opciones configurables de la aplicación.
