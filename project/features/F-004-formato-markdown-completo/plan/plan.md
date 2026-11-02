F-004, Spec: v1

## Dependencias

### Features
- F-003 — Doble vista sincronizada: formato actúa sobre el panel activo y ambos paneles sincronizan.

### Componentes/módulos
- .NET 10 + Avalonia UI, MVVM, parser CommonMark+GFM (C-009), adaptador de plataforma (A-008); renderizadores locales KaTeX y Mermaid; resaltador de sintaxis.

### Servicios externos
No requerida — 100 % local/offline (BR-012, NFR-004, NFR-005).

### Datos/estructuras
| ENT/OP | Disposición técnica |
|---|---|
| ENT-020..ENT-031 | Nodos de formato tipados en el AST compartido (ENT-016); sin persistencia propia. |
| OP-025..OP-034 | Operaciones de formato (énfasis, encabezado, anidado, tabla, fórmula, diagrama, nota, escape). |

### Configuración/infraestructura
- Activos locales de KaTeX/Mermaid incluidos en la aplicación; sin configuración persistente propia (A-008 reutilizado).

### Librerías/paquetes
- Parser CommonMark+GFM, KaTeX, Mermaid y resaltador, licencias MIT-compatibles (NFR-011); sin red.

### Otras
- ADR-001 (superficie sin acciones conectadas) se extiende con menú/barra Formato; C-002/C-009. Mockups: No requerido / reutiliza SC-001.

## Riesgos
- Renderizado de fórmula/diagrama fallido: marcador en panel izquierdo, Markdown intacto (ERR-019, BR-011).
- Tablas fuera de límites (1 000×50): informar y degradar sin bloquear (ERR-021, NFR-016).
- Sanitización bloqueada al abrir Markdown peligroso: neutralizar o confirmar (ERR-020, NFR-017).
- Error de parseo CommonMark/GFM: documento sigue editable en panel Markdown (ERR-022).
- Rendimiento en documentos grandes: formato sin bloquear hilo UI (NFR-003, NFR-019, AC-044).
- Enlaces externos sin confirmación: no abrir, cancelar acción (ERR-023, BR-F004-006).
- Imágenes locales no encontradas: marcador de imagen rota (ERR-024).
- Accesibilidad de controles y diálogos (NFR-009, AC-045).

## Secuencia
1. Preparación: T-065..T-078 — motor y nodos de formato.
2. Interfaz: T-079..T-083 — diálogos, menú/barra y renderizado.
3. Robustez: T-084..T-088 — sanitización, parseo, degradación, logs, accesibilidad.
4. Integración final: T-089, T-090 — sincronización y pruebas AC-030..AC-045/DS-004.

## Tareas
- T-065 Implementar motor de formato sobre el AST compartido (ENT-020, COM-022, BR-F004-013).
- T-066 Implementar aplicación de énfasis (negrita/cursiva/tachado) (ENT-020, OP-034, BR-F004-001, AC-030).
- T-067 Implementar encabezados nivel 1–6 con promover/degradar (ENT-021, OP-025/026, BR-F004-002, AC-031).
- T-068 Implementar ancla de encabezado y resolución de enlaces internos (ENT-021, BR-F004-002, BR-018, AC-035).
- T-069 Implementar listas (viñetas/numerada/tareas) con anidado y renumeración (ENT-022, OP-027/028, BR-F004-003, AC-032).
- T-070 Implementar citas anidadas por niveles (ENT-023, BR-F004-004, AC-033).
- T-071 Implementar código en línea y en bloque con lenguaje opcional (ENT-024, BR-F004-005, AC-034).
- T-072 Implementar enlaces internos/externos con confirmación de apertura (ENT-025, BR-F004-006, NFR-017, ERR-020/023, AC-035).
- T-073 Implementar imágenes con texto alternativo obligatorio y rutas locales (ENT-026, BR-F004-007, NFR-009, ERR-024, AC-036).
- T-074 Implementar tablas editables con límites 1 000×50 (ENT-027, OP-029/033, BR-F004-008, NFR-016, ERR-021, AC-037).
- T-075 Implementar notas al pie con identificador único (ENT-028, OP-032, BR-F004-009, AC-038).
- T-076 Implementar fórmulas LaTeX en línea/en bloque (ENT-029, OP-030, BR-F004-010, BR-011, NFR-013, ERR-019, AC-039).
- T-077 Implementar diagramas Mermaid en bloque (ENT-030, OP-031, BR-F004-011, BR-011, NFR-013, ERR-019, AC-040).
- T-078 Implementar escape de sintaxis con barra invertida (ENT-031, BR-F004-012, BR-009, AC-041).
- T-079 Implementar diálogos de formato sobre SC-002 (SC-001, NFR-009, AC-042).
- T-080 Implementar menú Formato y barra actuando sobre el panel activo (FR-012, BR-F004-013, COM-021/023, VR-010, AC-042).
- T-081 Implementar resaltado de código con botón de copia (AC-034).
- T-082 Implementar renderizado de tablas con estados visuales (VR-011, AC-037).
- T-083 Implementar renderizado de fórmula/diagrama con marcador de error (VR-012, AC-039/040).
- T-084 Implementar sanitización al abrir Markdown (NFR-017, ERR-020, LOG-014, AC-043).
- T-085 Implementar manejo de error de parseo CommonMark/GFM (ERR-022, LOG-012, AC-044).
- T-086 Implementar degradación en documentos grandes y límites de tabla (ERR-017/021, LOG-015/016, NFR-003/016, AC-044).
- T-087 Implementar registro técnico LOG-012..LOG-016.
- T-088 Implementar accesibilidad de controles y diálogos (NFR-009, AC-045, VR-010/011/012/013).
- T-089 Integrar motor de formato con sincronización de paneles (COM-016/017, BR-F003-001/004, AC-030..AC-045).
- T-090 Pruebas unitarias ≥70 % y verificación AC-030..AC-045/DS-004.