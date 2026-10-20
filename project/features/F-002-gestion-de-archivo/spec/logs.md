# Logs — F-002 Gestión de archivo

Regla anti-sobre-registro:

- Solo generan `LOG-xxx` los fallos de operaciones de archivo relevantes para diagnosticar: escritura del archivo del documento, apertura de archivo .md y ciclo del fichero temporal de respaldo (PD-FEAT-004). Las operaciones exitosas no se registran — su resultado queda observable en el documento de trabajo y en `SC-001`; no se añade ningún registro de inicio por rutina.
- Estos son los registros que `error-handling.md` enlaza como `Log: Requerido. Referencia: LOG-xxx`; la trazabilidad funcional antes/después de cada acción de usuario (quién invocó qué y con qué efecto) corresponde a `audit.md`, no a este documento.
- Correlación técnica: sesión y proceso únicos heredados de F-001; todo registro lleva marca de tiempo ISO 8601 y sale por la salida estándar del proceso — sin ficheros de registro propios en disco ni salida del equipo (BR-001, C-001).

LOG-004 — Fallo de escritura del archivo del documento

- **Evento:** solo el fin fallido de la escritura en Guardar, Guardar como, «Guardar y continuar» (COM-008) y en el ciclo de autoguardado con destino «Archivo del documento» (COM-006); sin registro del inicio ni de escrituras exitosas — el resultado exitoso lo refleja el estado de guardado del documento.
- **Resultado:** fallo — la escritura no concluye y el documento permanece «Con cambios sin guardar», conservando su contenido íntegro a la vista (NFR-004).
- **Datos:** hora · sesión · operación {guardar, guardar como, guardar y continuar, autoguardado} · ruta destino del archivo · tipo y mensaje de la excepción.
- **Nivel:** `Error` (enlazado desde `error-handling.md`).
- **No debe registrarse:** contenido del documento ni fragmentos escritos · sucesión de intentos exitosos de autoguardado · datos del aviso `SC-002`.

LOG-005 — Fallo de apertura de archivo .md

- **Evento:** solo el fin fallido de la apertura, por diálogo, arrastre o entrada de recientes (COM-005); las aperturas exitosas no se registran — el documento cargado y la reordenación de recientes lo evidencian (FR-004).
- **Resultado:** fallo — archivo ilegible o no disponible; el documento abierto se conserva tal cual y, si la apertura venía de arrastre o recientes, la operación queda sin efecto (process.md).
- **Datos:** hora · sesión · vía de apertura {diálogo, arrastre, recientes} · ruta intentada · tipo y mensaje de la excepción.
- **Nivel:** `Error`.
- **No debe registrarse:** contenido leído (indeterminable si el archivo no carga) · aperturas exitosas · la ruta del archivo soltado que no es .md.

LOG-006 — Ciclo del fichero temporal de respaldo

- **Evento:** fin fallido de cada respaldo periódico y de su eliminación al finalizar el ciclo de vida (PD-FEAT-004, COM-006); los respaldos y borrados exitosos no se registran — un asiento por ciclo sería ruido periódico.
- **Resultado:** fallo de respaldo — la copia de recuperación queda obsoleta o inexistente · fallo de eliminación — queda un fichero temporal huérfano en el directorio de temporales del sistema (BR-001).
- **Datos:** hora · sesión · operación {respaldo, eliminación} · ubicación del fichero temporal · tipo y mensaje de la excepción.
- **Nivel:** `Error` en fallo de respaldo · `Warning` en fallo de eliminación.
- **No debe registrarse:** contenido respaldado · cadencia o resultado de cada ciclo exitoso · contenido del documento.

Sin LOG — declaradas explícitas:

- Resolución del aviso `SC-002` y sus tres opciones (PD-FEAT-003): sin registro técnico — es decisión de usuario con efecto funcional, ámbito de `audit.md`, y su fallo operativo enlazable vive en `error-handling.md`.
- Arrastre de archivo no .md ignorado, atajos inertes y demás interacción de `SC-001`: sin registro — eventos no diagnosticables que no alteran el documento (process.md).
- Actualización del registro de recientes (COM-009): sin registro — su fallo no sale del proceso y la lista es estado visible en `SC-001`, máximo 10 entradas (FR-004).
- Creación de documento nuevo y cierre de la aplicación resuelto por el aviso: sin registro — la terminación del proceso ya queda registrada en F-001 (su LOG-003 de fin de sesión).
