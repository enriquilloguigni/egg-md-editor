# Functional Requirements

FR-001 — Doble vista sincronizada

El sistema debe presentar el mismo documento en una vista WYSIWYG y una vista Markdown estándar, ambas editables y sincronizadas en tiempo real, con correspondencia de posición y resaltado pasajero de la sección equivalente al navegar entre vistas.

FR-002 — Formato Markdown completo

El sistema debe soportar énfasis, encabezados 1–6 con promover/degradar, listas de viñetas, numeradas y de tareas, citas anidadas, código en línea y bloque, enlaces e imágenes, tablas editables, notas al pie, fórmulas en línea y en bloque, diagramas y escape de sintaxis con barra invertida, conforme a CommonMark y GFM.

FR-003 — Índice e inserción rápida

El sistema debe generar un índice automático a partir de encabezados H1–H6 y permitir inserción rápida de bloques frecuentes basada en plantillas.

FR-004 — Gestión de archivo

El sistema debe crear, abrir, guardar y guardar como documentos Markdown, aceptar archivos por arrastre, mantener una lista de archivos recientes, autoguardar de forma silenciosa y avisar de cambios sin guardar; la persistencia será UTF-8 con finales de línea consistentes por sistema.

FR-005 — Exportación a PDF

El sistema debe exportar el documento a PDF fiel a la vista WYSIWYG, permitiendo elegir carpeta, nombre y opciones de exportación.

FR-006 — Metadatos del documento

El sistema debe mantener un bloque de metadatos delimitado al inicio del documento, visible como ficha y editable como Markdown, con edición guiada de propiedades; el idioma declarado regirá la corrección ortográfica.

FR-007 — Edición

El sistema debe ofrecer deshacer/rehacer, cortar/copiar/pegar, selección avanzada, buscar/reemplazar con opciones, ir a línea con indicador de posición, seleccionar todo y menú contextual en ambas vistas.

FR-008 — Revisión ortográfica

El sistema debe realizar revisión ortográfica en ambas vistas según el idioma del documento, con sugerencias y diccionario personal ampliable.

FR-009 — Presentación y navegación

El sistema debe permitir mostrar/ocultar vistas y barra de formato, zoom con restablecimiento, ajuste de línea, desplazamiento sincronizado, pantalla completa, modo foco, tema claro/oscuro, elección de fuente del Markdown, plegado de secciones y contador de palabras y caracteres.

FR-010 — Localización y ayuda

El sistema debe ofrecer la interfaz en español e inglés, guía integrada de Markdown, listado de atajos y ventana Acerca de con versión y atribuciones.

FR-011 — Menú principal

El sistema debe ofrecer un menú principal persistente con Archivo, Edición, Ver, Formato, Herramientas y Ayuda, agrupando las capacidades declaradas en el alcance.

FR-012 — Barra de formato

El sistema debe ofrecer una barra de formato con icono y aviso flotante por control, de doble naturaleza según la vista activa, actuando sobre la selección o preparando la estructura, en el flujo de escritura declarado.

FR-013 — Persistencia de preferencias

El sistema debe persistir entre sesiones las preferencias del usuario: tema, zoom, visibilidad de paneles y barra de formato y demás opciones configurables.

## Fidelidad fuente

| unidad | cita | FR |
|---|---|---|
| U-01 (L1) | "menús de siempre —Archivo, Edición, Ver, Formato, Herramientas, Ayuda—" | FR-011, FR-012 |
| U-02 (L3) | "dos paneles que ocupan el resto del espacio... dos vistas del mismo documento, sincronizadas en tiempo real" | FR-001 |
| U-03 (L5) | "El panel izquierdo es el pensado para escribir con comodidad... editor WYSIWYG puro" | FR-001 |
| U-04 (L7) | "El panel derecho... muestra el documento en Markdown estándar" | FR-001, FR-002 |
| U-05 (L9) | "sincronización constante... correspondencia no es solo del contenido: también lo es de la posición" | FR-001, FR-009, FR-013 |
| U-06 (L11) | "En Archivo empiezas con Nuevo..." | FR-004, FR-005 |
| U-07 (L13) | "metadatos... bloque delimitado que abre el documento" | FR-006 |
| U-08 (L15) | "En Edición está lo que tus manos ya saben hacer" | FR-007 |
| U-09 (L17) | "menú contextual de edición" | FR-007, FR-008 |
| U-10 (L19) | "En Formato está el corazón" | FR-002 |
| U-11 (L21) | "Seguir con Formato es seguir insertando" | FR-002, FR-003 |
| U-12 (L23) | "sección de elementos rápidos... Índice se genera solo" | FR-003 |
| U-13 (L25) | "En Herramientas llevas la cuenta y el cuidado" | FR-008, FR-009 |
| U-14 (L27) | "En Ayuda tienes el Acerca de" | FR-010 |
| U-15 (L29) | "eliges cómo trabajar en cada momento" | FR-001 |

Cobertura inversa (referencias):

| FR | Unidades fuente |
|---|---|
| FR-001 | U-02, U-03, U-04, U-05, U-15 |
| FR-002 | U-04, U-10, U-11 |
| FR-003 | U-11, U-12 |
| FR-004 | U-06 |
| FR-005 | U-06 |
| FR-006 | U-07 |
| FR-007 | U-08, U-09 |
| FR-008 | U-09, U-13 |
| FR-009 | U-05, U-13 |
| FR-010 | U-14 |
| FR-011 | U-01 |
| FR-012 | U-01 |
| FR-013 | U-05 |

Cobertura: 15/15 unidades y 13/13 FR.
