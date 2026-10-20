# Result de Verification — F-001 Entorno de aplicación

Feature Spec: v1
Spec verificada v1
Plan: v1
Execution: E-002
Run: v1 (`verification/runs/v1/verification-run.md`) — corrida 2026-10-20 en Linux local (A-001, C-001, catálogos en memoria); sustituye a E-001, abortada por causa de tooling (HV-001), no por defecto de Spec ni de Plan.
Commit: `ec92dd3d2811e64f82da10c7d1bcec6226ef5560`
Implementation commit: ec92dd3d2811e64f82da10c7d1bcec6226ef5560
Commit verificado: ec92dd3d2811e64f82da10c7d1bcec6226ef5560
Commit verificado: `ec92dd3d2811e64f82da10c7d1bcec6226ef5560`
```probe
- Run: v1
- Spec: v1
- Plan: v1
- Commit: ec92dd3d2811e64f82da10c7d1bcec6226ef5560
- Execution: E-002
- Resultado: cubierto
- Commit verified: ec92dd3d2811e64f82da10c7d1bcec6226ef5560
- Implementation Commit: `ec92dd3d2811e64f82da10c7d1bcec6226ef5560`
```

Alcance verificado: FR-010, FR-011, FR-012 (parte visual, ADR-001), NFR-006; reglas BR-003, BR-007; criterios DS-001, DS-002, DS-003 — 13/13 tareas T-001–T-013 done y verificadas en esta corrida, conforme a la Secuencia del Plan (T-001/T-012/T-013 por `dotnet test` operativo en `project/implementation/`).

Cobertura: 100 % — 13/13 tareas · 15/15 AC (AC-001–AC-015) · 3/3 DS · 10/10 dimensiones mínimas del Run v1 (funcionales, reglas, pantallas, datos, seguridad, comunicación interna, externos, errores, estados/transiciones, AC+tareas), según la matriz AC→prueba del `verification-run.md`.

## Hallazgos

- HV-001 — Incidente de tooling resuelto (no de Spec/Plan): la Execution E-001 (2026-10-19) abortó con exit 127 por ancla de comando de captura inexistente; corregido en el ciclo con la utilidad `project/implementation/scripts/EggMdEditor.VisualCapture/` y anclas reales `dotnet run -- … VisualCapture.csproj -- <superficie> <SC> <salida>` en `project/design/design-system.md` (líneas 9–11), determinista y con exit 0 en E-002.
- HV-002 — La captura headless de Avalonia rasteriza solo el área cliente Z2–Z4: la barra de título del SO (Z1 del mockup aprobado) no figura en `SC-001.txt`; diferencia observable que deberá valorar el juez humano contra el mockup de `plan/mockups.md`.
- HV-003 — La captura materializa únicamente el estado A del mockup (español, menú cerrado, aviso oculto); los estados B (menú desplegado con selección de idioma) y C (aviso flotante VR-002 visible) no quedan capturados; el Run exige juzgar la conformidad contra el mockup — queda constancia para el veredicto humano del gate PLAN_COVERED, sin incidencia pendiente a nivel de herramienta.

## Correcciones requeridas

Ninguna: la Spec v1 y el Plan v1 son correctos y la Implementation responde a ambos (comporte la fórmula abajo). Registradas en el ciclo, no como requisitos al Developer: corrección de tooling de la captura visual (HV-001, anclas reales en `design-system.md`) y retrospectiva C-003 (código en inglés), decisión 2026-10-19; HV-002/HV-003 son desvíos de representación de la captura que se someten al dictamen humano, no defectos de la Implementation ni de la Spec.

## Evidencias

- `verification/executions/E-002/evidence/test-run.json` — `dotnet test project/implementation/egg-md-editor.slnx`, exit_code=0, 139/139 pruebas superadas; `evidence_sha256 8d0b6d896b55168ea868b429066e65718043ae546ad483958334c8d3c46402c1` registrado íntegro en el checkpoint.
- `verification/executions/E-002/evidence/visual-capture/SC-001.txt` + manifiesto `SC-001.json` — captura PNG byte-exacta de SC-001 (superficie ventana-principal, comando anclado en `design-system.md`), exit_code=0; `capture_sha256 fff8d2567ed9cdb930e1036950709bbb51805a9272905d2c1e1f47b3a95f25d7` íntegro contra el manifiesto y `visual_sha256 6ab57aeb35dac62d122560841ff0cf9d7c167161c7e36392014ed8be127c0b09` registrado íntegro en el checkpoint.

## Resultado: Plan cubierto

Fórmula completa: Tareas verificadas (13/13) + Spec cumplida (FR/BR/NFR evidenciadas) + AC cumplidos (15/15, AC-001–AC-015) + DS presentes (3/3, DS-001/002/003) + 0 incidencias pendientes (HV-001 resuelta; HV-002/HV-003 documentadas y sin bloqueo de herramienta); Cobertura 100 % como medida del estado binario. La conformidad visual de la captura (estados y Z1, HV-002/HV-003) queda para el veredicto humano del gate PLAN_COVERED contra `plan/mockups.md`.
