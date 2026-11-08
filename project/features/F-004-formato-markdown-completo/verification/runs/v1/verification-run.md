Feature: F-004 — Formato Markdown completo
Feature Spec: v1
Plan: v1

## Elementos a comprobar

| Dimensión | IDs | Comprobación |
|---|---|---|
| Funcionales | FR-002, FR-012, NFR-003, NFR-007, NFR-009, NFR-013, NFR-016, NFR-017, NFR-019, AC-030..AC-045, DS-004 | Énfasis, encabezados, listas, citas, código, enlaces, imágenes, tablas, notas al pie, fórmulas, diagramas, escape, barra de formato sobre panel activo, sanitización, rendimiento y accesibilidad. |
| Reglas | BR-009, BR-011, BR-016, BR-018, BR-F004-001..BR-F004-013 | CommonMark+GFM; fórmulas/diagramas en panel izquierdo; barra sobre selección/estructura; enlaces a secciones por encabezado; escape literal; límites de tabla. |
| Pantallas | SC-001, VR-010, VR-011, VR-012 | Ventana principal reutilizada con menú/barra Formato y diálogos de formato sobre SC-002; estados de tabla, fórmula y diagrama. |
| Datos | ENT-020..ENT-031, OP-025..OP-034 | Nodos de formato tipados en el AST compartido y operaciones de formato; sin persistencia propia. |
| Seguridad | security-permissions.md, NFR-004, NFR-005, NFR-010, NFR-017 | 100 % local/offline; sanitización de HTML/script y enlaces peligrosos al abrir Markdown. |
| Comunicación interna | COM-021, COM-022, COM-023 | Barra de formato sobre panel activo; motor de formato sobre AST compartido. |
| Externos | external-services.md, NFR-004 | No aplica: 0 servicios externos; comprobación de ausencia de red. |
| Errores | ERR-019, ERR-020, ERR-021, ERR-022, ERR-023, ERR-024, LOG-012..LOG-016 | Renderizado fallido de fórmula/diagrama con marcador; sanitización; límites de tabla; error de parseo; enlaces externos sin confirmación; imágenes no encontradas. |
| Estados y transiciones | ST-023..ST-026, TR-040..TR-045 | Diálogo de formato abierto/cerrado y válido/inválido; tabla dentro/de los límites; renderizado ok/error; escape activo. |
| Compatibilidad y licencias | NFR-007, NFR-008, NFR-011 | Build net10.0; cobertura unitaria ≥70 %; dependencias MIT-compatibles. |
| AC y tareas del Plan | AC-030..AC-045, DS-004, T-065..T-090 | Cumplimiento de AC-030..AC-045 y DS-004; verificación de cada tarea del Plan. |

**Tareas del Plan y evidencia**

