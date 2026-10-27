# Acceptance criteria — F-002 Gestión de archivo

AC-010 — Nuevo documento (FR-004)

Dado: SC-001 abierta sin cambios pendientes

Cuando: el usuario elige Archivo → Nuevo o pulsa Ctrl/Cmd+N

Entonces: queda un documento vacío sin ruta, título «Sin título», sin diálogo y listo para editar; no se crea bloque de metadatos (F-007) ni otra ventana (BR-F002-002).

AC-011 — Abrir un `.md` (FR-004)

Dado: SC-001 abierta con un documento sin cambios; un archivo `.md` existente y legible o un arrastre sobre la ventana.

Cuando: el usuario elige Archivo → Abrir y confirma, o suelta un archivo sobre la ventana.

Entonces: el contenido se muestra en el área de documento, el título refleja nombre/ruta, la ruta pasa al frente de recientes y el documento queda limpio; los metadatos y tareas se conservan tal cual (BR-005, BR-F002-004, BR-F002-007, SC-002); por arrastre el resaltado es visible durante la operación (BR-F002-003, BR-F002-014); si suelta varios archivos o uno que no es `.md`, no se abre ninguno, se informa y el documento actual no cambia; no se crea una segunda ventana ni otro documento.

AC-013 — Guardar y Guardar como (FR-004)

Dado: un documento abierto, con o sin cambios y con o sin ruta.

Cuando: el usuario elige Guardar, Guardar como o pulsa Ctrl/Cmd+S, e indica carpeta y nombre y confirma en el segundo caso.

Entonces: con ruta se escribe en la misma ruta en UTF-8 sin BOM con finales del SO, el bloque de metadatos se conserva, el indicador desaparece y la ruta pasa al frente de recientes; sin ruta se abre Guardar como y se crea una copia `.md` en la ruta elegida, el documento pasa a trabajar sobre la copia, el original no se modifica y la copia pasa al frente de recientes; si el nombre no lleva `.md` se añade, si ya existe se confirma la sobrescritura y si se cancela nada cambia; si falla, sigue con cambios y se informa; el título muestra el nombre y un indicador de cambios visible no dependiente solo del color, accesible a lectores de pantalla, que desaparece al guardar (BR-F002-005, BR-F002-007, NFR-006, ERR-009, SC-002, VR-006).

AC-015 — Lista de 10 recientes (FR-004, FR-013)

Dado: el usuario ha abierto o guardado más de 10 archivos `.md`

Cuando: abre Archivo → Recientes, y también tras cerrar y reabrir la aplicación

Entonces: la lista muestra como máximo 10 entradas, en orden descendente, sin duplicados, con nombre y carpeta padre; seleccionar una la abre; si el archivo ya no existe, se informa y la entrada se elimina; la lista persiste localmente entre sesiones (FR-013, C-008, BR-F002-010/011, ERR-006, COM-014).

AC-016 — Autoguardado silencioso cada 30 s (FR-004)

Dado: un documento con cambios sin guardar

Cuando: transcurren 30 s sin que el usuario guarde

Entonces: se escribe una copia de recuperación local sin diálogo ni aviso de éxito; el archivo del usuario no se modifica y el documento sigue marcado con cambios; si la copia falla, se avisa de forma no modal y se reintenta en el siguiente ciclo (BR-F002-012, C-008, A-009, ERR-012, LOG-006, COM-011).

AC-017 — Aviso antes de reemplazar o cerrar con cambios (FR-004)

Dado: un documento con cambios sin guardar

Cuando: el usuario elige Nuevo, Abrir, un reciente, suelta un `.md` sobre la ventana o cierra la ventana

Entonces: SC-002 pide confirmación con Guardar / No guardar / Cancelar; Guardar escribe y continúa (o cierra tras escribir al cerrar); No guardar descarta e invalida la recuperación (o cierra e invalida al cerrar); Cancelar aborta y conserva el documento (o mantiene la ventana al cerrar); si Guardar falla, la operación no continúa; sin cambios el cierre es directo (BR-F002-014/015, SC-002, COM-009/010).

