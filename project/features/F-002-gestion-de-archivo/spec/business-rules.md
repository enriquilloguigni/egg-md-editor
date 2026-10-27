# Business Rules — F-002 Gestión de archivo

Referencia (heredada, no reproducida): textos en Project Spec vigente.

Referencias heredadas:
- `FR-004` — gestión de archivo (crear, abrir, guardar, guardar como, arrastre, recientes, autoguardado, aviso, UTF-8).
- `FR-013` — persistencia local de preferencias/estado entre sesiones (recientes y recuperación).
- `BR-001` — un único documento por instancia.
- `BR-005` — bloque de metadatos al inicio, conservado íntegro.
- `BR-007` — UTF-8 y finales de línea por SO.
- `BR-008` — autoguardado silencioso, confirmación, 10 recientes y recuperación.
- `BR-012` — 100 % local y offline.
- `NFR-004`–`NFR-009`, `NFR-011` — sin red, privacidad, cero pérdida, desacople, multiplataforma, accesibilidad y licencias.
- `C-007`, `C-008`, `A-009` — UTF-8/finales, autoguardado/recientes y recuperación a los 30 s.

```
| id | condición | regla | consecuencia |
|---|---|---|---|
| BR-F002-001 | Cualquier operación que reemplace el documento | Un solo documento por instancia; Nuevo/Abrir/Reciente/Arrastrar reemplaza el actual solo tras resolver cambios pendientes | Nunca coexisten dos documentos (BR-001) |
| BR-F002-002 | Nuevo | Documento vacío, sin ruta, «Sin título»; F-002 no crea ni edita metadatos | Listo para editar; F-007 añade el bloque |
| BR-F002-003 | Abrir o arrastrar | Solo `.md` (extensión sin distinguir mayúsculas) y un archivo por operación; otro tipo o varios se rechazan | No se abre nada; el documento actual no cambia |
| BR-F002-004 | Abrir destino válido | Reemplaza el contenido, actualiza título y pasa la ruta al frente de recientes; si falla, no altera el documento | El documento anterior queda intacto ante error |
| BR-F002-005 | Guardar | Escribe en la ruta actual en UTF-8 sin BOM y finales del SO; sin ruta, actúa como Guardar como | El archivo del usuario solo cambia por acción explícita |
| BR-F002-006 | Guardar como | Crea una copia en la ruta elegida y pasa a trabajar sobre ella; el original no se modifica | La copia rige desde ese momento; recientes al frente |
| BR-F002-007 | Lectura/escritura | Lee UTF-8 con o sin BOM; escribe UTF-8 sin BOM; rechaza contenido no decodificable | No se convierte ni se guarda un archivo ilegible |
| BR-F002-008 | Finales de línea | Lee CRLF/LF/CR; al guardar normaliza todos al SO (CRLF Windows, LF macOS/Linux); no añade ni quita el salto final | Un archivo guardado no queda con finales mixtos |
| BR-F002-009 | Bloque de metadatos | Se conserva íntegro en contenido, orden y posición al abrir y guardar; solo se normalizan sus finales de línea; F-002 no lo edita | El bloque llega intacto a F-007 |
| BR-F002-010 | Recientes | Máximo 10, orden descendente, sin duplicados; abrir/guardar mueve al frente; entrada no disponible se elimina tras el error | La lista es corta, estable y depurable |
| BR-F002-011 | Persistencia de recientes | La lista sobrevive al cierre y se guarda solo en local (FR-013, BR-012) | El usuario vuelve a sus archivos en la siguiente sesión |
| BR-F002-012 | Autoguardado | Cada 30 s con cambios, sin diálogos, escribe copia de recuperación local; no modifica el archivo ni limpia el estado de cambios | El trabajo está protegido sin escrituras silenciosas al documento |
| BR-F002-013 | Arranque con copia vigente | Restaura automáticamente contenido y ruta, marca cambios sin guardar y no muestra diálogo | El último estado sobrevive a un cierre inesperado |
| BR-F002-014 | Nuevo/Abrir/Reciente/Arrastrar/Cerrar con cambios | Confirma con Guardar/No guardar/Cancelar; Cancelar aborta; si Guardar falla, la operación no continúa | Ningún cambio se descarta sin decisión explícita |
| BR-F002-015 | Guardado exitoso o descarte confirmado | Invalida/elimina la copia de recuperación; una copia obsoleta nunca se restaura | No reaparece contenido que el usuario descartó |
| BR-F002-016 | Documento abierto | El título muestra nombre y un indicador de cambios visible y no solo cromático | El estado de persistencia es perceptible y accesible |
| BR-F002-017 | Toda operación de archivo | Sin red, telemetría, servicios externos ni elevación; usa permisos del SO; un fallo conserva documento y recuperación | La privacidad y la ausencia de pérdida son invariantes |
```
