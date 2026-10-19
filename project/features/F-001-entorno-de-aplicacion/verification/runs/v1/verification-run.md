# Verification Run — F-001 Entorno de aplicación

Feature: F-001 — Entorno de aplicación
Feature Spec: v1 (`spec/`) · Plan: v1 (`plan/plan.md`, T-001–T-013)

## Elementos a comprobar

```
| Dimensión | Elementos | Evidencia esperada |
|---|---|---|
| Funcionales | FR-010 (localización y atajos), FR-011 y FR-012 en su parte visual (ADR-001), NFR-006 (i18n extensible); DS-001 recuento 100 % con 0 ausencias y 0 elementos no declarados, DS-002 resolución 100 % de claves, DS-003 conmutación sin reinicio ni cambio parcial | test-run.json exit_code=0 · captura SC-001 |
| Reglas | BR-003 (una ventana, un documento), BR-007 (español/inglés sobre i18n extensible) y BR-F001-001…BR-F001-008 | test-run.json exit_code=0 |
| Pantallas | SC-001 según mockup aprobado (plan/mockups.md): Z1 título, Z2 menú con 6 entradas al 100 % de FR-011, Z3 barra al 100 % de FR-012 con icono VR-001 y aviso VR-002 por control, Z4 área de documento vacía, sin menú contextual de Edición (BR-F001-008) | captura SC-001 · test-run.json |
| Datos | Catálogos de sesión, menú, barra de formato y cadenas i18n: exactamente un idioma activo con defecto español, entradas y controles completos y en orden de escritura, claves únicas con textos es/en, nada escrito a disco | test-run.json exit_code=0 |
| Seguridad | Perfil único local sin permisos ni autenticación (A-001); sin red, sin disco, ningún dato fuera del equipo (C-001); sesión liberada al cerrar | test-run.json exit_code=0 |
| Comunicación interna | COM-001 conmutación de idioma, COM-002 despachador de atajos, COM-003 estado visual de menú y avisos, COM-004 cierre de sesión; todos síncronos y en proceso | test-run.json exit_code=0 |
| Externos | Ausencia de EXT-xxx (C-001): verificación negativa — arranque, pruebas y cierre completan sin acceso a red ni a servicios externos | test-run.json exit_code=0 |
| Errores | ERR-001 y ERR-002 (fallo de arranque: sin ventana usable, stderr en español, código de error, sin procesos residuales), ERR-003 (aborto sin cambio parcial), ERR-004 (fallback a español), ERR-005 (cierre con excepción); niveles de LOG-001/LOG-002/LOG-003 según logs.md | test-run.json exit_code=0 |
| Estados/transiciones | ST-001→ST-002 (TR-001), ST-002⇄ST-003 (TR-002/TR-003), ST-002/ST-003→ST-004 (TR-004/TR-005); no permitidas: arranque directo en inglés, conmutación abortada sin transición (ERR-003), segunda ventana sin transición (AC-014) y reanudación de ST-004 | test-run.json exit_code=0 |
| AC + tareas | AC-001–AC-015 cubiertos por las pruebas y T-001–T-013 verificadas | test-run.json exit_code=0 · execution.md |
```

Cobertura AC → prueba:

```
| AC | Prueba |
|---|---|
| AC-001, AC-012 | Arranque: ventana única en español con TR-001 a ST-002 y sin arranque directo en inglés (ST-001→ST-003 no permitida); fallo de inicialización o pintado con stderr «Error al iniciar la aplicación: …», código de error y sin procesos residuales (ERR-001/ERR-002, LOG-001) |
| AC-002, AC-003, AC-015 | Forma completa: 6 menús con sus entradas al 100 % (FR-011), barra al 100 % de FR-012 con icono (VR-001) y aviso flotante (VR-002) por control, superficie acotada a menú + barra con área de documento vacía |
| AC-004, AC-005 | Opciones sin acción: el menú abre y cierra y el control muestra su aviso, sin efecto sobre documento ni sesión (BR-F001-002, ADR-001) |
| AC-006, AC-014 | COM-002: responden los atajos disponibles y el resto permanece inerte (BR-F001-003); solicitud de segunda ventana enfoca SC-001 existente sin crear otra |
| AC-007, AC-008, AC-009, AC-010 | COM-001: es⇄en en caliente conservando ventana, estado del menú y aviso (TR-002/TR-003, DS-003); fallback a español de claves sin resolver (ERR-004, LOG-002 Warning); aborto sin cambio parcial con catálogo del destino no disponible (ERR-003, LOG-002 Error) |
| AC-011, AC-013 | COM-004: cierre con ST-004, sin procesos residuales y sin persistencia del idioma (BR-F001-006); excepción al cerrar o liberar termina igualmente (ERR-005, LOG-003) |
| T-001–T-013 | Verificadas por las pruebas anteriores y por `dotnet test` operativo en la raíz de `project/implementation/` (T-001, T-012, T-013) |
```

## Actividades / pruebas

- `sdd-test-run(F-001-entorno-de-aplicacion)` ejecuta `dotnet test` en `project/implementation/` (comando anclado en architecture.md, aprobado en DESIGN_APPROVED) → `evidence/test-run.json` con `exit_code=0` y `sha256`.
- `sdd-visual-capture` de SC-001 (comando anclado en design-system.md, superficie ventana-principal) → `evidence/visual-capture/`.
- La conformidad de la captura con el mockup aprobado (plan/mockups.md, Z1–Z4 con estados de arranque, menú desplegado y aviso visible) la juzga el humano en el Result.
- Los desvíos se registran como hallazgos `HV-xxx` en el Result; la prosa del tester no es evidencia (§12.5).

## Condiciones

- PLAN_COVERED exige `exit_code=0` en `test-run.json` con hash íntegro contra el checkpoint y la evidencia visual de SC-001 con `sha256` (§12.5).
- Entorno: build local desde código fuente (A-001), sin red, sin variables de entorno ni escritura a disco; catálogos en memoria (C-001, T-012).
- Alcance ADR-001: verificación limitada a la parte visual; la conexión de acciones (F-028/F-029), vistas y contenido de documento (F-002/F-003) y persistencia del idioma (F-030) quedan fuera de las pruebas.
- Iteración: hallazgo que revele Spec equivocada → NEEDS_HUMAN_DECISION → Spec v2 → Plan v2 → Run v2; Implementation incorrecta → corrección del Developer y nueva Execution del mismo Run v1.

## Checks visuales

Checks visuales: SC-001 — la forma completa y la conformidad con el mockup son el entregable central de la Feature (BR-F001-001, DS-001, VR-001/VR-002); comando anclado en design-system.md y mockup aprobado en plan/mockups.md.
