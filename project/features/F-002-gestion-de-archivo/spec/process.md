# Process — F-002 Gestión de archivo

## Inicio

- **Proceso de acceso:** el usuario trabaja sobre la ventana única `SC-001` entregada por F-001 e invoca las acciones de archivo mediante sus atajos de teclado y el arrastre de archivos .md sobre `SC-001`; la conexión funcional de las entradas del menú Archivo corresponde a F-028 (ADR-001).
- **Condiciones de acceso:** ninguna — operación 100 % local y offline, sin autenticación ni servicios de red (BR-001, C-001).
- **Datos requeridos:** para abrir y guardar como, la ruta elegida en el diálogo de archivos del sistema operativo; para crear, guardar y autoguardar, ninguno.
- **Precondiciones:** `SC-001` operativa con la base i18n es/en activa (F-001, BR-007); un único documento de trabajo a la vez (BR-003), de tipo archivo Markdown UTF-8 en disco local (BR-004).

## Flujo principal

**Arranque — ofrecimiento de recuperación (PD-FEAT-006, BR-F002-005):** al arrancar, el sistema detecta todos los ficheros temporales huérfanos — respaldos de documentos sin ruta tras una terminación inesperada, uno por cada documento sin ruta pendiente de elección — y presenta el ofrecimiento por cada uno, secuencialmente, para que el usuario elija por fichero entre dos rutas:

- **Recuperar:** el contenido respaldado de ese fichero se abre como documento de trabajo sin ruta y en estado «Con cambios sin guardar», y entra por el mismo ciclo de apertura y guardado que cualquier documento sin ruta — aviso `SC-002` en los tres momentos de PD-FEAT-003 y respaldo temporal del autoguardado (PD-FEAT-004).
- **Eliminar:** se descarta ese fichero temporal y, terminado el ofrecimiento de todos los huérfanos, el arranque continúa normal.
- **Sin elección sobre un fichero:** ese fichero se conserva con los permisos restrictivos de BR-F002-003 y su ofrecimiento se repite en cada arranque hasta que el usuario elija.

Los textos del ofrecimiento los define `screens.md`; el diagnóstico de fallos de lectura de un huérfano vive en `error-handling.md`.

1. **Nuevo:** el usuario invoca Nuevo → si el documento abierto tiene cambios sin guardar se presenta el aviso (PD-FEAT-003) con sus opciones; tras resolverlo, o si no hay cambios, el sistema crea un documento vacío sin ruta.
2. **Abrir:** el usuario invoca Abrir y elige un archivo .md en el diálogo del sistema → si hay cambios sin guardar, aviso (PD-FEAT-003) → resuelto, el sistema carga el archivo como único documento de trabajo (BR-003) y lo coloca al frente de recientes.
3. **Abrir por arrastre:** el usuario suelta un archivo .md sobre `SC-001` → mismo comportamiento del paso 2, aviso incluido (PD-FEAT-003).
4. **Abrir desde recientes:** el usuario pulsa una entrada de la lista → apertura con el mismo comportamiento del paso 2; la lista tiene máximo 10 archivos, ordenada por uso y abrible con un clic (FR-004).
5. **Guardar:** el usuario invoca Guardar → con ruta asignada, el sistema escribe el documento en su archivo en UTF-8 con finales de línea consistentes con el sistema (NFR-004) y el documento queda sin cambios pendientes; sin ruta, continúa como Guardar como.
6. **Guardar como:** el usuario invoca Guardar como y elige ruta y nombre .md → escritura conforme a NFR-004 → el documento queda asociado a la nueva ruta, que se registra en recientes.
7. **Autoguardado silencioso:** mientras existan cambios sin guardar, el sistema persiste periódicamente sin aviso ni intervención: documento con ruta, escritura directa en su archivo (NFR-004); documento sin ruta, respaldo periódico en fichero temporal. El respaldo temporal complementa al aviso de cambios sin guardar, no lo sustituye (PD-FEAT-004).
8. **Cierre de la aplicación:** el usuario cierra `SC-001` → si hay cambios sin guardar, aviso (PD-FEAT-003) → resuelto, la aplicación termina.

Opciones del aviso de cambios sin guardar: guardar y continuar (pasos 5–6), continuar sin guardar, o cancelar la operación en curso; sus textos y controles se definen en `screens.md`.

## Flujos alternativos y de excepción

- **Alternativo — arrastre de archivo no .md:** se ignora el archivo soltado y el documento abierto no se altera (FR-004 solo acepta .md por arrastre).
- **Alternativo — Guardar como sobre documento ya guardado:** el documento pasa a asociarse a la nueva ruta; el archivo anterior no se modifica.
- **Excepción — fallo de escritura al guardar o autoguardar:** el documento conserva su contenido y sus cambios pendientes; el manejo del error vive en `error-handling.md`.
- **Excepción — archivo a abrir ilegible o no disponible:** el sistema lo informa y conserva el documento actual; detalle en `error-handling.md`.

## Resultado observable y finalización

- **Resultado:** un único documento Markdown UTF-8 creado, abierto o guardado según la acción (BR-003, BR-004), persistido sin pérdida de contenido (NFR-004), con la lista de recientes actualizada y el autoguardado silencioso activo mientras haya cambios sin guardar (PD-FEAT-004).
- **Finalización:** cada acción termina con el documento en su estado resultante y `SC-001` lista para la siguiente operación; el cierre de la aplicación concluye tras resolver el aviso (PD-FEAT-003), sin conexión de menú en F-002 (F-028).
