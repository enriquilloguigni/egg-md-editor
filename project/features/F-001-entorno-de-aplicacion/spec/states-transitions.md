# States & Transitions — F-001 Entorno de aplicación

## Estados

### ST-001 — Sesión en arranque

Proceso lanzado desde el código fuente; la ventana `SC-001` aún no se presenta. Estado inicial.

### ST-002 — Entorno operativo — español

`SC-001` presentada con el 100 % del menú y la barra de formato visibles; idioma activo: español (BR-F001-004).

### ST-003 — Entorno operativo — inglés

`SC-001` presentada como en ST-002; idioma activo: inglés, tras conmutación en caliente.

### ST-004 — Sesión finalizada

`SC-001` cerrada, recursos liberados sin procesos residuales; la elección de idioma no pervive. Estado final.

Estado inicial: `ST-001`. Estado final: `ST-004`. Los estados transitorios de superficie (menú desplegado o cerrado, aviso flotante visible u oculto) no son estados funcionales: se describen en `process.md` y `inapp-communication.md` (COM-003).

## Transiciones

| id | origen | destino | evento | condición |
|---|---|---|---|---|
| TR-001 | ST-001 | ST-002 | Creación y presentación completa de `SC-001` | Inicialización correcta: catálogo i18n inicial cargado; el idioma de arranque es siempre español (BR-F001-004) |
| TR-002 | ST-002 | ST-003 | El usuario alterna el idioma a inglés (Herramientas → Idioma de la interfaz) | Catálogo i18n de inglés disponible; relabelizado en caliente conservando ventana y estado (BR-F001-005) |
| TR-003 | ST-003 | ST-002 | El usuario alterna el idioma a español | Catálogo i18n de español disponible; relabelizado en caliente conservando ventana y estado (BR-F001-005) |
| TR-004 | ST-002 | ST-004 | El usuario cierra `SC-001` | Liberación de los datos de sesión; sin persistencia de la elección de idioma (BR-F001-006) |
| TR-005 | ST-003 | ST-004 | El usuario cierra `SC-001` | Igual que TR-004 |

## Transiciones no permitidas

- `ST-001 → ST-003`: no existe arranque directo en inglés; el idioma inicial es siempre español (BR-F001-004) → `error-handling.md` no aplica, es regla.
- `ST-002/ST-003 → ST-002/ST-003` fallida por catálogo i18n del idioma destino no disponible: la conmutación se aborta sin cambio parcial y la sesión permanece en su estado actual → `ERR-003` (con `ERR-004` como fallback de claves sin resolver).
- `ST-004 → ST-001/ST-002/ST-003`: la sesión no se reanuda; cada nuevo lanzamiento crea una sesión nueva en `ST-001`.
- Solicitud de segunda ventana mientras la sesión está en `ST-002`/`ST-003`: sin transición; la ventana existente recibe el foco (BR-003).
- Fallo en `ST-001` durante inicialización o pintado: sin transición a `ST-002`; la sesión termina sin ventana usable → `ERR-001`, `ERR-002`.
- Fallo al cerrar en `ST-004`: la finalización se completa igualmente, sin procesos residuales → `ERR-005`.
