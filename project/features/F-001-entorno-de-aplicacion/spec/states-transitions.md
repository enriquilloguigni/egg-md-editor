# States and transitions — F-001 Entorno de aplicación

## Estados

- ST-001 — Ventana abierta (inicial) · ST-002 — Ventana cerrada (final)
- ST-003 — Idioma español activo (inicial) · ST-004 — Idioma inglés activo
- ST-005 — Menú cerrado (inicial) · ST-006 — Menú desplegado
- ST-007 — Aviso oculto (inicial) · ST-008 — Aviso visible
- ST-009 — Tema claro activo · ST-010 — Tema oscuro activo
- ST-011 — Rótulo de menú seleccionado (posición 1–6; inicial 1 Archivo)

| Aspecto | Estados |
|---|---|
| Estado inicial | ST-001, ST-003, ST-005, ST-007, ST-011 |
| Tema | ST-009 o ST-010 según el SO (preferencia inicial Sistema) |

## Transiciones

- TR-001 ST-001 → ST-002 | Evento: cerrar ventana | Condición: ventana abierta
- TR-002 ST-003 → ST-004 | Evento: conmutar idioma a inglés | Condición: sesión activa y catálogo destino carga (ERR-003 si falla)
- TR-003 ST-004 → ST-003 | Evento: conmutar idioma a español | Condición: idem
- TR-004 ST-005 → ST-006 | Evento: abrir rótulo seleccionado | Condición: ventana abierta
- TR-005 ST-006 → ST-005 | Evento: cerrar menú (Esc o clic fuera) | Condición: menú desplegado
- TR-006 ST-007 → ST-008 | Evento: pasar puntero sobre control de barra | Condición: control válido en ENT-005
- TR-007 ST-008 → ST-007 | Evento: retirar puntero del control | Condición: aviso visible
- TR-008 ST-009 → ST-010 | Evento: Tema → Oscuro, o Tema → Sistema con SO en oscuro | Condición: preferencias en memoria (ENT-003)
- TR-009 ST-010 → ST-009 | Evento: Tema → Claro, o Tema → Sistema con SO en claro | Condición: idem
- TR-010 ST-011 → ST-011 | Evento: botón «Siguiente» | Condición: rótulo actual i; destino (i mod 6) + 1
- TR-011 ST-011 → ST-011 | Evento: botón «Anterior» | Condición: rótulo actual i; destino ((i + 4) mod 6) + 1

## No permitidas

- Segunda ventana: sin transición a otra instancia; la existente recibe el foco (COM-005, BR-001).
- Conmutación con catálogo destino que no carga: se aborta y permanece el idioma activo (ERR-003).
- Rótulos/controles y atajos sin acción: no abren opciones internas ni alteran el documento (BR-F001-002/003, ADR-001).
- Tema fuera de {Claro,Oscuro,Sistema} o navegación fuera de 1–6: no hay transición; se conserva el estado válido.
- Cierre con excepción: el proceso termina igualmente, sin rollback (ERR-005).
