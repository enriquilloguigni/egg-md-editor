# Estados y transiciones — F-002 Gestión de archivo

## Estados

### ST-005 — Documento nuevo sin ruta

Documento de trabajo vacío, sin ruta asignada y en estado «Sin cambios pendientes»; sin fichero temporal y con contenido vacío. Es el estado con el que arranca la aplicación y al que vuelve la acción «Nuevo».

### ST-006 — Documento sin ruta con cambios sin guardar

Documento sin ruta asignada y en estado «Con cambios sin guardar», con el contenido editado en F-003. Corre aquí el ciclo de autoguardado con destino «Fichero temporal» (BR-F002-001); existe un fichero temporal de respaldo propio, único e impredecible (BR-F002-003), que puede faltar si su creación falló (ERR-013).

### ST-007 — Documento con ruta

Documento con ruta asignada y en estado «Sin cambios pendientes», con el contenido del archivo en disco, en UTF-8 (NFR-004). No tiene fichero temporal: el autoguardado escribe en el archivo del documento (PD-FEAT-004).

### ST-008 — Documento con ruta con cambios sin guardar

Documento con ruta asignada y en estado «Con cambios sin guardar», con el contenido editado en F-003. Corre aquí el ciclo de autoguardado con destino «Archivo del documento» (BR-F002-001, PD-FEAT-004) y no tiene fichero temporal.

### ST-009 — Ofrecimiento de recuperación de respaldo huérfano

Capa modal sobre el documento en espera, que no cambia: la ruta y el estado de guardado se mantienen sin cambio y el contenido sigue siendo el último respaldado. El fichero temporal huérfano se conserva con permisos restrictivos (BR-F002-003) y su ofrecimiento se repite en cada arranque (BR-F002-005).

### ST-010 — Documento cerrado — sesión finalizada

Sesión terminada: el documento se descarta, su contenido queda descartado y no tiene ruta ni fichero temporal —el ciclo del fichero temporal termina con el documento (data-structures.md)—. Estado final, sin salida.

Estado inicial: `ST-005` — el documento de trabajo vacío y sin ruta con el que arranca la aplicación; la acción «Nuevo» vuelve a este estado. Estado final: `ST-010`.

El ciclo de autoguardado corre solo en `ST-006` y `ST-008`, con destino «Fichero temporal» y «Archivo del documento» respectivamente (BR-F002-001, PD-FEAT-004); en los demás estados no corre.

`ST-009` es una capa modal: el documento de trabajo queda en espera mientras se ofrece cada huérfano (BR-F002-005).

Los estados de superficie —arrastre sobre `SC-001`, aviso `SC-002` visible— no son estados funcionales: se describen en `process.md` e `inapp-communication.md`.

## Transiciones

