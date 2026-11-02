# States and transitions — F-004 Formato Markdown completo

## Estados

- ST-023 — Tabla en edición (inicial al insertar/editar tabla)
- ST-024 — Celda activa
- ST-025 — Fórmula renderizada (inicial tras inserción)
- ST-026 — Fórmula con error de renderizado
- ST-027 — Diagrama renderizado (inicial tras inserción)
- ST-028 — Diagrama con error de renderizado
- ST-029 — Enlace pendiente de confirmación (inicial al activar enlace externo)
- ST-030 — Enlace abierto (final)
- ST-031 — Texto escapado
- ST-032 — Texto sin escapar (inicial)
- ST-033 — Encabezado en nivel actual (inicial según inserción)

| Aspecto | Estados |
|---|---|
| Estado inicial | ST-023 (tabla), ST-025 (fórmula), ST-027 (diagrama), ST-032 (texto), ST-033 (encabezado) |
| Enlace externo | ST-029 → ST-030 tras confirmación |
| Celda activa | ST-024 dentro de ST-023 |
| Renderizado | ST-025/ST-027 OK · ST-026/ST-028 error |

## Transiciones

| id | origen → destino | evento | condición |
|---|---|---|---|
| TR-042 | ST-023 → ST-024 | Activar celda | tabla en edición |
| TR-043 | ST-024 → ST-023 | Cerrar edición de celda | celda activa |
| TR-044 | ST-023 → ST-023 | Editar celda (OP-033) | índices válidos |
| TR-045 | ST-025 → ST-026 | Fallo de renderizado LaTeX | código inválido (ERR-019) |
| TR-046 | ST-026 → ST-025 | Corrección y re-render exitoso | código válido |
| TR-047 | ST-025 → ST-025 | Editar código (OP-030) | código no vacío |
| TR-048 | ST-027 → ST-028 | Fallo de renderizado Mermaid | código inválido (ERR-019) |
| TR-049 | ST-028 → ST-027 | Corrección y re-render exitoso | código válido |
| TR-050 | ST-029 → ST-030 | Confirmar apertura de enlace externo | usuario confirma (BR-F004-006, ERR-023 si cancela) |
| TR-051 | ST-032 → ST-031 | Anteponer barra invertida | carácter escapable (BR-009) |
| TR-052 | ST-031 → ST-032 | Eliminar barra invertida | texto escapado |
| TR-053 | ST-033 → ST-033 | Promover encabezado (OP-025) | nivel < 6 (BR-F004-002) |
| TR-054 | ST-033 → ST-033 | Degradar encabezado (OP-026) | nivel > 1 (BR-F004-002) |

## No permitidas

- Editar celda fuera de índices: no hay transición; permanece ST-023 (ERR-021 si excede límites de tabla).
- Renderizar fórmula o diagrama con código vacío: no hay transición a ST-025/ST-027; permanece el estado previo (BR-F004-010/011).
- Abrir enlace externo sin confirmación: no hay transición a ST-030; permanece ST-029 y se cancela (ERR-023).
- Escapar carácter no escapable o fuera del panel Markdown: no hay transición; permanece ST-032 (BR-009).
- Promover por encima de nivel 6 o degradar por debajo de nivel 1: no hay transición; permanece ST-033 (BR-F004-002, OP-025/026).
- Insertar tabla por encima de 1 000×50: no hay transición completa; se degrada sin bloquear (ERR-021, NFR-016).