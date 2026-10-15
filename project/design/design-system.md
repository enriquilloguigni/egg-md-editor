# Design System — egg-md-editor

## Superficies

- ventana-principal: ventana única con menús, barra de formato y los dos paneles del mismo documento (WYSIWYG a la izquierda, Markdown a la derecha).
- diálogo: modales sobre la ventana principal (exportar a PDF, propiedades del documento, buscar/reemplazar).
- ayuda: ventana Acerca de y guía integrada de Markdown.

Comando de captura (ventana-principal): sdd-visual-capture ventana-principal <SC> <salida>
Comando de captura (diálogo): sdd-visual-capture diálogo <SC> <salida>
Comando de captura (ayuda): sdd-visual-capture ayuda <SC> <salida>

## Patrones por superficie

- ventana-principal: barra de formato con iconos y tooltips, paneles con scroll propio y sincronizado, resaltado pasajero de la correspondencia de posición.
- diálogo: formulario con acciones primaria/secundaria y confirmación al cerrar con cambios sin guardar.
- ayuda: lectura fluida con navegación por secciones y texto seleccionable.

## Tokens (TK-xxx)

- TK-001 — Paleta claro/oscuro (superficie: todas). Valor: fondo, texto, acento, selección y resaltado con contraste AA.
- TK-002 — Tipografía de la UI (superficie: todas). Valor: fuente de interfaz y escala de tamaños base.
- TK-003 — Paneles y zoom (superficie: ventana-principal). Valor: fuente y tamaño de los paneles WYSIWYG y Markdown, con zoom de documento independiente del zoom de UI.
- TK-004 — Retícula de controles y estados (superficie: todas). Valor: altura de la barra, iconos y espaciado; estados de foco, selección y error.

Origen: /sdd-architecture-design-system
