# Result v1 — F-004 Formato Markdown completo

Spec verificada: v1
Plan: v1
Execution: E-010
Implementation commit: 48af8af1bab6d9f893638e87778ccdf9642d321b
Cobertura: no medido (sin `evidence/coverage-run.json`; NFR-007 no contrastada)

## Hallazgos

- `HV-015` — Evidencia de pruebas no registrada en el checkpoint (`test_evidence`: `hash_not_recorded`): `executions/E-010/evidence/test-run.json` tiene `exit_code=0` (479 correctas, 0 omitidas) pero su `sha256` no está registrado vía `sdd-state-set`. `sdd-gate-check PLAN_COVERED` falla.
- `HV-016` — Evidencia visual ausente (`visual_evidence`: `evidence_absent`): el Run declara `Checks visuales: SC-001` y una renuncia visual para `SC-001/A`, pero no existe manifiesto agregado producido por `sdd-visual-capture` ni renuncia registrada en el checkpoint.
- `HV-017` — Cobertura de requisitos del Run incompleta (`run_requisitos_cobertura`: falta `BR-004`): `feature.md` y `spec/business-rules.md` citan `BR-004` (exportación PDF reproduce el panel izquierdo), pero la matriz del Run no lo verifica.
- `HV-018` — Umbral de rendimiento de `AC044_Performance_LargeDoc` (≤ 5 s) es más laxo que `AC-044`/`DS-004` (≤ 2 s); la prueba no verifica el límite normativo.
- `HV-019` — `AC-039` y `AC-040` solo comprueban `html.Length > 0`, sin verificar renderizado KaTeX/Mermaid ni marcador de error controlado (`ERR-019`).
- `HV-020` — Prueba `T090_ReflectionTouch_FormatTypes` invoca métodos por reflexión con argumentos por defecto y descarta excepciones, inflando cobertura sin verificar comportamiento (NFR-007).
- `HV-021` — Cobertura unitaria no evidenciada: no existe `coverage-run.json` para contrastar NFR-007 (≥ 70 %).
- `HV-022` — Accesibilidad implementada mediante propiedades constantes (`WCAGCompliant => true`, `AACompliant => true`) y clases alias generadas; `AC-045` no valida UI real (NFR-009).

## Correcciones requeridas

1. Registrar `evidence_sha256` del `test-run.json` en el checkpoint (`sdd-state-set`) y re-ejecutar `sdd-gate-check PLAN_COVERED`.
2. Ejecutar `sdd-visual-capture` para `SC-001` o registrar formalmente la renuncia visual en el checkpoint.
3. Añadir la comprobación de `BR-004` a la matriz del Run (o justificar su exclusión y ajustar `feature.md`).
4. Endurecer `AC044_Performance_LargeDoc` a ≤ 2 s y medir sin bloqueo del hilo UI.
5. Reforzar las aserciones de `AC-039`/`AC-040` (salida KaTeX/diagrama o marcador de error).
6. Eliminar o sustituir la prueba de cobertura por reflexión por pruebas unitarias con aserciones.
7. Generar `coverage-run.json` y contrastar NFR-007 (≥ 70 %).
8. Sustituir los stubs de accesibilidad por validación real de controles y diálogos.

## Evidencias

- `executions/E-010/evidence/test-run.json` — `exit_code=0`, 479 correctas, 0 omitidas, duración 30 898 ms.
- `verification/runs/v1/verification-run.md` — Run v1 (Checks visuales: `SC-001`, renuncia visual declarada).
- `verification/executions/E-010/execution.md` — commit `48af8af1bab6d9f893638e87778ccdf9642d321b`.
- `sdd-gate-check PLAN_COVERED` — `fail` (`HV`-relacionados arriba).

## Resultado

Plan no cubierto
