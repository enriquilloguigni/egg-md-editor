# Visual Resources — F-001 Entorno de aplicación

## VR-001 — Iconos de los controles de la barra de formato
Tipo: Icono (conjunto, un glyph por control).
Propósito: identificar visualmente cada control de la barra de formato (100 % de FR-012 en su parte visual) y exponer su nombre accesible para lectores de pantalla (NFR-009).
Origen: biblioteca de iconos existente (selección en Plan/Implementation; licencias MIT-compatibles), patrón ventana-principal del design-system.
Ubicación/referencia: SC-001 — Ventana principal · barra de formato, un icono por control en orden de escritura; superficie ventana-principal.
Formato y dimensiones: un glyph por control; tamaño, altura y espaciado según TK-004; contraste AA en ambos temas.
Comportamiento visual: glyph estable al pasar el puntero (sin indicar acción conectada, ADR-001); el nombre accesible no se sustituye por el aviso flotante.

Referencias:
- ADR-001 (propósito — parte visual de FR-012).
- NFR-011 (origen — licencias MIT-compatibles).
- TK-001 (formato y dimensiones — contraste AA en ambos temas).
- VR-002 (comportamiento visual — aviso flotante no sustituye el nombre accesible).

## VR-002 — Avisos flotantes de los controles de la barra de formato
Tipo: Aviso flotante (tooltip).
Propósito: mostrar el nombre de cada control al puntero, uno por control (FR-012 en su parte visual; ADR-001).
Origen: patrón existente del design-system (superficie ventana-principal: barra de formato con iconos y tooltips).
Ubicación/referencia: SC-001 — Ventana principal · superpuesto al área de documento junto al control apuntado; superficie ventana-principal.
Formato y dimensiones: burbuja de una línea con texto i18n es/en; ancho según contenido, altura y espaciado según TK-004; contraste AA en ambos temas.
Comportamiento visual: visible mientras el puntero permanece sobre el control y oculto al retirarlo; contenido en idioma activo con relabelizado en caliente (BR-013); sin interacción de clic; no sustituye el nombre accesible del icono (NFR-009).

Referencias:
- TK-001 (formato y dimensiones — contraste AA).