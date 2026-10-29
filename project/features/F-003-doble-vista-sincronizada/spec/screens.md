# Screens — F-003 Doble vista sincronizada

## SC-001 — Ventana principal (F-003)

Propósito: alojar el documento único en dos paneles sincronizados —izquierdo WYSIWYG puro, derecho Markdown estándar— y exponer los conmutadores de visibilidad de paneles y barra de formato, sobre la ventana de F-001/F-002.

Elementos: barra de título con nombre e indicador de cambios; menú principal (F-001) con Ver → Mostrar panel izquierdo/derecho y Mostrar barra de formato; barra de formato (F-001, 12 controles) opcional; separador redimensionable entre paneles; panel izquierdo WYSIWYG puro sin sintaxis visible; panel derecho Markdown estándar editable con resaltado de sintaxis; área de desplazamiento sincronizado; resaltado pasajero sobre la sección equivalente; indicador de panel activo.

Campos: Panel izquierdo — contenido renderizado (solo lectura visual, editable WYSIWYG); Panel derecho — texto Markdown (editable, con sintaxis visible); Separador — posición (redimensionable, persistente); Panel activo — {Izquierdo, Derecho}; Visibilidad panel izquierdo — booleano (def. visible); Visibilidad panel derecho — booleano (def. visible); Visibilidad barra de formato — booleano (def. visible); Resaltado pasajero — rango temporal (auto-desaparece).

Acciones: Editar en cualquiera de los paneles; conmutar visibilidad de panel izquierdo/derecho/barra (menú Ver o atajo); redimensionar separador; desplazar (scroll) sincronizado; cambiar panel activo con Tab/foco; aplicar formato desde la barra al panel activo; mostrar/ocultar resaltado.

Mensajes: sin mensajes propios de éxito; los errores de sincronización/renderizado se presentan de forma no modal en SC-001 (ERR-016–ERR-019); el estado de cambios y el nombre se comunican por el título (BR-F002-016).

Estados: ambos paneles visibles; solo izquierdo; solo derecho; barra visible/oculta; panel activo izquierdo/derecho; sincronización activa/degradada; resaltado pasajero visible/oculto; ventana con foco.

## Crítica

- Si ambos paneles se ocultan a la vez, quedaría sin área de edición: el sistema impide ocultar los dos simultáneamente (al menos un panel permanece visible); es restricción de BR-F003-007.
- El separador redimensionable debe tener tamaño mínimo para no colapsar un panel; accesibilidad de teclado para redimensionar (flechas) y foco visible (NFR-009).
- El resaltado pasajero no debe interferir con la selección ni con lectores de pantalla: se anuncia como cambio de foco/región, no como alerta.
- En documentos muy grandes, el scroll sincronizado puede saltar; se degrada sin bloquear (ERR-017) y se registra (LOG-009).