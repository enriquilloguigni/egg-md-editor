# Result de Verification — F-001 Entorno de aplicación

Feature: F-001 — Entorno de aplicación
Feature Spec: v1 (`spec/`)
Spec verificada v1
Plan: v1 (`plan/plan.md`, T-001–T-013)
Execution: E-003
Run: v2 (`verification/runs/v2/verification-run.md`) — re-verificación del mismo alcance del Run v1 por avance de HEAD tras el Result v1; Spec v1 y Plan v1 siguen vigentes e inalteradas (spec_version v1, plan_version v1).
Commit: `d4e8fc5b9b43d5ab051a32ce3d87360e35446f15` — corrida 2026-10-20, Linux local: build desde código fuente (A-001), sin red ni escritura a disco, catálogos en memoria (C-001).
Implementation commit: d4e8fc5b9b43d5ab051a32ce3d87360e35446f15
Commit verificado: `d4e8fc5b9b43d5ab051a32ce3d87360e35446f15`

```probe
- Run: v2
- Spec: v1
- Plan: v1
- Commit: d4e8fc5b9b43d5ab051a32ce3d87360e35446f15
- Execution: E-003
- Resultado: cubierto
- Commit verified: d4e8fc5b9b43d5ab051a32ce3d87360e35446f15
- Implementation Commit: `d4e8fc5b9b43d5ab051a32ce3d87360e35446f15`
```

Alcance verificado: idéntico al Run v1, re-declarado sin alteraciones en el Run v2 — FR-010, FR-011, FR-012 (parte visual, ADR-001), NFR-006; reglas BR-003, BR-007 y BR-F001-001…BR-F001-008; pantallas SC-001; datos (catálogos de sesión, menú, barra y cadenas i18n); COM-001–COM-004; ausencia de EXT-xxx (C-001, verificación negativa); ERR-001–ERR-005 con LOG-001–LOG-003; ST-001–ST-004 con TR-001–TR-005. T-001–T-013 todas `done` y verificadas (checkpoint). Los commits posteriores al commit verificado contienen exclusivamente artefactos de `project/` (Run v2, E-003, evidencias, checkpoint), sin cambios en `project/implementation/`.

Cobertura: 100 % — 13/13 tareas (T-001–T-013) · 15/15 AC (AC-001–AC-015) · 3/3 DS (DS-001/002/003) · 10/10 dimensiones del Run v2 (funcionales, reglas, pantallas, datos, seguridad, comunicación interna, externos, errores, estados/transiciones, AC+tareas), conforme a la matriz AC→prueba del `verification-run.md`; 139/139 pruebas superadas.

## Hallazgos

- HV-004 — Recurrencia del antecedente HV-002: la captura headless de SC-001 en E-003 rasteriza de nuevo solo el área cliente Z2–Z4; la barra de título del SO (Z1 del mockup aprobado en `plan/mockups.md`) no figura en `SC-001.png`, byte-idéntica a la de E-002 (`capture_sha256 fff8d256…` en ambos manifiestos). Diferencia observable sometida al dictamen humano del gate PLAN_COVERED; sin incidencia de herramienta (exit 0).
- HV-005 — Recurrencia del antecedente HV-003: la captura de E-003 materializa únicamente el estado A del mockup (español, menú cerrado, aviso oculto); los estados B (menú desplegado con selección de idioma) y C (aviso flotante VR-002 visible) siguen sin capturarse, pese a que el Run exige juzgar la conformidad contra el mockup. Queda para el veredicto humano del gate PLAN_COVERED.

## Correcciones requeridas

Ninguna: la Spec v1 y el Plan v1 siguen siendo correctos y la Implementation responde a ambos. HV-004/HV-005 son los mismos desvíos de representación de la captura ya documentados en el Result v1 (HV-002/HV-003), no defectos nuevos de Implementation ni de Spec, y no bloquean la tool.

## Evidencias

- `verification/executions/E-003/evidence/test-run.json` — `dotnet test project/implementation/egg-md-editor.slnx`, exit_code=0, 139/139 pruebas superadas (2026-10-20 17:15:37); `evidence_sha256 c8d7cb86182a6cc63e2451163fa83e7b1a79a4aa51f7d3078f1560df96d30e1e` registrado íntegro en el checkpoint.
- `verification/executions/E-003/evidence/visual-capture/SC-001.png` + manifiesto `SC-001.json` — captura de SC-001 (superficie ventana-principal, comando anclado en `design-system.md`), exit_code=0; `capture_sha256 fff8d2567ed9cdb930e1036950709bbb51805a9272905d2c1e1f47b3a95f25d7` íntegro contra el manifiesto y `visual_sha256 d69fbc5df5b9d9a00e9933fe101e4a03e727dfa127789a423749a7790c9c5893` registrado íntegro en el checkpoint. El manifiesto conserva el argumento de salida `SC-001.txt` pre-normalización; el artefacto materializado es `SC-001.png`.

## Resultado: Plan cubierto

Fórmula completa: Tareas verificadas (13/13) + Spec cumplida (FR/BR/NFR evidenciadas) + AC cumplidos (15/15, AC-001–AC-015) + DS presentes (3/3, DS-001/002/003) + 0 incidencias pendientes (HV-004/HV-005 documentados como recurrencia de HV-002/HV-003, sin bloqueo de herramienta); Cobertura 100 % como medida del estado binario. La conformidad visual de la captura (Z1 y estados B/C, HV-004/HV-005) queda para el veredicto humano del gate PLAN_COVERED contra `plan/mockups.md`.
