Feature Spec: v2
Spec verificada: v2
Plan: v2
Run: v2
Execution: E-004
Implementation commit: 0ac46891454615f1fde2f533df0e230c3f4e993a

## Cobertura

| Medida | Resultado |
|---|---|
| Tareas T-001..T-019 | 19/19 con evidencia directa (100 %); T-019 queda verificado tras retirar la API especulativa enumerada en HV-009 |
| AC-001..AC-009 | 9/9 con evidencia directa (100 %) |
| DS-001 | Alcanzado en sus métricas: 6 rótulos, 12 controles en orden, 3 opciones de tema, 2 botones de navegación, conmutación en caliente sin pérdida de estado, 0 claves i18n sin resolver y 0 procesos residuales |
| Suite automatizada | 141/141 correctas, 0 omitidas, `exit_code=0`; re-ejecución independiente: 141/141 |
| Fórmula del Plan | Alcanzada: 19 tareas verificadas + Spec v2 cumplida + 9 AC cumplidos + DS-001 + 0 incidencias pendientes |

## Hallazgos

| ID | Dimensión | Hallazgo | Estado / severidad |
|---|---|---|---|
| HV-001 | Evidencia / Run | Cerrado con renuncia explícita: el `command` de los manifiestos registra la ruta temporal `.txt` que `sdd-visual-capture` renombra por magic bytes a `.png`; es comportamiento determinista de la tool, no modificable desde el proyecto. El `capture_file` y el `capture_sha256` apuntan al PNG real y el agregado cuadra. | Cerrado / Baja |
| HV-002 | Pantallas / Visual | Cerrado. `SC-001-C` captura el aviso flotante visible («Énfasis») sobre el área de documento; el Run declara el estado C y la captura tiene `sha256` íntegro. | Cerrado |
| HV-003 | Reglas / Datos / DS-001 | Cerrado. El inventario y orden de los 12 controles queda fijado en Spec v2, Plan v2, `MainWindowViewModel.FormatBarLayout` y `FormatBarTests`. | Cerrado |
| HV-004 | Funcional / Estados | Cerrado. Herramientas → Tema expone Claro/Oscuro/Sistema, mutuamente excluyentes, enlazado a OP-002; `ThemeSwitchTests` y `SC-001-B` evidencian el estado oscuro. | Cerrado |
| HV-005 | AC-005 / T-018 | Cerrado. `ProcessLifecycleEndToEndTests.ASecondInstanceFocusesTheRunningWindowWithoutCreatingAnother` verifica foco, restauración y ausencia de segunda ventana a nivel de proceso. | Cerrado |
| HV-006 | AC-006 / AC-007 / T-018 | Cerrado. El E2E verifica código de error, stderr en español, ausencia de ventana usable, ausencia de residuales y no persistencia de idioma y tema. | Cerrado |
| HV-007 | VR-001 / NFR-011 | Cerrado. Los 12 controles usan `Material.Icons` MIT y `SC-003` muestra la atribución con nombre, versión y licencia; `HelpSurfaceTests` contrasta la versión con el paquete cargado. | Cerrado |
| HV-008 | Trazabilidad / Artefactos | Cerrado. `feature.md` no declara pendientes de revisión, el Run v2 cita E-004 y los gates de Spec y Plan devuelven `pass`. | Cerrado |
| HV-009 | Arquitectura / Calidad | Cerrado. No quedan `ISession`, `IUiPreferences`, `Session.Instance`, `Clone`, `ClearShortcuts`, `DispatchCommand`, `CommandResult`, `DispatchResult` ni alias de comando; `Execute` solo permanece como `ICommand.Execute` de `RelayCommand`. `SessionPreferencesTests.Sessions_AreIndependent` y `CommandDispatcherTests` cubren el comportamiento sin alias. | Cerrado |
| HV-010 | Funcional / Pantallas | Cerrado. Spec v2, AC-003 y BR-F001-002 fijan «opciones» como los 6 rótulos; Archivo, Edición, Ver y Formato se abren/cierran sin entradas internas. | Cerrado |
| HV-011 | Pantallas / NFR-009 | Cerrado. `MainWindow.axaml` fija `MinWidth="1280"` y `MinHeight="720"`; `MainWindowTests.Window_DeclaresAndEnforcesTheSc001MinimumSize` verifica que una solicitud de 800×600 no reduce la ventana por debajo del mínimo. | Cerrado |

