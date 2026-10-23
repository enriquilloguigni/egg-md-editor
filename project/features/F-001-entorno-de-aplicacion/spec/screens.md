# Screens — F-001 Entorno de aplicación

## SC-001 — Ventana principal
Propósito: ventana única con menú principal y barra de formato al 100 % de sus opciones visibles, interfaz es/en conmutable en caliente y atajos activos; acciones de menú/barra desconectadas (ADR-001).
Elementos: barra de título (única ventana, BR-003); menú persistente Archivo/Edición/Ver/Formato/Herramientas/Ayuda con 100 % de FR-011; barra de formato con icono+tooltip por control (FR-012); área de documento vacía; aviso flotante por control.
Campos: Idioma — selección Español/English, default Español, mutuamente excluyente, no persiste en F-001.
Acciones: conmutar idioma; abrir/cerrar entradas de menú; mostrar tooltip; cerrar ventana; atajos disponibles (Alt+letra, Esc, cierre).
Mensajes: rótulos y tooltips en idioma activo (i18n es/en extensible); sin mensajes propios (errores en error-handling.md).
Estados: arranque es/en; menú cerrado/desplegado; tooltip visible/oculto; opciones sin acción no alteran el documento.

## SC-002 — Diálogo
Propósito: modal sobre SC-001 para formularios y confirmaciones; en F-001 solo la superficie, sin diálogos conectados.
Elementos: overlay con elevación (TK-006); formulario con acciones primaria/secundaria; validación inline; confirmación al cerrar con cambios sin guardar.
Campos: según diálogo concreto (F-002…F-010); ninguno activo en F-001.
Acciones: aceptar/cancelar; cerrar con Esc; tabulación y foco inicial en acción primaria.
Mensajes: título y mensaje en idioma activo; errores inline asociados al campo.
Estados: abierto/cerrado; válido/inválido; con/sin cambios sin guardar.

## SC-003 — Ayuda
Propósito: ventana Acerca de (versión, atribuciones, licencias MIT) y guía integrada de Markdown con listado de atajos; superficie visible en F-001, contenido conectado por F-010.
Elementos: Acerca de (versión, atribuciones); guía con navegación por secciones y texto seleccionable; listado de atajos.
Campos: ninguno.
Acciones: cerrar; navegar secciones; copiar texto; cerrar con Esc.
Mensajes: textos en idioma activo.
Estados: abierto/cerrado; sección activa.

## Crítica
- SC-001 no declara tamaño mínimo de ventana (1280×720, NFR-001) ni orden de foco para navegación por teclado (NFR-009): ajuste del documento.
- SC-002 es genérico en F-001: las Features que lo usen deben concretar campos y validaciones en su propio screens.md para evitar divergencia; renuncia declarada.
- Iconos de la barra de formato (FR-012) requieren nombre accesible para lectores de pantalla (NFR-009); el tooltip por sí solo no basta: ajuste del documento.