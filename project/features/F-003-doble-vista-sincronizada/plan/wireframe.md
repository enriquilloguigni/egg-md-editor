# Mockups — F-003 Doble vista sincronizada

Superficie nativa (Avalonia): wireframe ASCII. Tokens TK-xxx por nombre.

## SC-001 — Ventana principal (ventana-principal)

Superficie: ventana-principal
Estados visuales: A base — ambos paneles y barra visibles, tema claro; B — solo panel izquierdo; C — solo panel derecho; D — barra oculta; E — tema oscuro; F — resaltado pasajero visible.
Layout: ventana 1280×720 con título, menú, barra de formato opcional, dos paneles separados por splitter redimensionable, indicador de panel activo, resaltado pasajero temporal.
Tokens: TK-001, TK-003, TK-004, TK-005, TK-007, TK-008.

```
+--------------------------------------------------------------+
| Egg MD Editor — Sin título                      [_][□][X]    |
+--------------------------------------------------------------+
| Archivo  Edición  Ver  Formato  Herramientas  Ayuda           |
+--------------------------------------------------------------+
| [B][I][U]  [H1][H2][H3]  [•][1.][❝][</>]  [🔗][🖼] ...      |
+--------------------------------------------------------------+
|  Panel WYSIWYG (izq.)  |#|  Panel Markdown (der.)            |
|  renderizado puro      |#|  sintaxis visible y editable      |
|                        |#|                                  |
|  ┌─resaltado pasajero─┐|#|                                  |
|  │ sección equivalente│|#|                                  |
|  └────────────────────┘|#|                                  |
+--------------------------------------------------------------+
| Estado: panel activo = Izquierdo   | separador 4px (TK-008)  |
+--------------------------------------------------------------+
```

Notas: el splitter tiene ancho mínimo y es operable por teclado (flechas, Home/End); el resaltado desaparece solo y no altera el contenido; al menos un panel permanece visible; `prefers-reduced-motion` reduce la duración del resaltado a 0.