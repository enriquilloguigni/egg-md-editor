# Acceptance criteria — F-002 Gestión de archivo

AC-010 — Nuevo documento (FR-004)
Dado: SC-001 abierta sin cambios pendientes
Cuando: el usuario elige Archivo → Nuevo o pulsa Ctrl/Cmd+N
Entonces: queda un documento vacío sin ruta, título «Sin título», sin diálogo y listo para editar; no se crea bloque de metadatos (F-007) ni otra ventana (BR-F002-002).

AC-011 — Abrir un `.md` válido (FR-004)
Dado: un archivo `.md` existente y legible, y un documento actual sin cambios
Cuando: el usuario elige Archivo → Abrir, selecciona el archivo y confirma
Entonces: el contenido se muestra en el área de documento, el título refleja nombre/ruta, la ruta pasa al frente de recientes y el documento queda limpio; los metadatos y tareas se conservan tal cual (BR-005, BR-F002-004/007, SC-002).

AC-012 — Arrastrar un `.md` (FR-004)
Dado: SC-001 abierta
Cuando: el usuario suelta exactamente un archivo `.md` sobre la ventana
Entonces: se abre como en AC-011; el resaltado de arrastre es visible durante la operación; si suelta varios archivos o uno que no es `.md`, no se abre ninguno y se informa (BR-F002-003/014, ERR-014).

AC-013 — Guardar en el mismo archivo (FR-004)
Dado: un documento con ruta y cambios sin guardar
Cuando: el usuario elige Guardar o pulsa Ctrl/Cmd+S
Entonces: se escribe en la misma ruta en UTF-8 sin BOM y con finales del SO; el bloque de metadatos se conserva; el indicador de cambios desaparece y la ruta pasa al frente de recientes; si no hay ruta, se abre Guardar como; si falla, sigue con cambios y se informa (BR-F002-005/007/008/009, NFR-006, ERR-009/010, LOG-005).

AC-014 — Guardar como (FR-004)
Dado: un documento abierto, con o sin ruta
Cuando: el usuario elige Guardar como, indica carpeta y nombre, y confirma
Entonces: se crea una copia `.md` en la ruta elegida, el documento pasa a trabajar sobre la copia, el original no se modifica y la copia pasa al frente de recientes; si el nombre no lleva `.md`, se añade; si ya existe, se confirma la sobrescritura; si se cancela, nada cambia (BR-F002-006/011, SC-002, ERR-011, LOG-005).

AC-015 — Lista de 10 recientes (FR-004, FR-013)
Dado: el usuario ha abierto o guardado más de 10 archivos `.md`
Cuando: abre Archivo → Recientes, y también tras cerrar y reabrir la aplicación
Entonces: la lista muestra como máximo 10 entradas, en orden descendente, sin duplicados, con nombre y carpeta padre; seleccionar una la abre; si el archivo ya no existe, se informa y la entrada se elimina; la lista persiste localmente entre sesiones (FR-013, C-008, BR-F002-010/011, ERR-006, COM-014).

AC-016 — Autoguardado silencioso cada 30 s (FR-004)
Dado: un documento con cambios sin guardar
Cuando: transcurren 30 s sin que el usuario guarde
Entonces: se escribe una copia de recuperación local sin diálogo ni aviso de éxito; el archivo del usuario no se modifica y el documento sigue marcado con cambios; si la copia falla, se avisa de forma no modal y se reintenta en el siguiente ciclo (BR-F002-012, C-008, A-009, ERR-012, LOG-006, COM-011).

AC-017 — Aviso antes de reemplazar con cambios (FR-004)
Dado: un documento con cambios sin guardar
Cuando: el usuario elige Nuevo, Abrir, un reciente o suelta un `.md` sobre la ventana
Entonces: SC-002 pide confirmación con Guardar / No guardar / Cancelar; Guardar escribe y continúa; No guardar descarta e invalida la recuperación; Cancelar aborta y conserva el documento; si Guardar falla, la operación no continúa (BR-F002-014/015, SC-002, COM-009/010).

