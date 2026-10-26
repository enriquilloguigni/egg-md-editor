# Design System — egg-md-editor

## Superficies

- ventana-principal: ventana única de Avalonia (menús, barra de formato, dos paneles WYSIWYG/Markdown e índice).
- diálogo: modales sobre la ventana principal (archivo, exportar a PDF, propiedades, buscar/reemplazar, insertar).
- ayuda: ventana Acerca de y guía integrada de Markdown.

Tecnología común: Avalonia UI sobre .NET 10 (FluentTheme), tema claro/oscuro, accesibilidad WCAG 2.1 AA.

## Identidad y navegación global

- Menú principal persistente Archivo/Edición/Ver/Formato/Herramientas/Ayuda; barra de formato con iconos y tooltips.
- Un documento por instancia, sin multi-ventana. Navegación por teclado y lectores de pantalla (Avalonia).
- Idioma de UI español/inglés (default español); tema claro/oscuro heredado del SO si no hay preferencia.

## Patrones por superficie

- ventana-principal: barra de formato con iconos y tooltips; paneles con scroll propio y sincronizado; resaltado pasajero de correspondencia de posición; modo foco atenúa el resto.
- diálogo: formulario con acciones primaria/secundaria; confirmación al cerrar con cambios sin guardar; validación inline.
- ayuda: lectura fluida con navegación por secciones y texto seleccionable.

## Tokens (TK-xxx)

TK-001 — Paleta de color (superficie: todas). Valor: roles semánticos claro/oscuro (fondo, texto, acento, selección, resaltado, error) con contraste WCAG 2.1 AA sobre Avalonia FluentTheme.
TK-002 — Tipografía de interfaz (superficie: todas). Valor: fuente de UI y escala de tamaños base.
TK-003 — Tipografía de documento (superficie: ventana-principal). Valor: fuente y tamaño de los paneles WYSIWYG y Markdown, con zoom de documento independiente del zoom de UI.
TK-004 — Espaciado y retícula (superficie: todas). Valor: escala de espaciado (múltiplos de 4 px), altura de barra de formato, tamaño de iconos y paddings.
TK-005 — Estados de control (superficie: todas). Valor: foco, hover, activo, deshabilitado y error sobre controles interactivos.
TK-006 — Elevación (superficie: diálogo). Valor: sombra y overlay de modales sobre la ventana principal.
TK-007 — Motion (superficie: todas). Valor: duración y easing del resaltado pasajero y transiciones de foco; respeta prefers-reduced-motion.
TK-008 — Resaltado de correspondencia (superficie: ventana-principal). Valor: color y duración del highlight pasajero entre paneles, no modifica contenido.

## Comandos de captura

Comando de captura (ventana-principal): dotnet run --project project/implementation/scripts/EggMdEditor.VisualCapture/EggMdEditor.VisualCapture.csproj -- ventana-principal <SC> <escenario> <salida>
Comando de captura (diálogo): dotnet run --project project/implementation/scripts/EggMdEditor.VisualCapture/EggMdEditor.VisualCapture.csproj -- diálogo <SC> <salida>
Comando de captura (ayuda): dotnet run --project project/implementation/scripts/EggMdEditor.VisualCapture/EggMdEditor.VisualCapture.csproj -- ayuda <SC> <salida>