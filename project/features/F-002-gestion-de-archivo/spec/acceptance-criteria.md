# Acceptance Criteria — F-002 Gestión de archivo

```
AC-016 — Crear un documento nuevo (FR-004, BR-003)
Dado:
SC-001 está operativa y el documento de trabajo está en ST-005 o ST-007, sin cambios sin guardar.
Cuando:
El usuario invoca Nuevo.
Entonces:
- Se crea un documento vacío sin ruta, en ST-005 y «Sin cambios pendientes» (TR-016).
- El documento anterior se descarta; su archivo en disco no se borra (TR-016).
- Sigue existiendo un único documento abierto en la sesión (BR-003).

AC-017 — Abrir un archivo por el diálogo nativo (FR-004, BR-003)
Dado:
SC-001 está operativa, el documento está en ST-005 o ST-007 y el usuario elige un archivo .md legible en el diálogo nativo de Abrir.
Cuando:
El sistema carga el archivo.
Entonces:
- El documento único pasa a tener la ruta y el contenido del archivo, en ST-007 y «Sin cambios pendientes» (TR-014).
- El contenido se decodifica como UTF-8 sin pérdida de caracteres y el documento de trabajo es ese archivo Markdown del disco local (NFR-004, BR-004).
- La ruta entra al frente de la lista de recientes (COM-009, BR-F002-004).

AC-018 — Abrir un archivo por arrastre sobre SC-001 (FR-004)
Dado:
El usuario arrastra un archivo .md sobre la ventana SC-001.
Cuando:
El usuario lo suelta sobre la ventana.
Entonces:
- Durante el arrastre, SC-001 se muestra resaltada como destino de soltado; al soltar o al salir de la ventana recupera su estado normal (VR-007, PD-FEAT-005).
- El archivo se abre con el mismo comportamiento que el diálogo: documento con ruta y contenido del archivo en ST-007 (TR-014).
- La ruta entra al frente de la lista de recientes (COM-009, BR-F002-004).

AC-019 — Soltar sobre SC-001 un archivo que no es .md (FR-004)
Dado:
El usuario arrastra sobre SC-001 un archivo que no es .md, o un arrastre que no puede distinguirse.
Cuando:
Se produce el soltado.
Entonces:
- El soltado se ignora: el documento conserva contenido, ruta y estado de guardado y permanece en ST-005, ST-007 o ST-008 (process.md).
- SC-001 no se resalta como destino de soltado y no se muestra ningún aviso (VR-007).
- No se genera ningún registro técnico por el ignorado (logs.md).

AC-020 — Abrir un archivo desde la lista de recientes con un clic (FR-004)
Dado:
La sublista Recientes de SC-001 muestra la ruta de un archivo .md existente.
Cuando:
El usuario pulsa esa entrada.
Entonces:
- El archivo se abre con el mismo comportamiento que el diálogo: documento con ruta y contenido del archivo en ST-007 (TR-014).
- La ruta pulsada pasa al frente de la lista de recientes (COM-009, BR-F002-004).
- Si el documento tenía cambios sin guardar, el aviso SC-002 aparece antes de sustituirlo (PD-FEAT-003).

AC-021 — Recientes: máximo 10, ordenados por uso y vigentes solo en la sesión (FR-004, BR-F002-004)
Dado:
El usuario ha abierto o guardado como más de 10 archivos distintos en la misma sesión.
Cuando:
El usuario consulta la sublista Recientes de SC-001.
Entonces:
- La lista muestra como máximo 10 entradas, sin duplicados, con la ruta usada más recientemente al frente (COM-009).
- Cada apertura y cada guardar como mueve su ruta al frente de la lista (BR-F002-004).
- Al cerrar la aplicación la lista no se escribe a disco y el siguiente arranque comienza con la lista vacía (BR-F002-004, TR-028).

AC-022 — Guardar un documento que ya tiene ruta (FR-004, NFR-004)
Dado:
El documento está en ST-008, con ruta asignada y cambios sin guardar.
Cuando:
El usuario invoca Guardar y la escritura en el archivo concluye.
Entonces:
- El archivo del documento queda con el contenido actual, en UTF-8 y con los finales de línea del sistema operativo (NFR-004, BR-004).
- El documento pasa a ST-007 y «Sin cambios pendientes», y el ciclo de autoguardado cesa (TR-011, BR-F002-002).
- La ruta del documento y la lista de recientes no cambian (TR-011).

AC-023 — Guardar un documento sin ruta deriva en Guardar como (FR-004, NFR-004)
Dado:
El documento está en ST-005 o ST-006, sin ruta asignada.
Cuando:
El usuario invoca Guardar y elige una ruta .md en el diálogo nativo; la escritura concluye.
Entonces:
- El documento obtiene la ruta elegida y su archivo queda escrito con el contenido íntegro (TR-012, NFR-004).
- El documento pasa a ST-007 y «Sin cambios pendientes»; si tenía cambios, su fichero temporal se elimina (TR-012).
- La nueva ruta entra al frente de la lista de recientes (COM-009, BR-F002-004).

AC-024 — Guardar como reasigna la ruta y no altera el archivo anterior (FR-004, NFR-004)
Dado:
El documento está en ST-007 o ST-008, con una ruta ya asignada.
Cuando:
El usuario invoca Guardar como, elige otra ruta .md y la escritura concluye.
Entonces:
- El documento se asocia a la nueva ruta y pasa a ST-007 y «Sin cambios pendientes» (TR-013).
- El archivo anterior no se modifica: su contenido en disco es el previo a la operación (TR-013).
- La nueva ruta entra al frente de la lista de recientes (COM-009, BR-F002-004).

AC-025 — Toda escritura conserva el contenido íntegro en UTF-8 y con finales de línea del sistema (NFR-004)
Dado:
El documento contiene acentos, emoji, saltos de línea y caracteres de control.
Cuando:
Se ejecuta cualquier escritura de la Feature — Guardar, Guardar como, autoguardado con ruta o respaldo temporal (COM-006, COM-008) — y se relee el fichero resultante.
Entonces:
- El contenido releído es idéntico al del documento: ningún carácter se pierde ni se altera, y la escritura es local, en el disco del equipo, sin red (NFR-004, BR-001).
- El fichero decodifica como UTF-8 válido y sus finales de línea son los del sistema operativo, sin mezcla de CRLF y LF (NFR-004).
- El documento pasa a «Sin cambios pendientes» solo tras una escritura correcta; un fallo lo conserva en «Con cambios sin guardar» (TR-011, TR-010).

AC-026 — Autoguardado silencioso de un documento con ruta (FR-004, NFR-004)
Dado:
El documento está en ST-008, con ruta asignada y cambios sin guardar.
Cuando:
Transcurren 30 segundos desde el último ciclo de autoguardado (BR-F002-001).
Entonces:
- El ciclo escribe el contenido en el archivo del documento, sin aviso, sin diálogo y sin intervención del usuario (COM-006, TR-009).
- El documento pasa a ST-007 y «Sin cambios pendientes», y el ciclo cesa (TR-009, BR-F002-002).
- El contenido escrito es el íntegro, en UTF-8 y con los finales de línea del sistema (NFR-004).

AC-027 — Autoguardado silencioso de un documento sin ruta (FR-004, NFR-004, BR-F002-003)
Dado:
El documento está en ST-006, sin ruta y con cambios sin guardar.
Cuando:
Transcurren 30 segundos desde el último ciclo de autoguardado (BR-F002-001).
Entonces:
- Se sobrescribe el fichero temporal propio del documento —nombre impredecible, único e inaccesible fuera de la cuenta del usuario— con una copia íntegra del contenido (TR-008, BR-F002-003, NFR-004).
- El documento permanece en ST-006 y «Con cambios sin guardar», y el ciclo no cesa: el respaldo no marca el documento como guardado (BR-F002-002, TR-008).
- No se muestra ningún aviso ni se pide intervención alguna (COM-006).

AC-028 — Aviso de cambios sin guardar en los tres momentos (FR-004, BR-007)
Dado:
El documento tiene cambios sin guardar, en ST-006 o ST-008.
Cuando:
El usuario invoca Nuevo, Abrir o el cierre de la aplicación.
Entonces:
- SC-002 aparece como aviso modal con las opciones «Guardar y continuar», «Continuar sin guardar» y «Cancelar», en el idioma activo (COM-007, BR-007).
- La operación en curso queda bloqueada hasta que el aviso se resuelve (PD-FEAT-003).
- El documento conserva contenido, ruta y estado de guardado mientras el aviso está visible (ST-006, ST-008).

AC-029 — Resolución del aviso con «Guardar y continuar» (FR-004, NFR-004)
Dado:
SC-002 está visible por cambios sin guardar.
Cuando:
El usuario elige «Guardar y continuar».
Entonces:
- Con ruta, el documento se persiste y pasa a ST-007; sin ruta, el Guardar como implícito elige ruta y el documento pasa igualmente a ST-007 (TR-019, TR-020, COM-008).
- El ciclo de autoguardado cesa y el fichero temporal, si existía, se elimina (TR-019, TR-020).
- La operación en curso continúa con el documento ya persistido (TR-019, TR-020).
- Si la escritura falla, la operación no continúa y SC-002 se cierra como cancelada (ERR-006, ERR-007, ERR-008).

AC-030 — Resolución del aviso con «Continuar sin guardar» (FR-004)
Dado:
SC-002 está visible por cambios sin guardar y el documento está en ST-006 o ST-008.
Cuando:
El usuario elige «Continuar sin guardar».
Entonces:
- Los cambios se descartan: el contenido vuelve al último estado persistido —vacío si el documento no tenía ruta— y el documento queda «Sin cambios pendientes» (TR-017, TR-018).
- El fichero temporal se elimina y el ciclo de autoguardado cesa (TR-017).
- La operación en curso continúa (TR-017, TR-018).

AC-031 — Resolución del aviso con «Cancelar» (FR-004)
Dado:
SC-002 está visible por cambios sin guardar.
Cuando:
El usuario elige «Cancelar».
Entonces:
- La operación en curso queda anulada: no se crea ningún documento, no se abre ningún archivo y la aplicación no se cierra (TR-021).
- El documento conserva contenido, ruta, estado de guardado y fichero temporal, y el ciclo de autoguardado no cesa (TR-021).

AC-032 — Fallo al escribir el archivo del documento (FR-004, NFR-004)
Dado:
La escritura falla por el sistema de archivos, la ruta deja de estar disponible o el sistema operativo deniega la escritura.
Cuando:
El usuario invoca Guardar, Guardar como o «Guardar y continuar», o corre el ciclo de autoguardado con destino «Archivo del documento».
Entonces:
- El documento conserva su contenido íntegro y permanece «Con cambios sin guardar»; no se le asigna ruta nueva ni entra en recientes (TR-010, ERR-006).
- La misma consecuencia se da con la ruta no disponible (ERR-007) o con el permiso denegado (ERR-008).
- SC-001 muestra el aviso no modal del motivo, con su acción correctora, y el fallo queda registrado para diagnóstico (ERR-006, ERR-007, ERR-008, LOG-004).
- Si la escritura venía de «Guardar y continuar», la operación en curso no continúa y SC-002 se cierra como cancelada (ERR-006).
- El ciclo de autoguardado no cesa: reintenta en el ciclo siguiente (BR-F002-001, TR-010).

AC-033 — Fallo al abrir el archivo (FR-004)
Dado:
El archivo elegido existe pero no se decodifica como UTF-8 válido, o no puede leerse por no existir, no estar disponible o por permisos.
Cuando:
El usuario intenta abrirlo por diálogo, arrastre o entrada de recientes.
Entonces:
- El archivo no se abre y la operación queda sin efecto: el documento conserva contenido, ruta y estado de guardado (TR-015, ERR-009).
- La misma consecuencia se da cuando el archivo no puede leerse (ERR-010).
- La ruta no entra en la lista de recientes (ERR-009, ERR-010).
- SC-001 muestra el aviso no modal del motivo y el fallo queda registrado con la vía de apertura intentada (ERR-009, ERR-010, LOG-005).

AC-034 — Fallo al invocar el diálogo nativo de archivos (FR-004)
Dado:
El sistema operativo no muestra el diálogo nativo de archivos al invocarlo.
Cuando:
El usuario invoca Abrir o Guardar como, o «Guardar y continuar» que deriva en Guardar como.
Entonces:
- La operación en curso se cancela sin efecto: no se elige ruta, no se escribe nada y el documento conserva contenido, ruta y estado de guardado (ERR-014, TR-012).
- SC-001 muestra el aviso no modal «No se pudo abrir el diálogo del sistema» (ERR-014).
- Si venía de «Guardar y continuar», la operación pendiente no continúa y SC-002 se cierra como cancelada (ERR-014).

AC-035 — Fallo al crear el respaldo temporal (FR-004, NFR-004)
Dado:
El directorio de temporales del sistema operativo no existe o no es accesible, o no se puede garantizar un nombre único e impredecible.
Cuando:
El ciclo de autoguardado de un documento sin ruta intenta crear su fichero de respaldo.
Entonces:
- No se crea ni se sobrescribe ningún temporal y el documento queda «Con cambios sin guardar» sin copia de recuperación (ERR-013, TR-010).
- SC-001 muestra el aviso no modal «No se pudo crear el respaldo temporal; el documento sigue sin guardar», con la acción correctora «Guardar como» (ERR-013).
- El aviso SC-002 de cambios sin guardar conserva toda su validez y el contenido sigue íntegro en SC-001 (BR-F002-002, ERR-013).
- El ciclo no cesa: reintenta en el ciclo siguiente (BR-F002-001).

AC-036 — Ofrecimiento de recuperación de un respaldo huérfano al arrancar (FR-004, BR-F002-005)
Dado:
Al arrancar existen ficheros temporales huérfanos de una sesión anterior terminada inesperadamente.
Cuando:
La aplicación los detecta y presenta el ofrecimiento SC-002, un diálogo por huérfano.
Entonces:
- El documento en espera no cambia y la sesión pasa a ST-009, con el documento en espera (TR-022).
- Con «Recuperar como documento de trabajo», el contenido respaldado pasa a ser el documento único, sin ruta y «Con cambios sin guardar» (ST-006, TR-023).
- Con «Eliminar respaldo», el fichero se elimina; con «Ahora no», se conserva con sus permisos restrictivos y volverá a ofrecerse en el siguiente arranque (TR-024, TR-025, BR-F002-003).
- Agotados los huérfanos, la aplicación queda operativa con el documento en espera (TR-027).

AC-037 — Fallo al leer o al borrar un respaldo huérfano (FR-004, BR-F002-005)
Dado:
El contenido de un huérfano no puede leerse, o el sistema operativo no permite borrarlo.
Cuando:
El usuario elige «Recuperar como documento de trabajo» o «Eliminar respaldo».
Entonces:
- Si no puede leerse, no se abre ningún documento y el huérfano se conserva con sus permisos restrictivos, volviendo a ofrecerse en el siguiente arranque (ERR-011, BR-F002-005).
- Si no puede borrarse, el fichero permanece con sus permisos y el ofrecimiento de los demás huérfanos continúa (ERR-012).
- SC-001 muestra el aviso no modal del motivo, con la acción correctora, y el fallo queda registrado (ERR-011, ERR-012, LOG-006).

AC-038 — Cierre de la aplicación (FR-004, BR-003)
Dado:
El documento está en ST-005, ST-006, ST-007 o ST-008.
Cuando:
El usuario cierra SC-001 y el aviso de cambios sin guardar, si lo había, queda resuelto.
Entonces:
- La sesión termina y el documento se descarta: no queda abierto ningún documento (ST-010, TR-028).
- No queda fichero temporal del documento y la lista de recientes no se escribe a disco (TR-028, BR-F002-004).
- El proceso termina sin procesos residuales (COM-004 de F-001).

DS-004 — Todo guardado conserva el contenido sin pérdida (FR-004, NFR-004)
Métrica:
- Tasa de escrituras verificadas por relectura —Guardar, Guardar como, autoguardado con ruta y respaldo temporal— con contenido idéntico al documento: 100 %; umbral 0 caracteres perdidos o alterados, 0 ficheros con finales de línea mezclados y decodificación UTF-8 válida en el 100 % de los casos (NFR-004).

DS-005 — Recientes conforme al máximo y al orden por uso (FR-004)
Métrica:
- Recuento de entradas de la sublista Recientes tras abrir 25 archivos distintos en la misma sesión: 10 entradas, sin duplicados y con la ruta usada más recientemente al frente; umbral 0 entradas por encima del máximo de 10, 0 duplicados y 0 posiciones fuera del orden por uso (BR-F002-004).

DS-006 — Autoguardado silencioso dentro de su ventana de 30 segundos (FR-004)
Métrica:
- Porcentaje de documentos con cambios sin guardar persistidos en ≤ 30 s desde el cambio: 100 %; umbral 0 documentos con más de 30 s sin persistir y 0 avisos o diálogos mostrados por el autoguardado (BR-F002-001).

DS-007 — Aviso de cambios sin guardar en los tres momentos (FR-004)
Métrica:
- Recuento de operaciones de Nuevo, Abrir y de cierre sobre un documento con cambios sin guardar que muestran SC-002 antes de continuar: 100 %; umbral 0 sustituciones del documento y 0 cierres de la aplicación sin aviso previo (PD-FEAT-003).
```

No requerido / Fuera de alcance — quedan fuera de esta Feature, con la referencia de su dueño:

- Edición y formato del contenido del documento (F-003, F-004, F-005).
- Bloque de metadatos del documento (F-008).
- Exportación a PDF (F-010).
- Conexión funcional de las entradas del menú Archivo con sus acciones (F-028).
- Persistencia de preferencias del usuario (F-030).
- Persistencia de la lista de recientes entre sesiones (F-030).
- Criterios de auditoría: `audit.md` los declara inexistentes para F-002.
- Fallos de arranque y de pintado de la ventana: pertenecen a F-001 (ERR-001, ERR-002).

El alcance de FR-004 se entrega completo en esta Feature, sin recortes (BR-010).
