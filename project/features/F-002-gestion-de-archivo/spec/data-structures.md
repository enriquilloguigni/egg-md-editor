# Data structures — F-002 Gestión de archivo

## Documento abierto

Descripción: único documento de trabajo Markdown UTF-8 que la Feature crea, abre y guarda; existe como máximo uno por sesión (BR-003, BR-004).

Atributos:
- Contenido  Tipo: Texto Markdown UTF-8  Requerido: Sí  Defecto: vacío tras crear un documento nuevo (FR-004).
- Ruta asignada  Tipo: Texto (ruta de disco local)  Requerido: No  Ausente en el documento nuevo; se fija al guardar como (BR-004).
- Estado de guardado  Tipo: Catálogo  Requerido: Sí  Defecto: Sin cambios pendientes; valores en Catálogos.
- Fecha de última modificación  Tipo: Fecha/hora  Requerido: Sí  Se actualiza al modificar o persistir el contenido; soporta la cadencia del autoguardado (PD-FEAT-004), cuyo umbral define business-rules.md.

Relaciones: asociado a ninguna o una ruta de disco; con cambios sin guardar alimenta el autoguardado silencioso — escritura directa en su archivo si tiene ruta, respaldo temporal si no la tiene (PD-FEAT-004, NFR-004); al abrirse o guardarse como registra una entrada en recientes (FR-004).

## Fichero temporal de respaldo

Descripción: copia de respaldo que el autoguardado escribe en disco mientras un documento sin ruta tiene cambios sin guardar; cada documento sin ruta escribe su respaldo en un fichero único por documento/instancia, con nombre impredecible (BR-F002-003); complementa al aviso de cambios sin guardar, no lo sustituye (PD-FEAT-004).

Atributos:
- Ubicación  Tipo: Texto (ruta de disco local)  Requerido: Sí  En el directorio de ficheros temporales del sistema operativo (BR-001, BR-F002-003).
- Contenido respaldado  Tipo: Texto Markdown UTF-8  Requerido: Sí  Copia íntegra del documento en cada respaldo (NFR-004).
- Cadencia  Tipo: Periódica  Requerido: Sí  Mismo período del autoguardado; el umbral numérico lo define business-rules.md.

Ciclo de vida: nace con el primer respaldo y se sobrescribe en cada ciclo del mismo documento —el respaldo de un documento nuevo nunca sobrescribe el huérfano conservado de otro, que persiste hasta que el usuario elija (BR-F002-005)—; se elimina al obtener el documento una ruta (guardar como) o al resolverse el aviso guardando o descartando los cambios (PD-FEAT-003, PD-FEAT-004); sin elección, el huérfano conserva los permisos restrictivos de BR-F002-003.

Recuperación: sobrevive a una terminación inesperada de la aplicación y conserva la última copia respaldada; al arrancar, la detección cubre todos los huérfanos —uno por cada documento sin ruta pendiente de elección— y por cada uno se ofrece recuperarlo o eliminarlo (BR-F002-005, PD-FEAT-006).

## Registro de archivos recientes

Descripción: lista de rutas de archivos .md abiertos o guardados como, ordenada por uso, limitada a 10 archivos y abrible con un clic (FR-004).

Atributos:
- Entradas  Tipo: Lista ordenada de rutas  Requerido: No  Máximo 10; una entrada por ruta, sin duplicados.
- Orden  Tipo: Por uso  Requerido: Sí  Cada apertura o guardar como mueve la ruta al frente (FR-004).
- Vigencia entre sesiones  Tipo: Booleano  Requerido: Sí  Valor definido en business-rules.md; FR-004 exige la lista durante la sesión.

Relaciones: cada entrada referencia un archivo Markdown del disco local (BR-004); elegida una entrada, su ruta pasa a ser la del documento abierto (screens.md, SC-001).

## Identificadores

- Documento abierto: uno por sesión (BR-003); su ruta lo identifica cuando la tiene; sin identificador externo mientras no.
- Fichero temporal: fichero único por documento sin ruta, nombre impredecible no compartido (BR-F002-003, BR-F002-005), bajo el directorio de ficheros temporales del sistema operativo.
- Entrada de recientes: la propia ruta del archivo, única en la lista (FR-004).

## Integridad

- Todo dato de la Feature reside en disco local, sin servicios de red (BR-001, C-001); máximo un documento abierto por sesión (BR-003).
- Un documento sin ruta con cambios sin guardar tiene exactamente un fichero temporal propio; con ruta asignada, ninguno (PD-FEAT-004).
- Toda escritura preserva el contenido íntegro en UTF-8 con finales de línea consistentes con el sistema (NFR-004).
- Invariante: Estado de guardado = Sin cambios pendientes implica última escritura correcta; un fallo de escritura lo deja en Con cambios sin guardar (process.md).
- Recientes: entre 0 y 10 entradas sin duplicados, la más usada al frente (FR-004).

## Catálogos y valores controlados

- Estado de guardado = {Sin cambios pendientes, Con cambios sin guardar}; gobierna el aviso SC-002 y el arranque del autoguardado (PD-FEAT-003, PD-FEAT-004).
- Destino del autoguardado = {Archivo del documento, Fichero temporal}; derivado de tener o no ruta asignada (PD-FEAT-004).

## Entradas y salidas

- Entradas: contenido leído de un archivo .md (abrir, arrastre, recientes) y ruta elegida en el diálogo del sistema al guardar como (BR-004).
- Salidas: escritura del documento en su archivo (NFR-004), fichero temporal de respaldo y registro de recientes actualizado (FR-004).
