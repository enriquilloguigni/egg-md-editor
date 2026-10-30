Feature: F-003 — Doble vista sincronizada
Feature Spec: v1
Plan: v1

## Elementos a comprobar

| Dimensión | IDs | Comprobación |
|---|---|---|
| Funcionales | FR-001, FR-009, NFR-001, NFR-003, NFR-009, NFR-019, AC-024..AC-029, DS-003 | Edición sincronizada bidireccional sin botón ni sondeo, correspondencia de posición y resaltado pasajero, conmutación de visibilidad de paneles y barra, persistencia de preferencias, rendimiento <200 ms hasta 50 000 palabras y hasta 100 000 palabras/10 000 líneas, accesibilidad WCAG 2.1 AA y respuesta <100 ms. |
| Reglas | BR-001, BR-002, BR-003, BR-012, BR-014, BR-016, BR-019, BR-F003-001..BR-F003-008 | Un único documento por instancia; dos paneles sobre el mismo modelo en tiempo real; izquierdo WYSIWYG puro y derecho Markdown estándar; 100 % local/offline; resaltado pasajero sin mutación; barra sobre el panel activo; visibilidad persistente; parser CommonMark+GFM reversible; al menos un panel visible. |
| Pantallas | SC-001, VR-007, VR-008, VR-009 | Ventana principal con ambos paneles, separador redimensionable, indicador de panel activo, resaltado pasajero temporal, conmutadores de visibilidad y barra de formato opcional en los 6 estados visuales declarados. |
| Datos | ENT-015..ENT-019, OP-020..OP-024 | Vistas de panel izq./der., árbol sintáctico compartido, mapa de correspondencia, resaltado pasajero, preferencias de visibilidad y ancho del separador persistidos; renderizado WYSIWYG y Markdown; sincronización, correspondencia, resaltado y desplazamiento. |
| Seguridad | security-permissions.md, NFR-004, NFR-005, NFR-006, NFR-010 | Sin autenticación ni elevación; sin red, telemetría ni escritura fuera del directorio de configuración; operaciones de edición/sincronización solo locales; registros locales por salida estándar. |
| Comunicación interna | COM-016..COM-021 | Sincronización bidireccional, correspondencia de posiciones, resaltado pasajero, desplazamiento sincronizado, conmutación de visibilidad de paneles y barra de formato. |
| Externos | external-services.md, NFR-004 | No aplica: 0 servicios externos; se comprueba la ausencia de red y de dependencias remotas en arranque, UI, sincronización y persistencia de preferencias. |
| Errores | ERR-016..ERR-019, LOG-009..LOG-011 | Fallo de sincronización con reintento y aviso no modal sin pérdida; degradación en documentos grandes sin bloquear; posición sin ancla; error de renderizado WYSIWYG con marcador y Markdown intacto; registros técnicos declarados. |
| Estados y transiciones | ST-018..ST-022, TR-034..TR-039 | Visibilidad de panel izquierdo/derecho (ambos, solo izq., solo der.) con restricción de al menos uno; visibilidad de barra de formato; panel activo izquierdo/derecho; sincronización activa/degradada; resaltado pasajero visible/oculto. |
| Compatibilidad y licencias | NFR-007, NFR-008, NFR-011 | Build net10.0 y ejecución en Linux, plataforma de la corrida; cobertura unitaria de la lógica de sincronización ≥70 %; dependencias MIT-compatibles. |
| AC y tareas del Plan | AC-024..AC-029, DS-003, T-040..T-058 | Cumplimiento de AC-024..AC-029 y de DS-003; verificación de cada tarea del Plan en la tabla siguiente. |

**Tareas del Plan y evidencia**

