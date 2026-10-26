F-001, Spec: v2

## Dependencias

### Features
No requerida — F-001 no depende de otra Feature (roadmap v1).

### Componentes/módulos
- .NET SDK 10.0.x (TFM net10.0), Avalonia UI (FluentTheme) y Microsoft.Extensions.Logging (stdout/stderr, NFR-010).

### Servicios externos
No requerida — 100 % local/offline (BR-012, NFR-004, NFR-005, C-013).

### Datos/estructuras
ENT/OP en spec/data-structures.md; en memoria, sin persistencia (FR-013 → F-010).

| ENT/OP | Disposición técnica |
|---|---|
| ENT-001/002/003 | Sesión, ventana única SC-001 y preferencias en memoria: idioma; tema {Claro,Oscuro,Sistema}; rótulo seleccionado 1–6; sin disco. |
| ENT-004 | Menú de 6 rótulos como botones y navegación secuencial Siguiente/Anterior; rótulos desde ENT-006. |
| ENT-005 | Barra con 12 controles en orden: 1 Énfasis, 2 Encabezados, 3 Listas, 4 Cita, 5 Código, 6 Enlace, 7 Imagen, 8 Tabla, 9 Nota al pie, 10 Fórmula, 11 Diagrama, 12 Índice. |
| ENT-006/007 | Catálogo i18n .resx es/en con fallback a español; atajos por KeyBinding. |
| OP-001..OP-007 | ViewModel/dispatcher: idioma, tema, menú, aviso, atajo, cierre y navegación de rótulo. |

### Configuración/infraestructura
- Directorio de configuración reservado (A-008), no usado; build/ejecución local desde fuente (C-013).

### Librerías/paquetes
- Librería de iconos MIT-compatible, sin glifos de texto; atribución en SC-003 (NFR-011, VR-001). Paquetes Avalonia (Controls, Themes.Fluent).

### Otras
- ADR-001 (superficie sin acciones conectadas) y ADR-002 (Spec v2: inventario FR-012, tema visible, navegación secuencial, iconos, E2E y retirada de API especulativa).

## Riesgos

- Divergencia Spec/mockup tras ADR-002 (área de documento vacía y 12 controles): el Plan fija el inventario y la recaptura de SC-001 lo valida en PLAN_REVIEW.
- Botones «Siguiente/Anterior» sin foco, sin nombre accesible o con salto incorrecto: T-015 y AC-009 prueban el ciclo 1→6→1 sin abrir menú.
- Entrada Tema no visible o estado oscuro no capturable: T-016 enlaza las 3 opciones a OP-002/TR-008/TR-009 y AC-008 exige captura oscura.
- Iconos con licencia no MIT o sin atribución: T-017 verifica licencia y atribución visible en SC-003 (NFR-011, VR-001).
- E2E de segunda instancia/procesos residuales inestable entre plataformas: T-018 usa arnés de proceso con timeouts y limpieza; sin skip sin renuncia registrada.
- Retirada de alias rompe compilación o tests existentes: T-019 busca referencias antes de borrar y sustituye tests de compilación por pruebas de comportamiento.
- Arranque fallido (ERR-001/ERR-002) sin ventana usable: T-013 garantiza código de salida, stderr y LOG-001; reintento por relanzamiento.
- i18n/cierre (ERR-003/ERR-004/ERR-005): fallback a español, aborto sin cambio parcial y cierre sin residuales; T-003/T-013.

## Secuencia

1. Preparación: T-001, T-002 — proyecto, tema, recursos.
2. Datos: T-003, T-004 — catálogo i18n y sesión/preferencias en memoria.
3. Backend/servicios: T-005 — dispatcher de comandos y atajos.
4. Frontend/interfaz: T-006..T-012 y T-015..T-017 — ventana, menú/navegación, barra de 12 controles, idioma, tema, avisos, atajos, ventana única e iconos.
5. Integraciones: No requerida.
6. Configuración: T-013 y T-019 — errores, logging, cierre y retirada de API especulativa.
7. Integración final: T-014 y T-018 + verificación AC-001..AC-009 y DS-001.

## Tareas

- T-001 Crear proyecto Avalonia .NET 10 (app + tests) con csproj y TFM net10.0.
- T-002 Configurar App.axaml con FluentTheme claro/oscuro y recursos i18n base.
- T-003 Implementar catálogo i18n es/en con fallback a español (ENT-006, COM-002, ERR-004).
- T-004 Implementar sesión y preferencias en memoria (ENT-001, ENT-003): idioma, tema, estado de menú, rótulo seleccionado y aviso.
- T-005 Implementar dispatcher de comandos compartidos y registro de atajos (ENT-007, COM-001) sin acciones conectadas (ADR-001).
- T-006 Crear ventana principal SC-001 con área de documento vacía y contenedores de menú y barra (ENT-002, FR-011; ADR-002).
- T-007 Crear barra de formato con los 12 controles de FR-012 en orden, icono, tooltip y nombre accesible (ENT-005, VR-001, VR-002).
- T-008 Implementar conmutación de idioma en caliente con aborto sin cambio parcial (OP-001, TR-002/TR-003, ERR-003).
- T-009 Implementar resolución del tema efectivo Claro/Oscuro/Sistema y herencia del SO (OP-002, TR-008/TR-009).
- T-010 Implementar apertura/cierre de menú y aviso flotante (OP-003/OP-004, COM-004/COM-006, TR-004..TR-007).
- T-011 Implementar handler de atajos disponibles en F-001; resto inertes (OP-005, BR-F001-003).
- T-012 Implementar ventana única con foco en segunda instancia y cierre de ventana (COM-005, OP-006, TR-001).
- T-013 Implementar errores de arranque/pintado/cierre y logging (ERR-001..ERR-005, LOG-001..LOG-003).
- T-014 Implementar superficies SC-002 (diálogo genérico) y SC-003 (Acerca de/guía) visibles, sin contenido conectado.
- T-015 Mostrar los 6 rótulos de menú como botones y añadir 2 botones de navegación secuencial (Siguiente/Anterior) que aumentan/disminuyen el rótulo seleccionado con salto cíclico (ENT-004, OP-007, TR-010/TR-011, BR-F001-010).
- T-016 Añadir en Herramientas la entrada Tema (Claro/Oscuro/Sistema), enlazada a OP-002/TR-008/TR-009, con estado oscuro observable y capturable (ENT-003, BR-F001-009, AC-008).
- T-017 Sustituir los glifos de texto de los 12 controles por una librería de iconos MIT-compatible y mostrar su atribución en SC-003 (VR-001, NFR-011).
- T-018 Añadir pruebas E2E de AC-005/AC-006/AC-007: foco de segunda instancia, códigos de salida/stderr, ausencia de ventana usable/procesos residuales y no persistencia de idioma o tema.
- T-019 Eliminar alias/API especulativa (Compatibility.cs, SessionAliases.cs, CompatibilityNamespaces.cs y alias del dispatcher) y sustituir los tests de compilación de alias por pruebas de comportamiento.
