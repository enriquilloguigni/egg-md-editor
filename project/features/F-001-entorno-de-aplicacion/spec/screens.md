# Screens — F-001 Entorno de aplicación

## SC-001 — Ventana principal
Propósito: ventana única con menú principal de seis rótulos como botones, navegación secuencial entre ellos, barra de formato con los doce controles de FR-012 en orden de escritura y área de documento vacía; interfaz es/en y tema Claro/Oscuro/Sistema conmutables en caliente, acciones de menú/barra desconectadas (ADR-001, ADR-002).
Elementos: barra de título (única ventana, BR-003); botones de rótulo Archivo, Edición, Ver, Formato, Herramientas, Ayuda; botones de navegación secuencial «Siguiente» y «Anterior»; barra de formato con doce controles —1 Énfasis, 2 Encabezados, 3 Listas, 4 Cita, 5 Código, 6 Enlace, 7 Imagen, 8 Tabla, 9 Nota al pie, 10 Fórmula, 11 Diagrama, 12 Índice—, cada uno con icono y aviso flotante; área de documento vacía, sin columnas WYSIWYG/Markdown/Índice; entradas Herramientas → Idioma (Español/English) y Herramientas → Tema (Claro/Oscuro/Sistema); tamaño mínimo 1280×720.
Campos: Idioma — selección Español/English, default Español, mutuamente excluyente, no persiste en F-001. Tema — selección Claro/Oscuro/Sistema, default Sistema (heredado del SO), mutuamente excluyente, no persiste en F-001. Rótulo seleccionado — lista de 6 valores, requerido, default Archivo; los botones Siguiente/Anterior lo recorren cíclicamente.
Acciones: conmutar idioma; seleccionar tema; navegar al rótulo siguiente/anterior; abrir/cerrar el rótulo seleccionado; mostrar aviso flotante; cerrar ventana; atajos disponibles (Alt+letra, Esc, cierre).
Mensajes: rótulos y avisos en idioma activo (i18n es/en extensible); sin mensajes propios (errores en error-handling.md).
Estados: arranque es/en; tema claro/oscuro/sistema; rótulo seleccionado 1–6; menú cerrado/desplegado; aviso visible/oculto; rótulos y controles sin acción no alteran el documento.

## SC-002 — Diálogo
Propósito: modal sobre SC-001 para formularios y confirmaciones; en F-001 solo la superficie, sin diálogos conectados.
Elementos: overlay con elevación (TK-006); formulario con acciones primaria/secundaria; validación inline; confirmación al cerrar con cambios sin guardar.
Campos: según diálogo concreto (F-002…F-010); ninguno activo en F-001.
Acciones: aceptar/cancelar; cerrar con Esc; tabulación y foco inicial en acción primaria.
Mensajes: título y mensaje en idioma activo; errores inline asociados al campo.
Estados: abierto/cerrado; válido/inválido; con/sin cambios sin guardar.

## SC-003 — Ayuda
Propósito: ventana Acerca de (versión, atribuciones de la librería de iconos MIT y demás licencias) y guía integrada de Markdown con listado de atajos; superficie visible en F-001, contenido conectado por F-010.
Elementos: Acerca de (versión, atribuciones); guía con navegación por secciones y texto seleccionable; listado de atajos.
Campos: ninguno.
Acciones: cerrar; navegar secciones; copiar texto; cerrar con Esc.
Mensajes: textos en idioma activo.
Estados: abierto/cerrado; sección activa.

## Crítica
- SC-001 incorpora tamaño mínimo 1280×720 y orden de foco (rótulos → navegación → barra de formato → área de documento) para NFR-009; sin hallazgo pendiente.
- SC-002 sigue siendo una superficie genérica: las Features que lo usen deben concretar campos y validaciones en su propio screens.md para evitar divergencia (renuncia declarada).
- SC-003 conecta guía y atajos en F-010; en F-001 solo se exige la superficie y la atribución de iconos (renuncia declarada).
