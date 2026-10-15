# Screens — F-001 Entorno de aplicación

SC-001 — Ventana principal

Propósito: presentar en la ventana única la forma completa de la aplicación —menú principal y barra de formato con el 100 % de sus opciones visibles— junto a la interfaz conmutable es/en y los atajos activos (BR-003, BR-007, FR-010, FR-011 y FR-012 solo en su parte visual; ADR-001).

Elementos de la interfaz:

```
Barra de título — nombre de la aplicación; es la única ventana de la aplicación (BR-003).

Barra de menú persistente, siempre visible, con el 100 % de los elementos de FR-011 (solo visibles; ADR-001):
- Archivo: Nuevo · Abrir · Guardar · Guardar como · Recientes · Autoguardado · Exportar a PDF
- Edición: Deshacer · Rehacer · Cortar · Copiar · Pegar · Seleccionar todo · Buscar · Reemplazar · Ir a línea
- Ver: Mostrar/ocultar vistas (WYSIWYG y Markdown) · Mostrar/ocultar barra de formato · Plegado de secciones de la vista Markdown · Zoom con restablecer · Ajuste de línea · Desplazamiento sincronizado · Pantalla completa · Modo foco · Tema claro/oscuro · Fuente del Markdown
- Formato: Énfasis (negrita, cursiva, tachado) · Encabezados 1–6 con promover/degradar · Listas (viñetas, numeradas, tareas) · Citas anidadas · Código en línea y bloque · Enlaces · Imágenes · Tablas · Notas al pie · Fórmulas (en línea y en bloque) · Diagramas · Escape de sintaxis · Abrazo de delimitadores · Alineación de columnas · TAB en tablas · Anclas internas
- Herramientas: Idioma de la interfaz (Español | English) · Contador de palabras y caracteres · Revisión ortográfica
- Ayuda: Guía de Markdown · Atajos de teclado · Acerca de
El menú contextual de Edición no forma parte de la barra de menú; F-001 no lo muestra (el alcance visual de ADR-001 es menú principal y barra de formato).

Barra de formato visible, con el 100 % de los controles de FR-012 en orden de escritura; un icono y un aviso flotante por control:
Énfasis (negrita, cursiva, tachado) · Encabezados 1–6 con desplegable y promover/degradar · Listas · Cita · Código en línea y bloque · Enlace · Imagen · Tabla · Nota al pie · Fórmula · Diagrama · Índice

Área de documento — vacía en F-001: sin vistas ni paneles (la doble vista WYSIWYG/Markdown es FR-001, entregada en F-003) ni contenido de documento (FR-004, entregado en F-002); los avisos flotantes se pintan sobre ella.
```

Campos: sin campos de formulario. Única entrada con valores — Idioma de la interfaz: tipo: selección en menú ·
requerido: sí (siempre hay un idioma activo) · valores: Español, English · defecto: Español · validación:
mutuamente excluyentes, siempre una marcada · la elección no se persiste en F-001 (FR-013 → F-030).

Acciones disponibles en F-001:
- Conmutar el idioma de la interfaz entre español e inglés.
- Abrir y cerrar cada entrada del menú, y mostrar el aviso flotante de cada control al pasar el puntero.
- Cerrar la ventana e invocar los atajos de teclado de las acciones disponibles en F-001.
- Las demás opciones quedan visibles sin acción conectada: pulsarlas no altera el documento (ADR-001).

Mensajes relevantes:
- Rótulos de menú, de elementos y avisos flotantes en el idioma activo, sobre una base i18n es/en
  extensible (BR-007, NFR-006).
- Un aviso flotante por control de la barra de formato (FR-012).
- Sin mensajes propios en F-001: fallos de arranque y de pintado se describen en error-handling.md.

Comportamiento dinámico:
- Relabelizado en caliente: al conmutar el idioma, todos los rótulos y avisos cambian sin reinicio,
  conservando ventana y estado (BR-007).
- Estados visibles: al arrancar, la interfaz está en español (idioma por defecto de la sesión) o ya en
  inglés; el menú aparece cerrado o desplegado con una entrada activa; el aviso flotante, visible u oculto.
- Opciones sin acción conectada: abren y cierran el menú o muestran su aviso, sin efecto sobre el
  documento (ADR-001).
