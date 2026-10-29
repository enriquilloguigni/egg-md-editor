# Visual Resources — F-003 Doble vista sincronizada

## VR-007 — Resaltado pasajero de sección correspondiente
Tipo: Indicador visual (fondo temporal sobre un rango).
Propósito: señalar brevemente la sección equivalente al cambiar de panel (BR-014, BR-F003-006).
Origen: patrón de estado de control de TK-005 y paleta de TK-001; sin recurso externo.
Ubicación/referencia: SC-001 — panel destino; superficie ventana-principal.
Formato y dimensiones: fondo semitransparente sobre el rango, duración breve (def. 1,5 s), contraste AA; no depende solo del color (icono/borde complementario).
Comportamiento visual: aparece al cambiar de panel y desaparece solo; no modifica el contenido ni la selección; se desactiva con `prefers-reduced-motion` (duración 0) y se respeta `prefers-contrast`.

## VR-008 — Separador redimensionable entre paneles
Tipo: Control de interfaz (splitter).
Propósito: dividir y redimensionar los dos paneles en SC-001 (FR-001).
Origen: control estándar de Avalonia; sin recurso gráfico propio.
Ubicación/referencia: SC-001 — entre panel izquierdo y derecho; superficie ventana-principal.
Formato y dimensiones: línea vertical de 4 px, color de borde de TK-001, cursor de redimensionado; foco visible y operable por teclado (flechas, Home/End).
Comportamiento visual: estático; muestra el cursor de redimensionado al pasar; el ancho se persiste en `ENT-019`.

## VR-009 — Indicador de panel activo
Tipo: Indicador visual (borde/foco).
Propósito: comunicar qué panel recibe la entrada y la barra de formato (BR-016, NFR-009).
Origen: patrón de foco de TK-005; sin recurso externo.
Ubicación/referencia: SC-001 — borde del panel activo; superficie ventana-principal.
Formato y dimensiones: borde de 2 px en color de acento de TK-001, contraste AA en ambos temas; visible además de cromático.
Comportamiento visual: se mueve al cambiar el foco; no depende solo del color (grosor + acento).

No requerido / Fuera de alcance: los selectores nativos del SO no aportan recursos gráficos propios; los iconos de los conmutadores de visibilidad reutilizan la biblioteca MIT-compatible de F-001 (NFR-011).