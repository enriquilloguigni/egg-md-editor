# Design System — egg-md-editor

## Superficies

- **Ventana principal** (Avalonia UI · XAML + MVVM · render por software, sin GPU): menús Archivo/Edición/Ver/Formato/Herramientas/Ayuda y barra de formato (FR-002) sobre dos paneles del mismo documento — WYSIWYG a la izquierda, Markdown a la derecha (FR-001, FR-009). Wireframe: `SC-xxx.svg`.
- **Diálogos modales** (mismo stack, sobre la ventana principal): abrir/guardar como, exportar a PDF, metadatos, buscar/reemplazar, ir a línea y Acerca de con atribuciones (FR-004, FR-005, FR-006, FR-007, FR-010). Wireframe: `SC-xxx.svg`.
- **Idiomas de UI** (transversal a ambas, sin superficie propia): cadenas en recursos español e inglés, sin texto literal en la UI, sobre i18n extensible (BR-007, NFR-006).

## Identidad y navegación global

Una sola ventana con un único documento abierto (BR-003); menús y barra de formato siempre accesibles, foco visible en el control activo y atajos idénticos en ambos idiomas; tema claro/oscuro y zoom aplicados por igual a las dos vistas; menú Ayuda con guía de Markdown, listado de atajos y Acerca de (FR-009, FR-010).

## Patrones por superficie

- **Ventana principal**: dos paneles con divisor arrastrable, desplazamiento sincronizado entre vistas, resaltado de la correspondencia de posición y barra de estado con contador de palabras y caracteres (FR-001, FR-009).
- **Formularios de diálogo**: etiqueta sobre el control, ancho único, validación inline, acciones primaria/secundaria a la derecha y confirmación al cerrar con cambios sin guardar (FR-004, FR-005, FR-006).
- **Listados** (archivos recientes y sugerencias ortográficas): selección única, navegación con teclado y estado vacío con texto explicativo (FR-004, FR-008).
- **Mensajes**: avisos no destructivos en la ventana; confirmaciones destructivas solo en diálogo; errores con mensaje y detalle locales (FR-004).
- **Estados**: subrayado de erratas en ambas vistas (FR-008), indicador ocupado en barra de estado y transiciones de hasta 150 ms (render por software).

## Tokens (TK-xxx)

```
| ID | Nombre | Superficie | Valor |
|---|---|---|---|
| TK-005 | Color de acento | Todas | #2563EB |
| TK-006 | Fondo claro | Todas | #FFFFFF |
| TK-007 | Fondo oscuro | Todas | #1B1B1F |
| TK-008 | Texto primario (tema claro) | Todas | #111827 |
| TK-009 | Texto primario (tema oscuro) | Todas | #ECECEC |
| TK-010 | Fuente de interfaz | Todas | Segoe UI, Inter, sans-serif — 14 px |
| TK-011 | Fuente por defecto del panel Markdown | Ventana principal | Cascadia Mono, Consolas, monospace — 14 px |
| TK-012 | Espaciado base | Todas | 8 px |
| TK-013 | Duración máxima de transición | Todas | 150 ms |
```

## Comandos de captura

Comando de captura (ventana principal): scripts/ui-capture.sh <SC> <salida>
Comando de captura (diálogos): scripts/ui-capture.sh <SC> <salida>
