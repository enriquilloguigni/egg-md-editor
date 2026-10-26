Spec verificada: v2
Plan: v2
Run: v2
Execution: E-003
Implementation commit: 60f8af180e44b97eeb550e522184879c43a40f2f

## Cobertura

| Medida | Resultado |
|---|---|
| Tareas T-001..T-019 | 18/19 con evidencia directa (95 %); T-019 queda parcial: se eliminaron los ficheros de alias, pero permanecen interfaces, API y nombres de comando sin uso |
| AC-001..AC-009 | 9/9 con evidencia directa (tests y/o capturas) |
| DS-001 | Alcanzado en las métricas cubiertas: 6 rótulos, 12 controles en orden, 3 opciones de tema, 2 botones de navegación, 0 claves i18n sin resolver y 0 procesos residuales; no cubre el tamaño mínimo de SC-001 |
| Suite automatizada | 142/142 correctas, 0 omitidas, `exit_code=0`; re-ejecución independiente: 142/142 |
| Fórmula del Plan | No alcanzada: persisten incidencias abiertas (HV-001, HV-002 y HV-009 parciales; HV-011 nuevo) |

## Hallazgos

| ID | Dimensión | Hallazgo | Estado / severidad |
|---|---|---|---|
| HV-001 | Evidencia / Run | Parcial. El formato `Estados visuales` ya se interpreta y las cuatro capturas tienen `sha256` íntegro; persiste que el `command` de los manifiestos termina en `.txt` mientras `capture_file` apunta a `.png` (SC-001, SC-001-B, SC-002 y SC-003), por lo que la instrucción registrada no reproduce el artefacto. | Abierto / Media |
| HV-002 | Pantallas / Visual | Parcial. SC-001 ya no tiene columnas y muestra los 12 controles; la captura A/B no muestra el aviso flotante que el mockup v2 incluye (el control «Código» aparece resaltado pero sin burbuja) y el Run no declara renuncia visual para el aviso. | Abierto / Media |
| HV-003 | Reglas / Datos / DS-001 | Cerrado. El inventario y orden de los 12 controles queda fijado en Spec v2, Plan v2, `MainWindowViewModel.FormatBarLayout` y `FormatBarTests`. | Cerrado |
| HV-004 | Funcional / Estados | Cerrado. Herramientas → Tema expone Claro/Oscuro/Sistema, mutuamente excluyentes, enlazado a OP-002; `ThemeSwitchTests` y SC-001-B evidencian el estado oscuro. | Cerrado |
| HV-005 | AC-005 / T-018 | Cerrado. `ProcessLifecycleEndToEndTests.ASecondInstanceFocusesTheRunningWindowWithoutCreatingAnother` verifica foco, restauración y ausencia de segunda ventana a nivel de proceso. | Cerrado |
| HV-006 | AC-006 / AC-007 / T-018 | Cerrado. El E2E verifica código de error, stderr en español, ausencia de ventana usable, ausencia de residuales y no persistencia de idioma y tema. | Cerrado |
| HV-007 | VR-001 / NFR-011 | Cerrado. Los 12 controles usan `Material.Icons` MIT y SC-003 muestra la atribución con nombre, versión y licencia; `HelpSurfaceTests` contrasta la versión con el paquete cargado. | Cerrado |
| HV-008 | Trazabilidad / Artefactos | Cerrado. `feature.md` ya no declara `PLAN_REVIEW pendiente`, el Run v2 cita E-003 y `sdd-gate-check SPEC_REVIEW` devuelve `pass`. | Cerrado |
| HV-009 | Arquitectura / Calidad | Parcial. Se eliminaron `Compatibility.cs`, `SessionAliases.cs` y `CompatibilityNamespaces.cs`, pero permanecen `ISession` y `IUiPreferences` sin uso, `Session.Instance`/`Clone` y `UiPreferences.Clone` ejercitados solo por tests, `ClearShortcuts` sin uso, y alias de comando y `DispatchCommand`/`Execute`/`CommandResult`/`DispatchResult` usados solo por tests. T-019 no queda verificado. | Abierto / Baja |
| HV-010 | Funcional / Pantallas | Cerrado. Spec v2, AC-003 y BR-F001-002 fijan «opciones» como los 6 rótulos; Archivo, Edición, Ver y Formato se abren/cierran sin entradas internas. | Cerrado |
| HV-011 | Pantallas / NFR-009 | Nuevo. SC-001 declara tamaño mínimo 1280×720 (`spec/screens.md`, `Crítica` ligada a NFR-009), pero `MainWindow.axaml` solo fija `Width`/`Height` y no `MinWidth`/`MinHeight`; con `CanResize` por defecto la ventana puede reducirse por debajo del mínimo. Ninguna tarea, AC ni prueba del Run lo cubre. | Abierto / Media |