## Correcciones requeridas

Ninguna. Las correcciones 1–4 del Result v2 están aplicadas y la corrección 5 se satisface con la Execution E-004 y este Result.

## Evidencias

- `executions/E-004/evidence/test-run.json`: `exit_code=0`, `passed=true`, 141 correctas, 0 omitidas; `sha256` recomputado `dc1a6204bb1992ae24c395ef59988194f5571814caedb1ffb6b279cf25f4ec76`, coincidente con el checkpoint. Re-ejecución independiente: 141/141, 0 omitidas.
- `executions/E-004/evidence/visual-capture/`: SC-001 `50b308ae1d73e89d1c4164767704b9d6935663fd728a0d4e9c3e1581bd55642d`, SC-001-B `709fe76e0463a19bc404338dca28e82cc1cab7156e22c2bec8202a18fe32cdef`, SC-001-C `5e7a0aee48e70c7760fb605fea10fd38275315f073ad89f94895bf8eb9226b2b`, SC-002 `6de368c4fa69210b1164578a1d43713bef85e9d6c1b80b2b87adadbeb5bd2b5a`, SC-003 `fcefb88efa6dda7d07566486fd019b253d4f9b2e80a73852442b8f2ef00ff687`; hashes PNG recomputados y coincidentes con sus manifiestos; agregado declarado `3b341315b864f5e3a661d5ab842aeabe9e419e914bbf9daf9d505907ed29e8b4`, coincidente.
- `SC-001`: 6 rótulos como botones, navegación Anterior/Siguiente, 12 controles en orden, área de documento vacía, 1280×720; `SC-001-B` con tema oscuro; `SC-001-C` con aviso flotante visible; `SC-002` con el diálogo modal; `SC-003` con la atribución de `Material.Icons` 3.0.2 y `Material.Icons.Avalonia` 3.0.2, licencia MIT.
- Código en 0ac4689: `MainWindow.axaml` declara `MinWidth`/`MinHeight` en 1280×720 y `MainWindowTests` los verifica; no existen los ficheros de alias ni las interfaces y API enumeradas en HV-009; `CommandDispatcherTests` y `SessionPreferencesTests` conservan pruebas de comportamiento.
- `sdd-gate-check SPEC_REVIEW`: `pass`. `sdd-gate-check PLAN_REVIEW`: `pass`. `sdd-gate-check UI_REVIEW`: `pass`. `sdd-gate-check PLAN_COVERED`: `fail` únicamente por `plan_cubierto(outcome=?)`, pendiente de registrar este Result en el checkpoint; el resto de checks pasa y la tupla Spec v2 / Plan v2 / Run v2 / E-004 / 0ac4689 no está stale.
- Limitación de HV-001: el `command` registrado termina en `.txt` porque la tool escribe ahí antes de renombrar por magic bytes; la renuncia a la reproducibilidad literal se registra en HV-001 y no afecta a la integridad de la captura.

## Resultado

**Resultado: cubierto**

Las 19 tareas y los 9 AC tienen evidencia directa, DS-001 se alcanza, la suite pasa 141/141 con `exit_code=0` y no quedan incidencias pendientes: HV-001 se cierra con renuncia explícita a la reproducibilidad literal del `command` por limitación determinista de la tool de captura, HV-002/HV-009/HV-011 quedan corregidos y el resto de hallazgos permanece cerrado. Plan cubierto.
