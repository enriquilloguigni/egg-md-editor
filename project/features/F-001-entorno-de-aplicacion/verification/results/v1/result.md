Spec verificada: v1
Plan: v1
Run: v1
Execution: E-002
Implementation commit: b1f4e944fcf8966860f4fb122bcf79487e84d537

## Cobertura

| Medida | Resultado |
|---|---|
| Tareas T-001..T-014 | 14/14 marcadas `done`; 11/14 con evidencia directa (79 %); T-009, T-012 y T-013 quedan parciales |
| AC-001..AC-007 | 3/7 con evidencia directa: AC-002, AC-003 y AC-004; AC-001, AC-005, AC-006 y AC-007 parciales |
| DS-001 | Parcial: la conmutación en caliente y las 0 claves i18n sin resolver están verificadas; el «100 % de entradas y controles» no está definido y los 0 procesos residuales no se miden de extremo a extremo |
| Suite automatizada | 119/119 correctas, 0 omitidas, `exit_code=0` |
| Fórmula del Plan | No alcanzada: permanecen incidencias abiertas (HV-001..HV-010) |

## Hallazgos

| ID | Dimensión | Hallazgo | Severidad |
|---|---|---|---|
| HV-001 | Evidencia / Run | La declaración `Estados visuales: SC-001: A, SC-002: A, SC-003: A` no es interpretable por `sdd-gate-check`: asigna a SC-001 los tokens `A, SC-002: A, SC-003: A` y busca capturas `SC-001-SC-002: A` y `SC-001-SC-003: A`. `PLAN_COVERED` devuelve `fail` por `estados_formato` y `visual_evidence`, aunque `SC-001.png/json`, `SC-002.png/json` y `SC-003.png/json` existen, tienen `exit_code=0` y sus hashes cuadran. Además, los manifiestos registran `SC-xxx.txt` en `command` mientras `capture_file` apunta a `SC-xxx.png`; la instrucción registrada no reproduce el artefacto. | Bloqueante |
| HV-002 | Pantallas / Visual | La captura SC-001 no coincide con el mockup aprobado: el mockup muestra barra de título, 5 controles en la barra y tres columnas (WYSIWYG, Markdown, Índice), con el aviso «Negrita»; la captura no tiene columnas, presenta 12 controles y ningún aviso. La Spec `BR-F001-008` declara que solo se muestran menú y barra y que el área de documento queda vacía, mientras `plan/mockups.md` y `design-system.md` muestran paneles. La captura sigue la Spec, no el mockup; la renuncia del Run a las decoraciones del SO no cubre las columnas ni el inventario de controles. | Alta |
| HV-003 | Reglas / Datos / DS-001 | FR-012 y DS-001 exigen el «100 % de los controles», pero ni la Spec ni el Plan fijan el inventario. El número 12 solo aparece en `MainWindowViewModel.FormatBarLayout` y en `FormatBarTests`; el mockup muestra 5. El 100 % no es verificable y los tests fijan una decisión de implementación no trazada a la Spec. | Alta |
| HV-004 | Funcional / Estados | OP-002, COM-003 y TR-008/TR-009 no tienen disparador visible en SC-001: no hay selector de tema en Herramientas, ni control en la barra, ni atajo; `ChangeThemeCommand` no está enlazado en la vista. La conmutación solo se ejerce desde el ViewModel en pruebas, aunque `feature.md`, `security-permissions.md` y `states-transitions.md` declaran la conmutación de tema. | Alta |
| HV-005 | AC-005 / T-012 / COM-005 | La segunda instancia se verifica a nivel de `SingleInstanceCoordinator` (evento de activación), no de la ventana: no hay prueba de que `App.RequestActivation` enfoque SC-001 ni de que no se cree una segunda ventana. AC-005 queda parcialmente verificado. | Media |
| HV-006 | AC-006 / AC-007 / T-013 / ERR | ERR-001, ERR-002 y ERR-005 se prueban a nivel de `ApplicationLifecycle` (mensaje, código y registro), pero no a nivel de proceso: no hay evidencia de `Program.Main`/`App` terminando con código de error, sin ventana usable y sin procesos residuales. AC-007 tampoco se prueba con un segundo arranque que confirme que el idioma no se persiste. | Media |
| HV-007 | VR-001 / Plan | La barra usa glifos de texto (`B`, `H▾`, `≡`, `“`, `</>`, `↗`, `▣`, `▦`, `†`, `Σ`, `◇`, `⋮`) en lugar de una biblioteca de iconos. El Plan preveía una librería MIT-compatible y VR-001 exige un conjunto de iconos; no hay librería ni atribución de iconos en Acerca de. | Media |
| HV-008 | Trazabilidad / Artefactos | `feature.md` indica «PLAN_REVIEW pendiente» con el gate ya aprobado; el Run v1 cita `executions/E-001` y «la corrida se registra sobre el commit de la Execution E-001», mientras la ejecución vigente es E-002 sobre b1f4e94. Además, `sdd-gate-check SPEC_REVIEW` falla hoy por presupuesto de prosa en `inapp-communication.md` (51 > 40), aunque el gate history lo declara aprobado. | Media |
| HV-009 | Arquitectura / Calidad | La superficie pública está duplicada de forma especulativa: `Compatibility.cs` (≈483 líneas), `SessionAliases.cs`, `CompatibilityNamespaces.cs` y un `CommandDispatcher` de ≈980 líneas con decenas de alias; hay tests que solo comprueban que los alias compilan (`Session_AliasesCompile`, `UiPreferences_AliasesCompile`). Contradice la arquitectura de vistas delgadas y añade deuda sin comportamiento de F-001. | Baja |
| HV-010 | Funcional / Pantallas | Los menús Archivo, Edición, Ver y Formato no tienen entradas internas: al pulsarlos solo cambia el estado (`OpenMenuKey`) y el foco, sin desplegar opciones. `feature.md` pide «la forma completa de menú … (sus opciones visibles)» y `process.md` habla del «100 % de sus opciones», mientras AC-001/AC-003 solo exigen los 6 menús. La ambigüedad no está resuelta y la captura no permite verificar opciones de menú. | Media |