| Tarea | Comprobación |
|---|---|
| T-040 | Parser CommonMark+GFM y árbol sintáctico regenerable, conversión reversible (`T040ParserCommonMarkTests`, ENT-016, BR-F003-008, C-009). |
| T-041 | Dos paneles izq./der. sobre el mismo modelo (`T041EditorPanelsTests`, ENT-015, BR-F003-001/002/003). |
| T-042 | Renderizado WYSIWYG y Markdown con marcador de error (`T042RenderTests`, OP-020/OP-021, ERR-019). |
| T-043 | Sincronización bidireccional por eventos sin sondeo (`T043SyncSmokeTests`, OP-022, COM-016/017, BR-F003-004). |
| T-044 | Motor de correspondencia de posiciones (`T044PositionMapTests`, ENT-017, OP-023, COM-018, BR-F003-005). |
| T-045 | Resaltado pasajero con expiración, sin mutación (`T045PassengerHighlightTests`, ENT-018, OP-024, COM-019, BR-F003-006, BR-014, VR-007). |
| T-046 | Desplazamiento sincronizado entre paneles (`T046ScrollSyncTests`, FR-009, COM-018). |
| T-047 | Layout de doble panel con separador redimensionable (`MainWindowTests`/`T053ManualVerificationTests`, VR-008, NFR-009). |
| T-048 | Conmutadores de visibilidad de paneles con restricción de al menos uno (`T049FormatBarVisibilityTests`/`T053ManualVerificationTests`, ST-018–ST-020, TR-034–TR-037, COM-020, BR-F003-007). |
| T-049 | Conmutador de visibilidad de barra de formato (`T049FormatBarVisibilityTests`, ST-021/ST-022, TR-038/TR-039, COM-021, BR-F003-007, BR-016). |
| T-050 | Persistencia de preferencias de visibilidad y ancho (`T050SmokeValidationTests`, ENT-019, BR-019, FR-013). |
| T-051 | Barra de formato sobre el panel activo (`T049FormatBarVisibilityTests`/`FormatBarTests`, COM-021, BR-016, VR-009). |
| T-052 | Indicador de panel activo (`T052ActivePanelIndicatorTests`, VR-009, NFR-009). |
| T-053 | Degradación de rendimiento y reintento de sincronía (`T053ManualVerificationTests`, ERR-016/017, LOG-009, NFR-001/NFR-003). |
| T-054 | Manejo de posición sin ancla (`T044PositionMapTests`/`T053ManualVerificationTests`, ERR-018, LOG-011). |
| T-055 | Registro técnico LOG-009/LOG-010/LOG-011 (`T053ManualVerificationTests`/`T058SyncCoverageAndAcTests`, LOG-009..LOG-011, ERR-016..ERR-019). |
| T-056 | Accesibilidad de la doble vista (`T052ActivePanelIndicatorTests`/`T058SyncCoverageAndAcTests`, NFR-009, AC-029, VR-007–VR-009). |
| T-057 | Carga de preferencias al iniciar la sesión (`T050SmokeValidationTests`/`T058SyncCoverageAndAcTests`, ENT-019, BR-019). |
| T-058 | Pruebas unitarias de sincronización ≥70 % y verificación AC-024..AC-029/DS-003 (`T058SyncCoverageAndAcTests`, NFR-007, AC-024..AC-029, DS-003). |

## Actividades y pruebas

1. Ejecutar la suite automatizada completa con el comando anclado y exigir `exit_code=0`, sin fallos ni pruebas omitidas.
2. Comprobar el arranque: SC-001 directa y única, ambos paneles visibles, barra de formato visible, tema claro, documento vacío, indicador de panel activo y separador redimensionable con foco visible.
3. Editar texto y formato en cualquiera de los paneles y verificar el reflejo automático en el panel opuesto sin botón ni sondeo, regeneración del árbol sintáctico y reversibilidad de la conversión (AC-024, BR-F003-004/008, COM-017).
4. Cambiar el foco de un panel al otro y verificar el desplazamiento a la sección equivalente, el resaltado pasajero que desaparece solo y la ausencia de mutación de contenido; si no hay ancla, no desplazar ni resaltar y registrar (AC-025, BR-F003-005/006, BR-014, COM-018/COM-019, ERR-018).
5. Ocultar y mostrar cada panel desde el menú Ver o su atajo y verificar conservación de contenido y estado, restricción de al menos un panel visible, re-renderizado desde el modelo y persistencia de la preferencia (AC-026, BR-F003-007, COM-020, BR-019).
6. Ocultar y mostrar la barra de formato y verificar que no bloquea la edición por menú y atajos, que actúa sobre el panel activo y que la preferencia persiste entre sesiones (AC-027, BR-F003-007, BR-016, COM-021, ST-021/ST-022, VR-009).
7. Medir el tiempo de reflejo en documentos de hasta 50 000 palabras (<200 ms) y hasta 100 000 palabras/10 000 líneas (sin degradación perceptible >1 s); inducir superación del umbral y verificar degradación sin bloqueo y registro (AC-028, NFR-001, NFR-003, ERR-017, LOG-009).
8. Navegar solo con teclado y lector de pantalla: paneles y separador operables con foco visible, panel activo comunicado de forma no solo cromática, resaltado no interferente ni anunciado como alerta y respeto de `prefers-reduced-motion` (AC-029, NFR-009, VR-007–VR-009).
9. Inducir fallos de sincronización y renderizado (fallo de sincronía, posición sin ancla, error LaTeX/Mermaid) y comprobar reintento, aviso no modal, conservación del documento, marcador en el panel izquierdo y Markdown intacto, con registros LOG-009/LOG-010/LOG-011 (ERR-016–ERR-019).
10. Revisar accesibilidad (nombres accesibles, foco, contraste AA, teclado) sobre SC-001 en los 6 estados visuales y el indicador de panel activo no dependiente solo del color.
11. Obtener las capturas visuales de SC-001 en estados A, B, C, D, E y F mediante el comando anclado en `design-system.md`.

