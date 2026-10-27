# In-app communication — F-002 Gestión de archivo

## COM-007 — Comando de archivo
Origen: menú Archivo, atajos, submenú Recientes, arrastre y arranque → Destino: coordinador de archivo (ViewModel/servicio) · Tipo: síncrono (invocación); la E/S se ejecuta asíncrona y no bloquea la UI.
Información enviada/recibida: se envía la operación {Nuevo,Abrir,Guardar,Guardar como,Reciente,Arrastrar,Cerrar} y la ruta si existe; se recibe el resultado {Completada,Cancelada,Error}.
Propósito: un único punto de entrada para las operaciones de archivo (FR-004) · Condición: `SC-001` abierta y sin otra operación de E/S en curso.

## COM-008 — Coordinador ↔ documento
Origen: coordinador de archivo → Destino: documento abierto (`ENT-008`) · Tipo: síncrono.
Información enviada/recibida: se envía contenido/ruta/origen; se recibe contenido lógico, estado de cambios y bloque de metadatos intacto.
Propósito: cargar, reemplazar y guardar el documento único (BR-001, BR-F002-004/005) · Condición: documento válido.

## COM-009 — Coordinador → diálogo SC-002
Origen: coordinador de archivo → Destino: `SC-002` (selector, confirmación o error) · Tipo: síncrono (apertura modal).
Información enviada/recibida: se envía modo, carpeta/nombre inicial, documento y acción pendiente; se recibe ruta elegida u opción {Guardar,No guardar,Cancelar}.
Propósito: obtener ruta y resolver el descarte (BR-F002-006/014) · Condición: operación pendiente.

## COM-010 — Diálogo SC-002 → coordinador
Origen: `SC-002` → Destino: coordinador de archivo · Tipo: síncrono (retorno modal).
Información enviada/recibida: se envía la ruta validada o la opción de confirmación; se recibe la continuación o cancelación de la operación.
Propósito: cerrar el circuito de decisión sin lógica de archivo en la UI · Condición: diálogo aceptado o cancelado.

## COM-011 — Temporizador de autoguardado
Origen: temporizador de 30 s → Destino: servicio de recuperación · Tipo: asíncrono (periódico).
Información enviada/recibida: se envía el tick y el documento con cambios; se recibe la copia de recuperación escrita o el fallo.
Propósito: autoguardado silencioso (BR-F002-012, A-009) · Condición: documento con cambios y sin operación de E/S en curso.

## COM-012 — Arrastre desde el SO
Origen: sistema operativo (arrastre) → Destino: ventana `SC-001` y coordinador · Tipo: asíncrono (evento de plataforma).
Información enviada/recibida: se envía la lista de rutas soltadas; se recibe la validación {un `.md`, ruta legible} y la apertura o el rechazo.
Propósito: abrir un `.md` soltándolo (FR-004, BR-F002-003) · Condición: ventana con foco.

## COM-013 — Recuperación al iniciar
Origen: arranque de la aplicación → Destino: servicio de recuperación y documento · Tipo: síncrono (antes de mostrar el documento).
Información enviada/recibida: se envía la consulta de copia vigente; se reciben contenido, ruta y marca de tiempo, o ausencia.
Propósito: restaurar el último estado autoguardado (BR-F002-013, NFR-006) · Condición: copia vigente y legible.

## COM-014 — Actualización de recientes
Origen: coordinador de archivo → Destino: lista de recientes (`ENT-010`) y almacén local · Tipo: síncrono en memoria; persistencia local asíncrona.
Información enviada/recibida: se envía ruta canónica, nombre, carpeta y fecha; se recibe la lista ordenada (máx. 10) y el resultado de la persistencia.
Propósito: mantener los recientes entre sesiones (BR-F002-010/011, FR-013) · Condición: apertura o guardado exitoso.

## COM-015 — Estado visible del documento
Origen: documento y coordinador → Destino: barra de título de `SC-001` · Tipo: síncrono.
Información enviada/recibida: se envían nombre, ruta y estado {Limpio,Con cambios}; se recibe el título con indicador textual/visible.
Propósito: comunicar el estado de persistencia (BR-F002-016, NFR-009) · Condición: documento cargado.

Referencias:
- TR-012…TR-033 (propósito — estados y transiciones del documento).
- ERR-006…ERR-015 (propósito — fallos de E/S, recuperación y estado local).
