# Process — F-003 Doble vista sincronizada

## Inicio

- **Proceso de acceso:** el usuario ya tiene abierta `SC-001` (F-001/F-002) con un documento cargado (BR-001); accede a la doble vista de forma automática al abrir la ventana y a los conmutadores de visibilidad desde el menú Ver y sus atajos. Sin autenticación ni cuentas (BR-012, NFR-004).
- **Condiciones de acceso:** aplicación local en ejecución; un único documento por instancia; lógica de sincronización desacoplada de la UI (NFR-007); soporte Windows 10+/macOS 12+/Linux de .NET 10 (NFR-008).
- **Datos requeridos:** el documento único (`ENT-008`); preferencias de visibilidad de paneles y barra (BR-019, FR-013). No se requieren datos externos.
- **Precondiciones:** F-002 completada (documento cargado con contenido UTF-8); parser CommonMark+GFM disponible (C-009, BR-009); modelo único compartido por ambos paneles (BR-001, C-002).

## Flujo principal

1. Al mostrar `SC-001`, el sistema instancia los dos paneles sobre el mismo modelo: izquierdo WYSIWYG puro y derecho Markdown estándar (BR-003, BR-F003-002/003).
2. El parser CommonMark+GFM construye el árbol sintáctico del documento (`ENT-016`) y cada panel renderiza su vista (OP-020, OP-021).
3. El usuario edita en cualquiera de los paneles; el cambio se propaga al modelo y al panel opuesto por evento, sin botón actualizar ni sondeo (C-002, BR-F003-004, COM-017).
4. Tras propagar, el sistema recalcula el mapa de correspondencia de posiciones (`ENT-017`) y, al cambiar el panel activo, desplaza el otro a la sección equivalente (BR-F003-005, COM-018).
5. Al cambiar de panel activo, la sección equivalente se resalta brevemente y desaparece sola; no modifica el contenido (BR-014, BR-F003-006, COM-019, VR-007).
6. El usuario puede ocultar/mostrar el panel izquierdo, el derecho o la barra de formato; el contenido y el estado de edición se conservan (BR-F003-007, COM-020).
7. La barra de formato actúa sobre el panel activo: efecto visual en el izquierdo, sintaxis equivalente en el derecho (BR-016, COM-021).
8. El desplazamiento se sincroniza entre paneles cuando ambos están visibles (FR-009).

## Flujos alternativos y de excepción

- **Alternativo — un solo panel visible:** si se oculta un panel, la sincronización sigue activa y al volver a mostrarlo se re-renderiza desde el modelo sin pérdida (BR-F003-007).
- **Alternativo — barra oculta:** la edición sigue disponible por menú y atajos; la barra se puede volver a mostrar (BR-F003-007, NFR-019).
- **Excepción — documento muy grande:** si la sincronía supera el umbral, se degrada sin bloquear la edición y se registra (ERR-017, NFR-001/NFR-003).
- **Excepción — posición sin correspondencia:** si no existe ancla equivalente, no se desplaza y no se resalta; se registra (ERR-018).
- **Excepción — fallo de sincronización:** se reintenta y se avisa de forma no modal; el documento no se pierde (ERR-016).
- **Excepción — error de renderizado WYSIWYG** (LaTeX/Mermaid): se muestra marcador de error en el panel izquierdo sin alterar el Markdown (ERR-019, BR-011).

## Resultado observable y finalización

- **Resultado:** ambos paneles reflejan el mismo documento en tiempo real con correspondencia de posición y resaltado pasajero; el usuario puede mostrar/ocultar paneles y barra; ninguna edición se pierde ni requiere botón actualizar (FR-001, BR-002, BR-003, BR-014).
- **Finalización:** al cerrar `SC-001` se conservan las preferencias de visibilidad para la siguiente sesión (BR-019, FR-013); las demás capacidades (formato completo, edición avanzada, exportación) quedan fuera de F-003 (F-002/F-004…).