AC-018 — Aviso al cerrar con cambios (FR-004)
Dado: un documento con cambios sin guardar
Cuando: el usuario cierra la ventana
Entonces: se muestra la misma confirmación; Guardar cierra tras escribir, No guardar cierra e invalida la recuperación, Cancelar mantiene la ventana y el documento; sin cambios, el cierre es directo (BR-F002-014/015, SC-002, COM-009/010).

AC-019 — Recuperación automática al iniciar (FR-004)
Dado: una copia de recuperación vigente de la última sesión
Cuando: el usuario inicia la aplicación
Entonces: se restauran automáticamente contenido y ruta, el documento queda marcado con cambios sin guardar y no se muestra diálogo; Guardar escribe; descartar invalida la copia; si la copia es ilegible, se inicia un documento vacío sin tocar el original y se informa (BR-F002-013/015, A-009, ERR-013, LOG-007, COM-013).

AC-020 — UTF-8 y finales de línea por SO (FR-004)
Dado: un archivo `.md` con caracteres no ASCII y finales CRLF, LF o CR
Cuando: se abre y se guarda en Windows, macOS o Linux
Entonces: se lee como UTF-8 con o sin BOM; se escribe UTF-8 sin BOM con finales del SO (CRLF en Windows, LF en macOS/Linux); no se añade ni quita el salto final; un archivo no decodificable como UTF-8 se rechaza sin modificar (BR-007, NFR-008, C-007, BR-F002-007/008, ERR-008).

AC-021 — Bloque de metadatos preservado (FR-004)
Dado: un `.md` con bloque de metadatos delimitado al inicio
Cuando: se abre y se guarda sin editar ese bloque
Entonces: el bloque conserva contenido, orden y posición; la única transformación es la normalización de finales de línea del archivo; F-002 no lo edita ni lo reordena (BR-005, BR-F002-009).

AC-022 — Sin red, telemetría ni servicios externos (FR-004)
Dado: la aplicación en uso
Cuando: se ejecutan Nuevo, Abrir, Guardar, Guardar como, Recientes, Arrastrar, autoguardado y recuperación
Entonces: no se produce ninguna conexión de red ni telemetría; archivos, recientes y recuperación solo se leen/escriben en local; no se solicita elevación (BR-012, NFR-004/005, BR-F002-017, COM-007/014).

AC-023 — Errores de E/S sin pérdida de datos (FR-004)
Dado: un fallo de permiso, disco lleno, ruta inválida o archivo no legible
Cuando: el usuario abre o guarda
Entonces: se informa en el idioma activo; el documento actual y la copia de recuperación se conservan; el archivo original no queda a medias; se ofrece Reintentar / Guardar como / Cancelar o Elegir otro según el caso; se registra el fallo (NFR-006, BR-F002-017, ERR-007/009/010, LOG-004/005).

AC-024 — Solo `.md` y un documento por instancia (FR-004)
Dado: SC-001 abierta
Cuando: se intenta abrir un archivo que no es `.md` o varios por arrastre
Entonces: no se abre ninguno, se informa y el documento actual no cambia; no se crea una segunda ventana ni se abre otro documento (BR-001, BR-F002-003, ERR-014).

AC-025 — Título e indicador de cambios (FR-004)
Dado: un documento abierto
Cuando: su contenido cambia o se guarda
Entonces: el título muestra el nombre y un indicador de cambios visible que no depende solo del color; al guardar el indicador desaparece; el nombre y el estado son accesibles para lectores de pantalla (NFR-009, BR-F002-016, VR-006, COM-015).

DS-002 — Gestión de archivo sin pérdida (FR-004, FR-013)
Métrica:
- 100 % de las operaciones Nuevo/Abrir/Guardar/Guardar como/Reciente/Arrastrar/Autoguardar/Recuperar verificadas sobre el sistema de archivos.
- 0 pérdidas de contenido en 100 ciclos de cierre inesperado con recuperación.
- Autoguardado ≤ 30 s desde el último cambio; 10 recientes como máximo y en orden descendente exacto.
- 100 % de archivos escritos en UTF-8 sin BOM y con finales del SO; 0 accesos de red/telemetría; cobertura unitaria de la lógica de archivo ≥ 70 % (NFR-007); 0 dependencias no MIT-compatibles (NFR-011).
", "path">
project/features/F-002-gestion-de-archivo/spec/acceptance-criteria.md