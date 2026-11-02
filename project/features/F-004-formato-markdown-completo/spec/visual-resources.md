# Visual Resources — F-004 Formato Markdown completo

## VR-010 — Iconos de formato de la barra (conjunto)

Tipo: Icono (conjunto, un icono por control).

Propósito: identificar los 12 controles de formato de FR-012 que F-004 activa (énfasis, encabezados, listas, cita, código, enlace, imagen, tabla, nota al pie, fórmula, diagrama, índice) y exponer su nombre accesible (NFR-009).

Origen: reutiliza la biblioteca MIT-compatible de F-001 (VR-001); atribución en SC-003 (NFR-011); sin glifos de texto.

Ubicación/referencia: SC-001 — barra de formato y menú Formato; superficie ventana-principal.

Formato y dimensiones: un icono por control; tamaño, altura y espaciado según TK-004; contraste AA en ambos temas.

Comportamiento visual: icono estable al pasar el puntero; el nombre accesible no se sustituye por el aviso flotante (VR-002); foco visible en el control.

Referencias:
- VR-001
- VR-002
- FR-002
- FR-012
- BR-016
- NFR-009
- NFR-011
- TK-004

## VR-011 — Estados de tabla editable

Tipo: Indicador visual (estado de control/tabla).

Propósito: comunicar la edición de tabla (celda activa, fila/columna, alineación, dentro/de los límites) en el panel activo (FR-002, NFR-016).

Origen: patrón de foco/estado de TK-005 y paleta de TK-001; sin recurso externo.

Ubicación/referencia: SC-001 — área de documento, tabla insertada; superficie ventana-principal.

Formato y dimensiones: borde de celda activa de 2 px en color de acento de TK-001; cabecera distinguible; contraste AA en ambos temas; no depende solo del color.

Comportamiento visual: se actualiza al navegar/editar; degradación sin bloqueo al superar límites (ERR-021); foco visible y operable por teclado; respeta prefers-reduced-motion (sin animación al cambiar de celda).

Referencias:
- FR-002
- BR-016
- NFR-009
- NFR-016
- ERR-021
- TK-001
- TK-005

## VR-012 — Renderizado de fórmula (LaTeX) y diagrama (Mermaid)

Tipo: Renderizado en línea (salida vectorial/texto).

Propósito: mostrar fórmulas en línea/en bloque y diagramas Mermaid en el panel izquierdo (BR-011, NFR-013).

Origen: motor de renderizado local (sin red); sin recurso gráfico propio.

Ubicación/referencia: SC-001 — panel izquierdo (WYSIWYG); superficie ventana-principal.

Formato y dimensiones: salida vectorial/texto accesible; tamaño según contenido y TK-002; contraste AA; ancho máximo del panel.

Comportamiento visual: aparece tras aplicar formato; si falla, marcador de error sin alterar el Markdown (ERR-019); el foco no se pierde; respeta prefers-reduced-motion (sin transición de aparición).

Referencias:
- FR-002
- BR-011
- BR-016
- NFR-009
- NFR-013
- ERR-019
- TK-002

## VR-013 — Escape de sintaxis (barra invertida)

Tipo: Indicador visual (marcado en el panel Markdown).

Propósito: señalar que la barra invertida escapa el carácter siguiente como literal (BR-009).

Origen: patrón de texto de TK-002; sin recurso externo.

Ubicación/referencia: SC-001 — panel derecho (Markdown); superficie ventana-principal.

Formato y dimensiones: carácter de escape visible en el código fuente; contraste AA; no depende solo del color.

Comportamiento visual: estático; no cambia el documento hasta confirmar; compatible con lectores de pantalla; foco visible.

Referencias:
- FR-002
- BR-009
- BR-016
- NFR-009
- TK-002