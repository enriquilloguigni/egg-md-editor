# In-app communication — F-004 Formato Markdown completo

## COM-022 — Aplicación de formato sobre la selección o estructura

Origen: menú Formato, barra de formato (FR-012) y atajos (COM-001) → Destino: motor de formato y modelo de documento (ENT-008, ENT-016, ENT-020).

Tipo: Síncrono.

Información enviada/recibida: se envía el tipo de formato {énfasis, encabezado (nivel/promover/degradar), lista, cita, código, enlace, imagen, tabla, nota al pie, fórmula, diagrama, escape} con la selección o posición y sus parámetros; se recibe el nodo de formato aplicado (ENT-021–ENT-031) o la estructura preparada con el cursor en posición de edición.

Propósito: aplicar el formato solicitado sobre la selección o preparar la estructura correspondiente en el panel activo (BR-016, FR-002, FR-012); condición: panel activo disponible y comando válido.

## COM-023 — Respuesta de formato a barra/menú/atajo

Origen: motor de formato → Destino: control invocante (menú Formato, barra de formato, atajo) y aviso flotante (COM-004).

Tipo: Síncrono.

Información enviada/recibida: se envía el resultado {aplicado, preparado, cancelado, error} con el identificador del control; se recibe la actualización del estado del control y, si aplica, el aviso no modal.

Propósito: confirmar la acción y actualizar la UI sin bloquear la edición (FR-012, BR-016, NFR-019); condición: comando procesado.

## COM-024 — Error de formato

Origen: motor de formato / renderizador (fórmula, diagrama, tabla, enlace) → Destino: panel izquierdo (SC-001) y aviso flotante.

Tipo: Síncrono (con marcador persistente en el panel).

Información enviada/recibida: se envía el código de error {renderizado, límite, sanitización} con el contexto afectado; se recibe el marcador de error en el panel izquierdo o el aviso no modal, sin alterar el Markdown.

Propósito: informar fallos de formato/renderizado de forma no bloqueante (ERR-019, ERR-020, ERR-021, BR-011, NFR-017); condición: fallo detectado.

Referencias:
- COM-016/COM-017: la actualización del AST compartido y el re-render de ambos paneles se canalizan por el evento de cambio existente (COM-017); el formato escribe en el modelo único (COM-016) y ambos paneles se sincronizan automáticamente.
- COM-021: la barra de formato actúa sobre el panel activo (solicitud inicial).
- COM-001: comando compartido menú/barra/atajo.
- ERR-017, ERR-019, ERR-020, ERR-021 (fallos y límites).