# Visual Resources — F-001 Entorno de aplicación

```
VR-001 — Iconos de los controles de la barra de formato
Tipo: Icono.
Propósito: identificar visualmente cada control de la barra de formato (100 % de FR-012 en su parte visual; ADR-001).
Origen: iconografía existente del design-system (patrón ventana-principal: barra de formato con iconos).
Ubicación/referencia: SC-001 — Ventana principal · barra de formato, un icono por control en orden de
  escritura; superficie ventana-principal.
Formato y dimensiones: un glyph por control; tamaño, altura y espaciado según TK-004; contraste AA en
  ambos temas según TK-001.
Comportamiento visual: glyph estable al pasar el puntero (sin indicar acción conectada, ADR-001);
  acompañado del aviso flotante VR-002 sobre el área de documento.

VR-002 — Avisos flotantes de los controles de la barra de formato
Tipo: Aviso flotante (tooltip).
Propósito: mostrar el nombre de cada control al puntero, uno por control (FR-012 en su parte visual; ADR-001).
Origen: patrón existente del design-system (superficie ventana-principal: barra de formato con iconos y
  tooltips); estilo de burbuja según TK-001 y TK-004.
Ubicación/referencia: SC-001 — Ventana principal · superpuesto al área de documento junto al control
  apuntado; superficie ventana-principal.
Formato y dimensiones: burbuja de una línea con texto i18n es/en (BR-007, NFR-006); ancho según el
  contenido, altura y espaciado según TK-004, contraste AA según TK-001.
Comportamiento visual: visible mientras el puntero permanece sobre el control y oculto al retirarlo;
  contenido en el idioma activo con relabelizado en caliente (BR-007); sin interacción de clic.
```
