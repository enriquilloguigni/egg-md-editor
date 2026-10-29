# States and transitions — F-003 Doble vista sincronizada

## Estados
- ST-018 — Ambos paneles visibles (estado inicial por defecto)
- ST-019 — Solo panel izquierdo visible
- ST-020 — Solo panel derecho visible
- ST-021 — Barra de formato visible
- ST-022 — Barra de formato oculta

| Aspecto | Estados |
|---|---|
| Estado inicial | ST-018 y ST-021 (ambos paneles y barra visibles) |
| Visibilidad de paneles | ST-018 (ambos) / ST-019 (solo izq.) / ST-020 (solo der.) |
| Visibilidad de barra | ST-021 (visible) / ST-022 (oculta) |
| Sincronización | activa mientras exista al menos un panel visible |

## Transiciones
```
| id | origen → destino | evento | condición |
|---|---|---|---|
| TR-034 | ST-018 → ST-019 | Ocultar panel derecho | panel izquierdo visible |
| TR-035 | ST-018 → ST-020 | Ocultar panel izquierdo | panel derecho visible |
| TR-036 | ST-019 → ST-018 | Mostrar panel derecho | — |
| TR-037 | ST-020 → ST-018 | Mostrar panel izquierdo | — |
| TR-038 | ST-021 → ST-022 | Ocultar barra de formato | — |
| TR-039 | ST-022 → ST-021 | Mostrar barra de formato | — |
| TR-040 | ST-018 → ST-018 | Editar en un panel | cambio propagado al modelo y al panel opuesto (COM-017) |
| TR-041 | ST-018 → ST-018 | Cambiar panel activo | ancla encontrada → desplazamiento + resaltado pasajero (COM-018/019) |
```

## No permitidas
- Ocultar ambos paneles a la vez: no hay transición; al menos un panel permanece visible (BR-F003-007, ERR-016).
- Sincronizar sin modelo válido o con el árbol parcial: no hay transición; se registra y se reintenta (ERR-016/017).
- Aplicar resaltado sin ancla encontrada: no hay transición de resaltado; se registra (ERR-018).
- Redimensionar el separador hasta colapsar un panel: no hay transición; se respeta el ancho mínimo (NFR-009).