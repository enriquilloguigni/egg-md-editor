# Screens — F-002 Gestión de archivo

## SC-001 — Ventana principal (F-002)
Propósito: alojar el documento único y exponer las acciones de archivo de F-002 —Nuevo, Abrir, Guardar, Guardar como, recientes y arrastre— sobre la ventana de F-001: F-002 conecta el rótulo Archivo y llena el área de documento; la doble vista es F-003.

Elementos: barra de título con nombre del documento e indicador de cambios; rótulo Archivo que despliega Nuevo, Abrir…, Recientes (submenú), Guardar y Guardar como…; submenú Recientes con hasta 10 entradas (nombre + carpeta padre) o «No hay archivos recientes» deshabilitado; área de documento editable mínima (sin columnas WYSIWYG/Markdown; F-003); resaltado de arrastre sobre toda la ventana; aviso no modal de fallo de autoguardado, sin bloquear la edición. Las entradas Propiedades y Exportar a PDF no se añaden en F-002 (F-007/F-009); los demás rótulos mantienen el comportamiento de F-001.

Campos: Nombre del documento — texto de solo lectura en el título, «Sin título» si no hay ruta; Indicador de cambios — visible/oculto, textual además de visual (NFR-009); Recientes — lista de 0 a 10 rutas, orden descendente, sin duplicados, cada una con nombre y carpeta padre.

Acciones: Nuevo (Ctrl+N / Cmd+N); Abrir… (Ctrl+O / Cmd+O); Guardar (Ctrl+S / Cmd+S); Guardar como… (Ctrl+Shift+S / Cmd+Shift+S); seleccionar un reciente con ratón o flechas + Enter; arrastrar un único `.md`; cerrar la ventana; Guardar se deshabilita si el documento está limpio y tiene ruta; Recientes muestra «No hay archivos recientes» deshabilitado si la lista está vacía.

Mensajes: sin mensajes propios de éxito; la confirmación y los errores de E/S se muestran en SC-002; el aviso de fallo de autoguardado se muestra de forma no modal en SC-001; el título comunica el estado sin depender solo del color.

Estados: documento nuevo/limpio/con cambios/recuperado; título con y sin indicador; menú Archivo cerrado/desplegado; submenú Recientes vacío/lleno; arrastre inactivo/activo; aviso de fallo de autoguardado visible/oculto; Guardar habilitado/deshabilitado; ventana con foco.

## SC-002 — Diálogo (F-002)
Propósito: superficie modal de F-002 para elegir archivo (Abrir/Guardar como), confirmar el descarte de cambios y presentar errores de E/S; los selectores nativos los aporta el SO y las confirmaciones/errores propios conservan la elevación, el foco inicial y la validación inline de la superficie genérica de F-001.

Elementos: selector de archivo nativo del SO para Abrir y Guardar como, con filtro Markdown (`*.md`), carpeta inicial y campo de nombre; diálogo propio de confirmación con icono de advertencia y tres acciones; diálogo de error con mensaje y acciones contextuales.

Campos: Abrir — Carpeta (navegación del SO), Nombre de archivo (requerido, existente, extensión `.md`), Filtro `Markdown (*.md)`. Guardar como — Carpeta (navegación del SO), Nombre (requerido, no vacío; si falta `.md` se añade; otra extensión → error inline), Tipo `Markdown (*.md)`, confirmación de sobrescritura del SO. Confirmación — Documento (solo lectura), Acción pendiente (Nuevo/Abrir/Reciente/Arrastrar/Cerrar, solo lectura), Opción (Guardar/No guardar/Cancelar, requerida, default Guardar). Error — Operación y causa (solo lectura), Archivo (solo lectura), Acciones según caso (Reintentar/Guardar como/Cancelar o Aceptar).

Acciones: Abrir/Guardar/Cancelar en el selector; Guardar/No guardar/Cancelar en la confirmación; Reintentar/Guardar como/Cancelar o Aceptar en el error; Enter activa la acción primaria, Esc equivale a Cancelar, Tab recorre campos y acciones; el foco inicial va a la acción primaria segura.

Mensajes: en el idioma activo (es/en); confirmación: «El documento «<nombre>» tiene cambios sin guardar. ¿Quieres guardarlos antes de continuar?»; error: «No se pudo <abrir|guardar> «<nombre>»: <causa>. El documento no se ha modificado.»

Estados: selector válido/inválido; archivo existente/nuevo; confirmación pendiente/resuelta; error con/sin reintento; acción primaria habilitada/deshabilitada durante la operación.

## Crítica
- El selector nativo no permite validación inline de la extensión: se declara que la app valida al aceptar y muestra el error en SC-002; renuncia a validación inline dentro del picker nativo.
- «No guardar» es destructivo: se etiqueta explícitamente, se mantiene como acción secundaria y el foco inicial queda en «Guardar»; Esc y el cierre equivalen a «Cancelar» para evitar descartes accidentales.
- Los recientes con nombres repetidos muestran la carpeta padre para desambiguar; la lista vacía se declara con un texto deshabilitado, sin acción.
- El indicador de cambios no depende solo del color y el título expone el nombre accesible; sin hallazgo pendiente para NFR-009.
