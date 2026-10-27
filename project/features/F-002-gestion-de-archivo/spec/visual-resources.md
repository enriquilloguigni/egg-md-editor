# Visual Resources — F-002 Gestión de archivo

## VR-004 — Icono de advertencia de cambios sin guardar
Tipo: Icono.
Propósito: reforzar visualmente la confirmación de descarte en `SC-002` (BR-F002-014).
Origen: biblioteca de iconos MIT-compatible ya usada en F-001; atribución en `SC-003`/Acerca de (NFR-011).
Ubicación/referencia: `SC-002` — confirmación de cambios sin guardar; superficie diálogo.
Formato y dimensiones: vectorial, 24×24 px, color semántico de advertencia de TK-001, contraste AA en ambos temas.
Comportamiento visual: decorativo (el texto de la confirmación comunica el riesgo); no sustituye el mensaje ni el nombre accesible.

## VR-005 — Icono de documento Markdown en recientes
Tipo: Icono.
Propósito: distinguir las entradas `.md` del submenú Recientes de `SC-001` (BR-F002-010).
Origen: misma biblioteca MIT-compatible de F-001; atribución en `SC-003`.
Ubicación/referencia: `SC-001` — submenú Archivo → Recientes, una por entrada; superficie ventana-principal.
Formato y dimensiones: vectorial, 16×16 px, color de texto de TK-001, contraste AA en ambos temas.
Comportamiento visual: estático; el nombre y la carpeta de la entrada aportan el texto accesible; no es el único indicador de tipo.

## VR-006 — Indicador de cambios sin guardar
Tipo: Indicador visual (glifo en la barra de título).
Propósito: señalar que el documento tiene cambios pendientes (BR-F002-016, NFR-009).
Origen: patrón de estado de control de TK-005 y paleta de TK-001; glifo `•` o `*` junto al nombre.
Ubicación/referencia: `SC-001` — barra de título; superficie ventana-principal.
Formato y dimensiones: un carácter, tamaño de TK-002, contraste AA; no depende solo del color.
Comportamiento visual: visible con cambios y oculto al guardar; el título incluye el nombre y el estado en texto para lectores de pantalla; es un indicador textual, no un icono de control.

No requerido / Fuera de alcance: los selectores de archivo nativos del SO no aportan recursos gráficos propios de F-002; el resaltado de arrastre se define como estado visual de `SC-001` (TK-001/TK-005).
