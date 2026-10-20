# Visual Resources — F-002 Gestión de archivo

La única superficie nueva de F-002 con recurso visual propio es el aviso modal `SC-002` sobre `SC-001`, reutilizada de F-001; los elementos que la Feature añade sobre `SC-001` se rigen por lo ya vigente y no añaden recursos nuevos.

Los recursos siguen el patrón de superficie diálogo del design-system y comparten los tokens TK-001, TK-002 y TK-004 con las entregas visuales de F-001 (simetría), en ambos temas de la paleta; la iconografía del aviso no requiere recurso nuevo.

```
VR-005 — Acción primaria del aviso de cambios sin guardar
Tipo: Acción visual (botón de opción primaria).
Propósito: materializar la parte visual del aviso de cambios sin guardar (FR-004) presentando
  «Guardar y continuar», opción segura y por defecto del diálogo (SC-002, PD-FEAT-003).
Origen: patrón existente del design-system (superficie diálogo: acciones primaria/secundaria); sin
  recurso ni token nuevo.
Ubicación/referencia: SC-002 — Aviso de cambios sin guardar · botón «Guardar y continuar»; superficie
  diálogo, modal sobre SC-001.
Formato y dimensiones: botón con rótulo i18n es/en (BR-007); altura, espaciado y retícula de controles
  según TK-004; color de acento de la superficie con contraste AA en ambos temas según TK-001;
  tipografía de la UI según TK-002.
Comportamiento visual: recibe el foco inicial al mostrarse el aviso como opción por defecto (SC-002);
  se distingue de las acciones secundarias VR-006 por el acento, y su estado de foco sigue los estados
  de TK-004; resuelto el aviso, desaparece con él (SC-002).

VR-006 — Acciones secundarias del aviso de cambios sin guardar
Tipo: Acción visual (botones de opción secundaria).
Propósito: presentar «Continuar sin guardar» y «Cancelar» (FR-004, PD-FEAT-003) por debajo de la
  acción primaria, de modo que la opción segura destaque en el aviso (SC-002).
Origen: patrón existente del design-system (superficie diálogo: acciones primaria/secundaria); sin
  recurso ni token nuevo.
Ubicación/referencia: SC-002 — Aviso de cambios sin guardar · botones «Continuar sin guardar» y
  «Cancelar», junto a la acción primaria VR-005; superficie diálogo.
Formato y dimensiones: botones con rótulo i18n es/en (BR-007) en estilo secundario, sin énfasis de
  acento; retícula según TK-004 y contraste AA según TK-001; tipografía según TK-002.
Comportamiento visual: reciben el foco por navegación después de la acción primaria (estado de foco
  según TK-004); sin estilo de riesgo ni icono — mantienen la jerarquía primaria/secundaria del patrón;
  la opción elegida resuelve y el aviso desaparece con ellos (SC-002).

Sin recurso nuevo — aplicado y declarado:
- Título y mensaje del aviso (SC-002): presentación aplicada, sin recurso nuevo — título en el paso
  superior de la escala base de TK-002 y mensaje en el paso base, contraste AA según TK-001 en ambos
  temas, retícula de TK-004; textos canónicos del catálogo i18n (BR-007) en screens.md.
- Iconografía del aviso: No requerida — la iconografía vigente es la de la barra de formato (patrón
  ventana-principal) y el patrón superficie diálogo no define icono para la confirmación de cambios
  sin guardar; el aviso se comunica con título, mensaje y botones (SC-002).
- Elementos añadidos a SC-001 (área de documento, sublista Recientes, destino de arrastre): No
  requeridos — retícula, tipografía y estados ya vigentes en SC-001 según F-001; sin icono, imagen
  ni token nuevo.
- Indicación visual de fallo de las operaciones de archivo: Fuera de alcance — se define en
  error-handling.md apoyada en los estados de TK-004, sin recurso propio de esta Feature.
```

Separación con screens: screens.md define qué es cada superficie y sus elementos —el título, el mensaje y los tres botones de `SC-002` con sus efectos, y los elementos añadidos a `SC-001`—; este documento define solo su recurso visual: jerarquía de acciones, foco por defecto y tokens aplicados. Ningún recurso requiere un token distinto de TK-001, TK-002 y TK-004.
