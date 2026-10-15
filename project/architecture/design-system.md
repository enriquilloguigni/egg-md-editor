# Design System — egg-md-editor

## Superficies

- ventana-principal: ventana única con menús, barra de formato y los dos paneles del mismo documento (WYSIWYG a la izquierda, Markdown a la derecha).
- diálogo: modales sobre la ventana principal (exportar a PDF, propiedades del documento, buscar/reemplazar).
- ayuda: ventana Acerca de y guía integrada de Markdown.

Comando de captura (ventana-principal): `sdd-visual-capture ventana-principal`
Comando de captura (diálogo): `sdd-visual-capture diálogo`
Comando de captura (ayuda): `sdd-visual-capture ayuda`

## Patrones por superficie

- ventana-principal: barra de formato con iconos y tooltips, paneles con scroll propio y sincronizado, resaltado pasajero de la correspondencia de posición.
- diálogo: formulario con acciones primaria/secundaria y confirmación al cerrar con cambios sin guardar.
- ayuda: lectura fluida con navegación por secciones y texto seleccionable.

## Tokens

- TK-001: paleta claro/oscuro con contraste AA (fondo, texto, acento, selección, resaltado).
- TK-002: tipografía de la UI (fuente de interfaz y escala de tamaños base).
- TK-003: fuente y tamaño de los paneles WYSIWYG y Markdown, con zoom de documento independiente del zoom de UI.
- TK-004: retícula de controles (altura de la barra, iconos, espaciado) y estados de foco, selección y error.