AC-019 — Recuperación automática al iniciar (FR-004)

Dado: una copia de recuperación vigente de la última sesión

Cuando: el usuario inicia la aplicación

Entonces: se restauran automáticamente contenido y ruta, el documento queda marcado con cambios sin guardar y no se muestra diálogo; Guardar escribe; descartar invalida la copia; si la copia es ilegible, se inicia un documento vacío sin tocar el original y se informa (BR-F002-013/015, A-009, ERR-013, LOG-007, COM-013).

AC-020 — UTF-8, finales de línea y metadatos preservados (FR-004)

Dado: un archivo `.md` con caracteres no ASCII, finales CRLF/LF/CR y, opcionalmente, bloque de metadatos delimitado al inicio.

Cuando: se abre y se guarda en Windows, macOS o Linux.

Entonces: se lee como UTF-8 con o sin BOM y se escribe UTF-8 sin BOM con finales del SO (CRLF en Windows, LF en macOS/Linux); no se añade ni quita el salto final; un archivo no decodificable como UTF-8 se rechaza sin modificar; el bloque de metadatos conserva contenido, orden y posición, con la única transformación de normalización de finales de línea, sin editar ni reordenar (BR-005, BR-007, NFR-008, C-007, BR-F002-007, ERR-008).

AC-023 — Errores de E/S sin pérdida de datos (FR-004)

Dado: un fallo de permiso, disco lleno, ruta inválida o archivo no legible

Cuando: el usuario abre o guarda

Entonces: se informa en el idioma activo; el documento actual y la copia de recuperación se conservan; el archivo original no queda a medias; se ofrece Reintentar / Guardar como / Cancelar o Elegir otro según el caso; se registra el fallo (NFR-006, BR-F002-017, ERR-007, ERR-009, ERR-010, LOG-004).

DS-002 — Gestión de archivo sin pérdida (FR-004, FR-013) Métrica: 100 % de las operaciones Nuevo/Abrir/Guardar/Guardar como/Reciente/Arrastrar/Autoguardar/Recuperar verificadas sobre el sistema de archivos; 0 pérdidas de contenido en 100 ciclos de cierre inesperado con recuperación; autoguardado ≤ 30 s desde el último cambio, 10 recientes como máximo y en orden descendente exacto; 100 % de archivos escritos en UTF-8 sin BOM y con finales del SO; 0 accesos de red/telemetría; cobertura unitaria de la lógica de archivo ≥ 70 % (NFR-007); 0 dependencias no MIT-compatibles (NFR-011).

```
| Criterio | IDs |
|---|---|
| AC-010 | FR-004, SC-001, F-007, BR-F002-002 |
| AC-011 | FR-004, SC-001, BR-005, BR-F002-004, BR-F002-007, SC-002, BR-F002-003, BR-F002-014, BR-001, ERR-014 |
| AC-013 | FR-004, BR-F002-005, BR-F002-006, BR-F002-007, BR-F002-008, BR-F002-009, BR-F002-011, BR-F002-016, NFR-006, NFR-009, ERR-009, ERR-010, ERR-011, LOG-005, SC-002, VR-006, COM-015 |
| AC-015 | FR-004, FR-013, C-008, BR-F002-010, BR-F002-011, ERR-006, COM-014 |
| AC-016 | FR-004, BR-F002-012, C-008, A-009, ERR-012, LOG-006, COM-011 |
| AC-017 | FR-004, BR-F002-014, BR-F002-015, SC-002, COM-009, COM-010 |
| AC-019 | FR-004, BR-F002-013, BR-F002-015, A-009, ERR-013, LOG-007, COM-013 |
| AC-020 | FR-004, BR-005, BR-007, NFR-008, C-007, BR-F002-007, BR-F002-008, BR-F002-009, ERR-008 |
| AC-023 | FR-004, NFR-006, BR-F002-017, ERR-007, ERR-009, ERR-010, LOG-004, LOG-005 |
| DS-002 | FR-004, FR-013, NFR-007, NFR-011 |
```