# Inapp communication — F-002 Gestión de archivo

COM-005 — Invocación de acción de archivo → servicio de archivo
- **Origen:** fuente de entrada de `SC-001`: atajos de teclado, arrastre de archivos .md y entradas de Recientes.
- **Destino:** servicio de archivo de la aplicación.
- **Tipo:** síncrona, en proceso (aplicación local 100 % offline, C-001).
- **Información:** enviada — acción {nuevo, abrir, abrir por arrastre, abrir desde recientes, guardar, guardar como} y ruta elegida si aplica · recibida — documento de trabajo actualizado y, si aplica, su ruta asignada.
- **Propósito:** ejecutar cada operación de archivo sobre el documento único de trabajo (BR-003, BR-004).
- **Condiciones:** lo soltado que no sea .md se ignora sin alterar el documento abierto (FR-004); la conexión de las entradas del menú Archivo llega con F-028 (ADR-001).

COM-006 — Estado de guardado → ciclo del autoguardado
- **Origen:** estado de guardado del documento abierto.
- **Destino:** ciclo del autoguardado del servicio de archivo.
- **Tipo:** asíncrona, en proceso — periódica mientras dure «Con cambios sin guardar».
- **Información:** enviada — estado de guardado y destino {archivo del documento, fichero temporal} según ruta asignada · recibida — resultado de cada ciclo de escritura; el respaldo temporal no marca el documento como guardado (PD-FEAT-004).
- **Propósito:** persistir silenciosamente mientras haya cambios, sin aviso ni intervención (PD-FEAT-004, NFR-004).
- **Condiciones:** arranca solo con cambios sin guardar y cesa al persistirlos o al resolverse el aviso; la cadencia la define business-rules.md.

COM-007 — Operación bloqueante → aviso modal SC-002
- **Origen:** operación en curso {crear un documento nuevo, abrir otro archivo, cerrar la aplicación}.
- **Destino:** aviso modal `SC-002`.
- **Tipo:** síncrona, en proceso, modal sobre `SC-001`.
- **Información:** enviada — operación en curso (para el mensaje i18n) y estado de guardado · recibida — opción resuelta {guardar y continuar, continuar sin guardar, cancelar}.
- **Propósito:** resolver la operación en curso sin perder cambios (PD-FEAT-003).
- **Condiciones:** solo si hay cambios sin guardar y en los tres momentos declarados (PD-FEAT-003); cancelar anula la operación y el documento queda tal cual.

COM-008 — Opción «Guardar y continuar» → flujo de guardar
- **Origen:** aviso modal `SC-002`.
- **Destino:** servicio de archivo (Guardar o Guardar como).
- **Tipo:** síncrona, en proceso.
- **Información:** enviada — orden de guardar · recibida — documento persistido en su archivo o ruta asignada al documento.
- **Propósito:** persistir los cambios antes de continuar la operación que originó el aviso.
- **Condiciones:** con ruta aplica Guardar; sin ruta deriva a Guardar como, cuyo diálogo de elección es externo a la aplicación (referencia: external-services.md).

COM-009 — Servicio de archivo → registro de recientes → sublista Recientes
- **Origen:** servicio de archivo al abrir un archivo o guardar como.
- **Destino:** registro de archivos recientes y su sublista en `SC-001`.
- **Tipo:** síncrona, en proceso.
- **Información:** enviada — ruta del archivo abierto o guardado como · recibida — lista actualizada con la ruta al frente, máximo 10 entradas sin duplicados.
- **Propósito:** reflejar en `SC-001` la lista ordenada por uso y abrible con un clic (FR-004).
- **Condiciones:** cada apertura o guardar como mueve la ruta al frente (FR-004); la vigencia de la lista entre sesiones la define business-rules.md.

- **Asíncronos y entre procesos:** solo el ciclo del autoguardado es asíncrono en proceso (periódico); el resto del intercambio es síncrono y en proceso (BR-001, C-001).
- **Textos y comunicación externa:** los textos de `SC-002` y de Recientes resuelven del catálogo i18n reutilizando el canal de idioma de F-001 (COM-001 de F-001, BR-007); diálogos nativos del SO y fallos de estos canales: Fuera de alcance — se declaran en external-services.md y error-handling.md; el flujo de usuario vive en process.md.
