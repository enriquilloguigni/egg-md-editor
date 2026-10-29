# Business Rules — F-003 Doble vista sincronizada

Referencia (heredada, no reproducida): textos en Project Spec vigente.

Referencias heredadas:
- `FR-001` — doble vista sincronizada.
- `FR-009` — mostrar/ocultar vistas y barra de formato (parcial, lo relativo a paneles/barra).
- `FR-013` — persistencia de preferencias (visibilidad y ancho del separador).
- `BR-001` — un único documento por instancia.
- `BR-002` — dos paneles reflejan el mismo documento en tiempo real; sin botón actualizar.
- `BR-003` — panel izquierdo WYSIWYG puro, derecho Markdown estándar editable.
- `BR-009` — CommonMark + GFM.
- `BR-012` — 100 % local y offline.
- `BR-014` — resaltado de correspondencia pasajero, no modifica contenido.
- `BR-016` — la barra actúa según el panel activo.
- `BR-019` — preferencias de presentación persisten entre sesiones.
- `NFR-001`, `NFR-003`, `NFR-009`, `NFR-019` — sincronía, escalabilidad, accesibilidad y respuesta.
- `C-002`, `C-003`, `C-009`, `C-011`, `C-012`, `C-013`.

```
| id | condición | regla | consecuencia |
|---|---|---|---|
| BR-F003-001 | Cualquier edición | Un único modelo compartido; ambos paneles leen y escriben el mismo documento; no hay copia divergente | La verdad es el modelo; las vistas son proyecciones (BR-001) |
| BR-F003-002 | Panel izquierdo | WYSIWYG puro: no muestra sintaxis Markdown ni delimitadores; renderiza el formato | El usuario escribe sin ver marcas (BR-003) |
| BR-F003-003 | Panel derecho | Markdown estándar editable: muestra sintaxis CommonMark+GFM y es editable como texto | La sintaxis es visible y editable (BR-003, BR-009) |
| BR-F003-004 | Cualquier cambio de contenido | Sincronización bidireccional por eventos, sin botón actualizar ni sondeo; el panel opuesto se actualiza automáticamente | Tiempo real sin acción del usuario (C-002, NFR-001) |
| BR-F003-005 | Cambio de panel activo | El panel no activo se desplaza a la sección equivalente según el mapa de posiciones | Correspondencia de posición (FR-001) |
| BR-F003-006 | Cambio de panel activo | La sección equivalente se resalta brevemente y desaparece sola; no modifica el contenido | Resaltado pasajero (BR-014) |
| BR-F003-007 | Conmutar visibilidad | Se puede ocultar/mostrar cada panel y la barra; el contenido y el estado de edición se conservan; al menos un panel permanece visible | Preferencia de trabajo sin pérdida (BR-019, FR-009) |
| BR-F003-008 | Conversión entre vistas | El parser CommonMark+GFM es el contrato; la conversión sintaxis↔WYSIWYG es reversible y sin pérdida para el subconjunto soportado | Coherencia entre paneles (C-009, BR-009) |
```