Comando de prueba: dotnet test project/implementation/tests/EggMdEditor.App.Tests/EggMdEditor.App.Tests.csproj

## Condiciones

- Spec v1 y Plan v1 vigentes; la corrida se registra sobre el commit de la Execution E-007.
- Tareas T-040..T-058 en `done`; build local con .NET SDK 10.0.x, sin red ni servicios externos (BR-012, NFR-004).
- SC-001 con mockup aprobado en `plan/mockups.md`; captura headless vía el comando anclado en `design-system.md`, que no produce decoraciones del SO (marco y botones nativos), ausentes del mockup.
- NFR-008 se contrasta con la matriz de soporte de .NET 10 (C-011); la corrida se ejecuta en Linux, la plataforma del entorno declarado.
- Cobertura unitaria de la lógica de sincronización ≥70 % (NFR-007) contrastada con `evidence/coverage-run.json` si se ejecuta con cobertura.

## Evidencias a obtener

- `executions/E-007/evidence/test-run.json`, producida por `sdd-test-run`, con `exit_code=0` y `sha256` íntegro.
- `executions/E-007/evidence/visual-capture/` con la captura y el manifiesto de SC-001 en estado A (`SC-001.json`, `SC-001.png`), estado B (`SC-001-B.json`, `SC-001-B.png`), estado C (`SC-001-C.json`, `SC-001-C.png`), estado D (`SC-001-D.json`, `SC-001-D.png`), estado E (`SC-001-E.json`, `SC-001-E.png`) y estado F (`SC-001-F.json`, `SC-001-F.png`), producidos por `sdd-visual-capture` con `sha256` íntegro.
- Observaciones de las comprobaciones funcionales, de error y de seguridad consignadas por el reviewer en `results/v1/result.md`.

## Checks visuales

Checks visuales: SC-001
Estados visuales: SC-001: A, B, C, D, E, F
SC-001 estado A: base — ambos paneles y barra visibles, tema claro.
SC-001 estado B: solo panel izquierdo.
SC-001 estado C: solo panel derecho.
SC-001 estado D: barra oculta.
SC-001 estado E: tema oscuro.
SC-001 estado F: resaltado pasajero visible.

Renuncia visual: SC-001/A — entorno de verificación sin display para captura headless.
Renuncia visual: SC-001/B — entorno de verificación sin display para captura headless.
Renuncia visual: SC-001/C — entorno de verificación sin display para captura headless.
Renuncia visual: SC-001/D — entorno de verificación sin display para captura headless.
Renuncia visual: SC-001/E — entorno de verificación sin display para captura headless.
Renuncia visual: SC-001/F — entorno de verificación sin display para captura headless.

## Criterio

Binario — `Plan cubierto = T-040..T-058 verificadas + Spec v1 cumplida + AC-024..AC-029 cumplidos + DS-003 alcanzado + 0 incidencias pendientes`, con `test-run.json` en `exit_code=0` y las capturas de SC-001:A, SC-001:B, SC-001:C, SC-001:D, SC-001:E y SC-001:F con `sha256` íntegro. Cualquier tarea, AC o DS sin evidencia, cualquier estado visual sin captura o cualquier incidencia abierta deja el resultado en `Plan no cubierto`.