| Tarea | Comprobación |
|---|---|
| T-065 | Motor de formato sobre AST compartido (`T065FormatEngineTests`, ENT-020, COM-022, BR-F004-013). |
| T-066 | Énfasis negrita/cursiva/tachado (`T065FormatEngineTests`/`T090FormatUnitAndAcTests`, ENT-020, OP-034, BR-F004-001, AC-030). |
| T-067 | Encabezados 1–6 con promover/degradar (`T068HeadingAnchorTests`/`T090FormatUnitAndAcTests`, ENT-021, OP-025/026, BR-F004-002, AC-031). |
| T-068 | Ancla de encabezado y resolución de enlaces internos (`T068HeadingAnchorTests`/`T090FormatUnitAndAcTests`, ENT-021, BR-F004-002, BR-018, AC-035). |
| T-069 | Listas viñetas/numerada/tareas con anidado (`T069ListTests`/`T090FormatUnitAndAcTests`, ENT-022, OP-027/028, BR-F004-003, AC-032). |
| T-070 | Citas anidadas (`T070QuoteTests`/`T090FormatUnitAndAcTests`, ENT-023, BR-F004-004, AC-033). |
| T-071 | Código en línea y en bloque (`T081CodeHighlightTests`/`T090FormatUnitAndAcTests`, ENT-024, BR-F004-005, AC-034). |
| T-072 | Enlaces internos/externos con confirmación (`T090FormatUnitAndAcTests`, ENT-025, BR-F004-006, NFR-017, ERR-020/023, AC-035). |
| T-073 | Imágenes con alt obligatorio (`T090FormatUnitAndAcTests`, ENT-026, BR-F004-007, NFR-009, ERR-024, AC-036). |
| T-074 | Tablas editables con límites 1000×50 (`T090FormatUnitAndAcTests`, ENT-027, OP-029/033, BR-F004-008, NFR-016, ERR-021, AC-037). |
| T-075 | Notas al pie con id único (`T090FormatUnitAndAcTests`, ENT-028, OP-032, BR-F004-009, AC-038). |
| T-076 | Fórmulas LaTeX en línea/en bloque (`T090FormatUnitAndAcTests`, ENT-029, OP-030, BR-F004-010, BR-011, NFR-013, ERR-019, AC-039). |
| T-077 | Diagramas Mermaid en bloque (`T090FormatUnitAndAcTests`, ENT-030, OP-031, BR-F004-011, BR-011, NFR-013, ERR-019, AC-040). |
| T-078 | Escape de sintaxis con barra invertida (`T090FormatUnitAndAcTests`, ENT-031, BR-F004-012, BR-009, AC-041). |
| T-079 | Diálogos de formato sobre SC-002 (`DialogSurfaceTests`/`T090FormatUnitAndAcTests`, SC-001, NFR-009, AC-042). |
| T-080 | Menú Formato y barra sobre panel activo (`FormatBarTests`/`T090FormatUnitAndAcTests`, FR-012, BR-F004-013, COM-021/023, VR-010, AC-042). |
| T-081 | Resaltado de código con botón de copia (`T081CodeHighlightTests`/`T090FormatUnitAndAcTests`, AC-034). |
| T-082 | Renderizado de tablas con estados visuales (`T090FormatUnitAndAcTests`, VR-011, AC-037). |
| T-083 | Renderizado de fórmula/diagrama con marcador de error (`T090FormatUnitAndAcTests`, VR-012, AC-039/040). |
| T-084 | Sanitización al abrir Markdown (`T090FormatUnitAndAcTests`, NFR-017, ERR-020, LOG-014, AC-043). |
| T-085 | Manejo de error de parseo CommonMark/GFM (`T085ParseErrorHandlingTests`/`T090FormatUnitAndAcTests`, ERR-022, LOG-012, AC-044). |
| T-086 | Degradación en documentos grandes y límites de tabla (`T086DegradationLargeDocsTests`/`T090FormatUnitAndAcTests`, ERR-017/021, LOG-015/016, NFR-003/016, AC-044). |
| T-087 | Registro técnico LOG-012..LOG-016 (`T085ParseErrorHandlingTests`/`T086DegradationLargeDocsTests`/`T090FormatUnitAndAcTests`, LOG-012..LOG-016). |
| T-088 | Accesibilidad de controles y diálogos (`T088AccessibilityTests`/`T090FormatUnitAndAcTests`, NFR-009, AC-045, VR-010/011/012/013). |
| T-089 | Integración motor de formato con sincronización (`T089FormatSyncTests`/`T090FormatUnitAndAcTests`, COM-016/017, BR-F003-001/004, AC-030..AC-045). |
| T-090 | Pruebas unitarias ≥70 % y verificación AC-030..AC-045/DS-004 (`T090FormatUnitAndAcTests`, NFR-007, AC-030..AC-045, DS-004). |

## Actividades y pruebas

1. Ejecutar la suite automatizada completa con el comando anclado y exigir `exit_code=0`, sin fallos ni pruebas omitidas.
2. Comprobar énfasis, encabezados, listas, citas, código, enlaces, imágenes, tablas, notas al pie, fórmulas, diagramas y escape sobre el panel activo (AC-030..AC-041, BR-F004-001..BR-F004-012).
3. Comprobar la barra de formato y los diálogos sobre el panel activo y la sanitización al abrir Markdown (AC-042, AC-043, NFR-017, ERR-020).
4. Medir rendimiento en documento de 10 000 palabras (≤2 s) y degradación en documentos grandes sin bloqueo (AC-044, DS-004, NFR-003, NFR-013, NFR-016, NFR-019, ERR-017/021).
5. Verificar accesibilidad de controles, diálogos, tablas, fórmulas y diagramas (AC-045, NFR-009, VR-010/011/012/013).
6. Contrastar cobertura unitaria ≥70 % (NFR-007) con `evidence/coverage-run.json` si se ejecuta con cobertura.

Comando de prueba: dotnet test project/implementation/tests/EggMdEditor.App.Tests/EggMdEditor.App.Tests.csproj

## Condiciones

- Spec v1 y Plan v1 vigentes; la corrida se registra sobre el commit de la Execution E-010.
- Tareas T-065..T-090 en `done`; build local con .NET SDK 10.0.x, sin red ni servicios externos (BR-012, NFR-004).
- SC-001 reutilizado; no se crea pantalla nueva (renuncia declarada).
- Cobertura unitaria de la lógica de formato ≥70 % (NFR-007) contrastada con `evidence/coverage-run.json` si se ejecuta con cobertura.

## Evidencias a obtener

- `executions/E-010/evidence/test-run.json`, producida por `sdd-test-run`, con `exit_code=0` y `sha256` íntegro.
- Observaciones de las comprobaciones funcionales, de error y de seguridad consignadas por el reviewer en `results/v1/result.md`.

## Checks visuales

Checks visuales: SC-001
Estados visuales: SC-001: A
SC-001 estado A: base — ventana principal con menú/barra Formato.

Renuncia visual: SC-001/A — entorno de verificación sin display para captura headless.

## Criterio

Binario — `Plan cubierto = T-065..T-090 verificadas + Spec v1 cumplida + AC-030..AC-045 cumplidos + DS-004 alcanzado + 0 incidencias pendientes`, con `test-run.json` en `exit_code=0`. Cualquier tarea, AC o DS sin evidencia o cualquier incidencia abierta deja el resultado en `Plan no cubierto`.