## Correcciones requeridas

1. Corregir HV-001: declarar una línea `Estados visuales: SC-xxx: A` por pantalla en el Run (o corregir el parser de la tool), alinear la ruta de salida de los manifiestos de captura (`.png` en `command` y `capture_file`), volver a ejecutar `sdd-gate-check PLAN_COVERED` y registrar la evidencia. Si el Run cambia, emitir la versión que corresponda.
2. Resolver HV-002 y HV-003 con decisión humana: fijar el artefacto rector entre `BR-F001-008` y el mockup/`design-system`, definir el inventario y orden de FR-012 en Spec/Plan y recapturar SC-001 con los estados visuales coherentes.
3. Resolver HV-004: añadir un punto de entrada visible para OP-002 (menú, control o atajo) y capturar el tema oscuro, o retirar TR-008/TR-009 de F-001 y alinear Spec/Plan.
4. Añadir verificación de extremo a extremo para HV-005 y HV-006: foco de SC-001 al solicitar una segunda instancia, código de salida y stderr en fallo de arranque/cierre, ausencia de ventana usable, ausencia de procesos residuales y no persistencia del idioma.
5. Corregir HV-007: seleccionar una librería de iconos MIT-compatible y sustituir los glifos, o modificar VR-001/Plan con justificación; actualizar las atribuciones de SC-003.
6. Corregir HV-008: actualizar `feature.md` y las referencias del Run (E-002 y commit b1f4e94) y resolver el presupuesto de prosa de `inapp-communication.md` o registrar una renuncia explícita; repetir `SPEC_REVIEW`.
7. Corregir HV-009: eliminar alias, interfaces y API no usadas, y sustituir los tests de compilación de alias por pruebas de comportamiento.
8. Resolver HV-010: definir en Spec/Plan si F-001 incluye las entradas internas de los 6 menús; si sí, implementarlas y capturarlas; si no, aclarar que «opciones» son los 6 menús y ajustar la prosa de `feature.md`/`process.md`.
9. Tras las correcciones, generar una nueva Execution y un nuevo Result con la suite, las capturas y `sdd-gate-check` en verde; el Result actual no puede declararse cubierto.

## Evidencias

- `verification/executions/E-002/evidence/test-run.json`: `exit_code=0`, `passed=true`, 119 correctas, 0 omitidas; `sha256` recomputado `36c63ac4c750e444ba4bd1b6e985dac956a7b8a332615b5c4277cb2d48077fdb`, coincidente con el checkpoint. Re-ejecución independiente de la suite: 119/119 correctas.
- `verification/executions/E-002/evidence/visual-capture/`: SC-001, SC-002 y SC-003 con `exit_code=0`; hashes PNG recomputados y coincidentes con sus manifiestos (`689ccac3…`, `75722171…`, `91336c69…`); agregado del checkpoint `ddfabf7b3835fbf40575e6018fd12c2fc8df7c652c4bc7390bd215a8ddc35b04`.
- Código en b1f4e944: `project/implementation` sin diferencias respecto al commit; la corrección del runner de pruebas (63f54d0) es ancestro del commit verificado.
- Revisión de dimensiones: sin acceso a red, telemetría ni escritura a disco desde la aplicación; sin autenticación ni elevación; una instancia/ventana; catálogo i18n es/en con 50 claves y 0 sin resolver en el idioma activo; registro técnico por stdout; sin acciones de menú/barra de otras Features conectadas.
- `sdd-gate-check PLAN_COVERED`: `fail` por `estados_formato` y `visual_evidence` (HV-001). `sdd-gate-check SPEC_REVIEW`: `fail` por presupuesto de prosa (HV-008).

## Resultado

**Resultado: Plan no cubierto**

Las 14 tareas están marcadas `done` y la suite pasa 119/119, pero la fórmula del Plan no se cumple: `sdd-gate-check PLAN_COVERED` falla, quedan incidencias de evidencia visual, alcance y trazabilidad (HV-001..HV-010) y no hay 0 incidencias pendientes. Se requiere corregir el Run y los artefactos, resolver las decisiones de Spec/Plan y completar la verificación end-to-end antes de emitir un Result cubierto.