```
| id | origen | destino | evento | condición | efecto |
|---|---|---|---|---|---|
| TR-006 | ST-005 | ST-006 | Cambio de contenido (evento de F-003) | El documento no tiene ruta | «Sin cambios pendientes» → «Con cambios sin guardar»; arranca el ciclo de autoguardado con destino «Fichero temporal» (BR-F002-001); ruta y contenido sin cambio |
| TR-007 | ST-007 | ST-008 | Cambio de contenido (evento de F-003) | El documento tiene ruta | «Sin cambios pendientes» → «Con cambios sin guardar»; arranca el ciclo de autoguardado con destino «Archivo del documento»; ruta y contenido sin cambio |
| TR-008 | ST-006 | ST-006 | Fin del ciclo de autoguardado con destino «Fichero temporal» | El respaldo se escribe correctamente (BR-F002-002) | El fichero temporal se sobrescribe con el contenido íntegro; el documento permanece «Con cambios sin guardar» y el ciclo no cesa (BR-F002-001) |
| TR-009 | ST-008 | ST-007 | Fin del ciclo de autoguardado con destino «Archivo del documento» | La escritura en el archivo del documento concluye correctamente (NFR-004, BR-F002-002) | El archivo queda con el contenido actual; el documento pasa a «Sin cambios pendientes»; el ciclo de autoguardado cesa; la ruta se conserva |
| TR-010 | ST-006, ST-008 | sin cambio | Fallo de escritura en el ciclo de autoguardado, en Guardar o en Guardar como | ERR-006, ERR-007, ERR-008 o ERR-013 | El documento conserva el contenido íntegro y permanece «Con cambios sin guardar»; no se le asigna ruta nueva ni entra en recientes; el ciclo no cesa y reintenta en el ciclo siguiente (BR-F002-001); aviso no modal (error-handling.md) |
| TR-011 | ST-008 | ST-007 | El usuario invoca Guardar | La escritura en el archivo del documento concluye (NFR-004) | El archivo del documento queda actualizado; «Sin cambios pendientes»; cesa el ciclo de autoguardado; ruta y recientes sin cambio |
| TR-012 | ST-005, ST-006 | ST-007 | El usuario invoca Guardar y la operación continúa como Guardar como | El usuario elige ruta .md y la escritura concluye (ERR-014 cancela la operación) | El documento obtiene la ruta elegida; «Sin cambios pendientes»; si tenía cambios sin guardar, su fichero temporal se elimina (data-structures.md); la ruta entra en recientes (BR-F002-004) |
| TR-013 | ST-008 | ST-007 | El usuario invoca Guardar como | El usuario elige otra ruta .md y la escritura concluye | El documento se asocia a la nueva ruta y el archivo anterior no se modifica (process.md); «Sin cambios pendientes»; la nueva ruta entra en recientes (BR-F002-004) |
| TR-014 | ST-005, ST-007 | ST-007 | El usuario abre un archivo .md por diálogo, arrastre o entrada de recientes | La lectura del archivo concluye | El documento único pasa a tener la ruta y el contenido del archivo (BR-003); «Sin cambios pendientes»; la ruta entra en recientes (BR-F002-004) |
| TR-015 | ST-005, ST-007 | sin cambio | Fallo de apertura | ERR-009 o ERR-010 | El documento se conserva tal cual —contenido, ruta y estado de guardado—; la operación queda sin efecto; la ruta no entra en recientes (error-handling.md) |
| TR-016 | ST-005, ST-007 | ST-005 | El usuario invoca Nuevo | El documento abierto no tiene cambios sin guardar | Se crea un documento vacío sin ruta, «Sin cambios pendientes»; el documento anterior se descarta y su archivo no se borra (security-permissions.md); los recientes no cambian (BR-F002-004) |
| TR-017 | ST-006 | ST-005 | El aviso SC-002 se resuelve con «Continuar sin guardar» | La resolución descarta los cambios (audit.md) | El contenido vuelve al último estado persistido —vacío si el documento no tenía ruta—; el fichero temporal se elimina; cesa el ciclo de autoguardado; el documento queda «Sin cambios pendientes»; la operación en curso puede continuar |
| TR-018 | ST-008 | ST-007 | El aviso SC-002 se resuelve con «Continuar sin guardar» | La resolución descarta los cambios (audit.md) | El contenido vuelve al del archivo en disco; la ruta se conserva; el documento queda «Sin cambios pendientes»; la operación en curso puede continuar |
| TR-019 | ST-006 | ST-007 | El aviso SC-002 se resuelve con «Guardar y continuar» | El Guardar como implícito elige ruta y la escritura concluye (ERR-014 deja la operación sin efecto) | El documento obtiene ruta y queda persistido, «Sin cambios pendientes»; su fichero temporal se elimina; la operación en curso continúa |
| TR-020 | ST-008 | ST-007 | El aviso SC-002 se resuelve con «Guardar y continuar» | La escritura en el archivo del documento concluye (ERR-006, ERR-007 o ERR-008 dejan la operación sin efecto) | El documento queda persistido, «Sin cambios pendientes»; cesa el ciclo de autoguardado; la operación en curso continúa |
| TR-021 | ST-006, ST-008 | sin cambio | El usuario invoca Nuevo, Abrir o Cerrar y resuelve SC-002 con «Cancelar» | — | La operación en curso queda anulada; el documento conserva contenido, ruta, estado de guardado y fichero temporal; el ciclo de autoguardado no cesa |
| TR-022 | ST-005 | ST-009 | Arranque de la aplicación | Se detectan ficheros temporales huérfanos (BR-F002-005) | Se presenta el ofrecimiento SC-002, un diálogo por huérfano (PD-FEAT-006); el documento en espera no cambia |
| TR-023 | ST-009 | ST-006 | El usuario elige «Recuperar como documento de trabajo» | La lectura del fichero temporal concluye (ERR-011 no abre documento) y, si el documento en espera tiene cambios sin guardar, el aviso SC-002 se ha resuelto antes (PD-FEAT-003) | El contenido respaldado pasa a ser el documento de trabajo: sin ruta y «Con cambios sin guardar»; el huérfano queda resuelto —deja de ofrecerse— y su fichero pasa a ser el respaldo del documento; arranca el ciclo de autoguardado con destino «Fichero temporal» |
| TR-024 | ST-009 | ST-009 | El usuario elige «Eliminar respaldo» | Quedan huérfanos por ofrecer; el borrado del fichero concluye (ERR-012 lo conserva y lo vuelve a ofrecer) | El fichero se elimina; el documento en espera no cambia |
| TR-025 | ST-009 | ST-009 | El usuario elige «Ahora no» (sin elección) | Quedan huérfanos por ofrecer | El fichero se conserva con los permisos restrictivos de BR-F002-003 y su ofrecimiento se repite en cada arranque (BR-F002-005); el documento en espera no cambia |
| TR-026 | ST-006 | ST-009 | Tras resolver un huérfano queda otro por ofrecer | — | El documento recuperado queda en espera y el ofrecimiento continúa con el siguiente huérfano |
| TR-027 | ST-009 | ST-005 o ST-006 | Fin del ofrecimiento de recuperación | Se han agotado los huérfanos; si se recuperó alguno el destino es ST-006, en caso contrario ST-005 | La aplicación queda operativa con el documento en espera; ningún huérfano queda pendiente en este arranque |
| TR-028 | ST-005, ST-006, ST-007, ST-008 | ST-010 | El usuario cierra la aplicación | El aviso SC-002, si había cambios sin guardar, se resolvió antes (TR-017 a TR-021) | La sesión termina: el documento se descarta, no queda fichero temporal del documento y los recientes no se escriben a disco (BR-F002-004) |
```

## Transiciones no permitidas

- El paso a «Con cambios sin guardar» solo lo produce un cambio de contenido de F-003: ni el fin del ciclo de autoguardado ni una escritura fallida lo producen (data-structures.md).
- El paso a «Sin cambios pendientes» solo lo produce una escritura correcta: un fallo de escritura conserva el estado «Con cambios sin guardar» (ERR-006, ERR-007, ERR-008, ERR-013).
- Sustituir el documento abierto —Nuevo, apertura o recuperación de otro huérfano— sin resolver antes el aviso `SC-002` cuando hay cambios sin guardar: no permitido (PD-FEAT-003); ERR-014 deja la operación sin efecto.
- No existe un estado con ruta asignada y fichero temporal a la vez: ruta y respaldo son excluyentes (data-structures.md).
- `ST-010` no admite salida: la sesión no se reanuda; cada arranque parte de `ST-005` y vuelve a ofrecer los huérfanos conservados (BR-F002-005).
