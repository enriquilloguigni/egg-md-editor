# Acceptance criteria — F-003 Doble vista sincronizada

AC-024 — Edición sincronizada en tiempo real (FR-001)

Dado: SC-001 abierta con un documento cargado y ambos paneles visibles.

Cuando: el usuario edita texto o formato en cualquiera de los paneles.

Entonces: el cambio aparece en el panel opuesto automáticamente, sin botón actualizar ni sondeo, y ambos reflejan el mismo documento (BR-002, BR-F003-001, BR-F003-004, COM-017); el árbol sintáctico se regenera y la conversión es reversible (BR-F003-008).

AC-025 — Correspondencia de posición y resaltado pasajero (FR-001)

Dado: SC-001 con ambos paneles visibles y un documento con varias secciones.

Cuando: el usuario cambia el foco de un panel al otro.

Entonces: el panel no activo se desplaza a la sección equivalente y esa sección se resalta brevemente y desaparece sola; el contenido no se modifica (BR-F003-005, BR-F003-006, BR-014, COM-018, COM-019, VR-007); si no hay ancla, no se desplaza ni se resalta y se registra (ERR-018).

AC-026 — Mostrar/ocultar paneles (FR-001, FR-009)

Dado: SC-001 con ambos paneles visibles.

Cuando: el usuario oculta o muestra un panel desde el menú Ver o su atajo.

Entonces: el panel indicado se oculta o aparece; el contenido y el estado de edición se conservan; al menos un panel permanece visible; al volver a mostrarlo se re-renderiza desde el modelo sin pérdida (BR-F003-007, COM-020, ST-018–ST-020); la preferencia se persiste (BR-019).

AC-027 — Mostrar/ocultar barra de formato (FR-001, FR-009, BR-019)

Dado: SC-001 abierta.

Cuando: el usuario oculta o muestra la barra de formato.

Entonces: la barra desaparece o aparece sin bloquear la edición por menú y atajos; la barra actúa sobre el panel activo; la preferencia persiste entre sesiones (BR-F003-007, BR-016, COM-021, ST-021/ST-022, VR-009).

AC-028 — Rendimiento de sincronía (NFR-001, NFR-003)

Dado: un documento de hasta 50 000 palabras (y hasta 100 000 palabras / 10 000 líneas).

Cuando: el usuario edita en cualquiera de los paneles.

Entonces: el cambio se refleja en el panel opuesto en <200 ms en el caso de 50 000 palabras y sin degradación perceptible >1 s hasta el límite; si se supera el umbral se degrada sin bloquear la edición y se registra (NFR-001, NFR-003, ERR-017, LOG-009).

AC-029 — Accesibilidad de la doble vista (NFR-009)

Dado: SC-001 con ambos paneles visibles.

Cuando: el usuario navega solo con teclado y lector de pantalla.

Entonces: ambos paneles y el separador son operables por teclado con foco visible; el panel activo se comunica de forma no solo cromática; el resaltado pasajero no interfiere con la selección ni se anuncia como alerta; se respeta `prefers-reduced-motion` (NFR-009, VR-007–VR-009).

DS-003 — Sincronización bidireccional verificable (FR-001, NFR-001)

Métrica:
- 100 % de las ediciones en un panel se reflejan en el otro en <200 ms para documentos de hasta 50 000 palabras; 0 pérdidas de contenido en 100 ciclos de edición sincronizada; correspondencia de posición correcta en ≥95 % de los cambios de panel; resaltado pasajero no modifica el contenido (0 mutaciones); conmutar visibilidad conserva contenido y estado en 100 % de los casos; cobertura unitaria de la lógica de sincronización ≥ 70 % (NFR-007); 0 accesos de red/telemetría (BR-012).

```
| Criterio | IDs |
|---|---|
| AC-024 | FR-001, SC-001, BR-002, BR-F003-001, BR-F003-004, BR-F003-008, COM-017, ENT-016 |
| AC-025 | FR-001, SC-001, BR-014, BR-F003-005, BR-F003-006, COM-018, COM-019, ENT-017, ENT-018, VR-007, ERR-018 |
| AC-026 | FR-001, FR-009, SC-001, BR-019, BR-F003-007, COM-020, ENT-019, ST-018, ST-019, ST-020 |
| AC-027 | FR-001, FR-009, BR-016, BR-019, BR-F003-007, COM-021, ST-021, ST-022, VR-009 |
| AC-028 | FR-001, NFR-001, NFR-003, ERR-017, LOG-009 |
| AC-029 | FR-001, NFR-009, VR-007, VR-008, VR-009 |
| DS-003 | FR-001, NFR-001, NFR-003, NFR-007, BR-012 |
```