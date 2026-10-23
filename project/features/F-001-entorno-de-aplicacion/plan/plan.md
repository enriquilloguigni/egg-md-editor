F-001, Spec: v1

## Dependencias

### Features
No requerida — F-001 no depende de otra Feature (roadmap v1).

### Componentes/módulos
- .NET SDK 10.0.x, TFM net10.0, Avalonia UI (FluentTheme) (architecture.md).
- Microsoft.Extensions.Logging (salida por stdout/stderr, NFR-010).

### Servicios externos
No requerida — 100 % local/offline (BR-012, NFR-004, NFR-005, C-013).

### Datos/estructuras
ENT/OP definidos en spec/data-structures.md; todos en memoria en F-001 (sin persistencia; FR-013 → F-010).

| ENT/OP | Disposición técnica |
|---|---|
| ENT-001 Sesión | Singleton en memoria por proceso; liberada al cerrar. |
| ENT-002 Ventana principal | Avalonia Window única (SC-001). |
| ENT-003 Preferencias UI | In-memory (idioma, tema); sin disco en F-001. |
| ENT-004 Menú principal | Estructura visual 6 menús; rótulos desde ENT-006. |
| ENT-005 Barra de formato | Controles en orden; icono + tooltip por control. |
| ENT-006 Catálogo i18n | Recursos .resx es/en con fallback a español. |
| ENT-007 Atajo | Bindings Avalonia KeyBinding. |
| OP-001..OP-006 | Métodos del ViewModel/dispatcher (idioma, tema, menú, aviso, atajo, cierre). |

### Configuración/infraestructura
- Directorio de configuración del usuario reservado (A-008), no usado en F-001.
- Build/ejecución local desde código fuente (C-013).

### Librerías/paquetes
- Librería de iconos MIT-compatible (selección en Implementation, NFR-011).
- Paquetes Avalonia (Controls, Themes.Fluent).

### Otras
- ADR-001 — parte visual de FR-012 sin acciones conectadas.

## Riesgos

- Arranque fallido por catálogo i18n o runtime (ERR-001/ERR-002): sin ventana usable, código de error, reintento por relanzamiento; LOG-001.
- Claves i18n sin resolver al conmutar (ERR-004): fallback a español; registrar en LOG-002.
- Cierre con excepción (ERR-005): proceso termina igualmente, sin residuales; LOG-003.
- Incompatibilidad de versión Avalonia/.NET 10: validar en T-001/T-013.
- Accesibilidad incompleta (nombres accesibles, foco): revisión NFR-009 sobre SC-001.

## Secuencia

1. Preparación: T-001, T-002 — proyecto, tema, recursos.
2. Datos: T-003, T-004 — catálogo i18n y sesión/preferencias en memoria.
3. Backend/servicios: T-005 — dispatcher de comandos y atajos.
4. Frontend/interfaz: T-006..T-012 — ventana SC-001, menú, barra, idioma, tema, avisos, atajos, ventana única.
5. Integraciones: No requerida.
6. Configuración: T-013 — errores, logging y cierre.
7. Integración final: T-014 + pruebas — superficies SC-002/SC-003 y verificación AC-001..AC-007, DS-001.

## Tareas

- T-001 Crear proyecto Avalonia .NET 10 (app + tests) con csproj y TFM net10.0.
- T-002 Configurar App.axaml con FluentTheme claro/oscuro y recursos i18n base.
- T-003 Implementar catálogo i18n es/en con fallback a español (ENT-006, COM-002, ERR-004).
- T-004 Implementar sesión y preferencias en memoria (ENT-001, ENT-003): idioma, tema, estado de menú y aviso.
- T-005 Implementar dispatcher de comandos compartidos y registro de atajos (ENT-007, COM-001) sin acciones conectadas (ADR-001).
- T-006 Crear ventana principal SC-001 con menú principal de 6 menús al 100 % (ENT-004, FR-011), solo visual.
- T-007 Crear barra de formato con controles FR-012 en orden, icono, tooltip y nombre accesible (ENT-005, VR-001, VR-002).
- T-008 Implementar conmutación de idioma en caliente con aborto sin cambio parcial (OP-001, TR-002/TR-003, ERR-003).
- T-009 Implementar conmutación de tema claro/oscuro heredado del SO (OP-002, TR-008/TR-009).
- T-010 Implementar apertura/cierre de menú y aviso flotante (OP-003/OP-004, COM-004/COM-006, TR-004..TR-007).
- T-011 Implementar handler de atajos disponibles en F-001; resto inertes (OP-005, BR-F001-003).
- T-012 Implementar ventana única con foco en segunda instancia y cierre de ventana (COM-005, OP-006, TR-001).
- T-013 Implementar errores de arranque/pintado/cierre y logging (ERR-001..ERR-005, LOG-001..LOG-003).
- T-014 Implementar superficies SC-002 (diálogo genérico) y SC-003 (Acerca de/guía) visibles, sin contenido conectado.