## Correcciones requeridas

1. Corregir HV-001: alinear el `command` de los manifiestos de captura con el `.png` de `capture_file` (o corregir la tool/registrar la renuncia correspondiente) y volver a producir la evidencia visual con la instrucción reproducible.
2. Corregir HV-002: capturar el estado con aviso flotante visible o declarar una renuncia visual explícita en el Run v3; la captura debe ser coherente con el mockup v2, que muestra el aviso «Código».
3. Corregir HV-009: eliminar la API y las interfaces sin uso (`ISession`, `IUiPreferences`, `Session.Instance`, `Clone`, `ClearShortcuts`, `DispatchCommand`/`Execute`, `CommandResult`/`DispatchResult` y alias de comando) o justificarlas con una decisión de arquitectura; actualizar T-019 y sus pruebas.
4. Corregir HV-011: fijar `MinWidth/MinHeight` en 1280×720 (o `CanResize=false` si el tamaño es fijo), añadir una prueba que impida reducirlo y verificar el mínimo en el Run.
5. Tras las correcciones, emitir una nueva Execution y un nuevo Result; `sdd-gate-check PLAN_COVERED` debe devolver `pass` con 0 incidencias.

## Evidencias

- `executions/E-003/evidence/test-run.json`: `exit_code=0`, `passed=true`, 142 correctas, 0 omitidas; `sha256` recomputado `4c61e637165971965f1e9f6e777234e300abbfb83c3462b4f73ae0c7e74c6a9f`, coincidente. Re-ejecución independiente: 142/142.
- `executions/E-003/evidence/visual-capture/`: SC-001 `50b308ae…`, SC-001-B `709fe76e…`, SC-002 `6de368c4…`, SC-003 `fcefb88e…`; hashes PNG recomputados y coincidentes con sus manifiestos; agregado declarado `c8cec06fee99c288780fdecd946dc74818630b6af76f2b3af77b1b5f5db26477`.
- SC-001: 6 rótulos como botones, navegación Anterior/Siguiente, 12 controles en orden, área de documento vacía y sin columnas; SC-001-B con tema oscuro; SC-003 con atribución de `Material.Icons` 3.0.2 y `Material.Icons.Avalonia` 3.0.2, licencia MIT.
- Código en 60f8af18: `program/implementation` sin cambios locales; no existen los ficheros de alias citados en HV-009. `MainWindow.axaml` no declara `MinWidth`/`MinHeight` en la ventana; `CommandTypes.cs` y `CommandDispatcher.cs` conservan los alias y la API duplicada indicados en HV-009.
- `sdd-gate-check SPEC_REVIEW`: `pass`. `sdd-gate-check PLAN_REVIEW`: `pass`. `sdd-gate-check PLAN_COVERED`: `fail` por `plan_cubierto(outcome=?)` (el checkpoint aún no registra el Result v2); la tupla Spec v2 / Plan v2 / E-003 / 60f8af18 no está stale y el dictamen es `Plan no cubierto`.

## Resultado

**Resultado: Plan no cubierto**

Las 19 tareas están marcadas `done` y la suite pasa 142/142, pero la fórmula del Plan no se cumple: HV-001, HV-002 y HV-009 no quedan completamente cerrados, HV-011 añade una violación de SC-001 no verificada y no hay 0 incidencias pendientes. Se requiere corregir los manifiestos visuales, evidenciar o renunciar al aviso flotante, retirar la API especulativa restante y fijar el tamaño mínimo de SC-001 antes de emitir un Result cubierto.
