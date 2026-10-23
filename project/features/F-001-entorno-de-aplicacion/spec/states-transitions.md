# States and transitions — F-001 Entorno de aplicación

## Estados

- ST-001 — Ventana abierta (inicial) · ST-002 — Ventana cerrada (final)
- ST-003 — Idioma español activo (inicial) · ST-004 — Idioma inglés activo
- ST-005 — Menú cerrado (inicial) · ST-006 — Menú desplegado
- ST-007 — Aviso oculto (inicial) · ST-008 — Aviso visible
- ST-009 — Tema claro (inicial por defecto del SO) · ST-010 — Tema oscuro

Estado inicial: ST-001, ST-003, ST-005, ST-007, ST-009.

## Transiciones

- TR-001 ST-001 → ST-002 | Evento: cerrar ventana | Condición: ventana abierta
- TR-002 ST-003 → ST-004 | Evento: conmutar idioma a inglés | Condición: sesión activa y catálogo destino carga (ERR-003 si falla)
- TR-003 ST-004 → ST-003 | Evento: conmutar idioma a español | Condición: idem
- TR-004 ST-005 → ST-006 | Evento: abrir entrada de menú | Condición: ventana abierta
- TR-005 ST-006 → ST-005 | Evento: cerrar menú (Esc o clic fuera) | Condición: menú desplegado
- TR-006 ST-007 → ST-008 | Evento: pasar puntero sobre control de barra | Condición: control válido en ENT-005
- TR-007 ST-008 → ST-007 | Evento: retirar puntero del control | Condición: aviso visible
- TR-008 ST-009 → ST-010 | Evento: conmutar tema a oscuro | Condición: preferencias disponibles (ENT-003)
- TR-009 ST-010 → ST-009 | Evento: conmutar tema a claro | Condición: idem

## No permitidas

- Segunda ventana: sin transición a otra instancia; la existente recibe el foco (COM-005, BR-001).
- Conmutación con catálogo destino que no carga: se aborta y permanece el idioma activo (ERR-003).
- Opciones de menú/control y atajos sin acción: no alteran ningún estado (BR-F001-002/003, ADR-001).
- Cierre con excepción: el proceso termina igualmente, sin rollback (ERR-005).