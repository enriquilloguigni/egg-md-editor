Feature: F-002 — Gestión de archivo
Feature Spec: v1
Plan: v1

## Elementos a comprobar

| Dimensión | IDs | Comprobación |
|---|---|---|
| Funcionales | FR-004, FR-013, AC-010..AC-023, DS-002 | Crear, abrir por selector/arrastre/reciente, guardar y guardar como, recientes, autoguardado silencioso, confirmación de descarte, recuperación al iniciar y persistencia UTF-8 con finales del SO sobre el sistema de archivos; DS-002 con 0 pérdidas en 100 ciclos. |
| Reglas | BR-001, BR-005, BR-007, BR-008, BR-012, BR-F002-001..BR-F002-017 | Un único documento por instancia; bloque de metadatos conservado; UTF-8 y finales normalizados; autoguardado, confirmación, 10 recientes y recuperación; 100 % local/offline; operaciones de archivo sin red ni elevación. |
| Pantallas | SC-001, SC-002, VR-004, VR-005, VR-006 | SC-001 con menú Archivo, título con indicador accesible, submenú Recientes, área de documento, resaltado de arrastre y aviso no modal; SC-002 como selector, confirmación y error; iconos y glifo con nombre accesible (NFR-009). |
| Datos | ENT-008..ENT-014, OP-008..OP-019 | Documento único, bloque de metadatos opaco, recientes ≤10 sin duplicados, copia de recuperación, diálogos y operaciones de crear/abrir/guardar/recuperar/leer/escribir/normalizar. |
| Seguridad | security-permissions.md, NFR-004, NFR-005, NFR-006, NFR-010 | Sin autenticación ni elevación; sin red, telemetría ni escritura fuera de las rutas elegidas y del estado local; cero pérdida de datos; registros solo locales por salida estándar. |
| Comunicación interna | COM-007..COM-015 | Comando de archivo, coordinador-documento, apertura/retorno de SC-002, temporizador de autoguardado, arrastre, recuperación al iniciar, actualización de recientes y estado visible del documento. |
| Externos | external-services.md, NFR-004 | No aplica: 0 servicios externos; se comprueba la ausencia de red y de dependencias remotas en arranque, UI y operaciones de archivo. |
| Errores | ERR-006..ERR-015, LOG-004..LOG-008 | Archivo inexistente/no legible/no UTF-8, fallos de permiso/disco/E/S, validación de nombre/ruta, fallo de autoguardado, copia ilegible y fallo de persistencia de recientes; registros con niveles y datos declarados, sin contenido ni rutas completas. |
| Estados y transiciones | ST-012..ST-017, TR-012..TR-033 | Documento nuevo/limpio/con cambios/recuperado, confirmación pendiente y cerrado; transiciones de editar, guardar, guardar como, abrir, reciente, arrastrar, descartar, recuperar y cerrar, incluidas las no permitidas. |
| Compatibilidad y licencias | NFR-007, NFR-008, NFR-009, NFR-011 | Build net10.0 y ejecución en Linux, plataforma de la corrida; cobertura unitaria de la lógica de archivo ≥70 %; accesibilidad WCAG 2.1 AA sobre SC-001 y SC-002; dependencias MIT-compatibles. |
| AC y tareas del Plan | AC-010..AC-023, DS-002, T-020..T-039 | Cumplimiento de AC-010..AC-023 y de DS-002; verificación de cada tarea del Plan en la tabla siguiente. |

**Tareas del Plan y evidencia**

| Tarea | Comprobación |
|---|---|
| T-020 | Modelo de documento abierto y bloque de metadatos opaco (`OpenDocumentModelTests`, ENT-008/ENT-009, BR-F002-002). |
| T-021 | Almacén de recientes ≤10, sin duplicados, orden descendente (`RecentFilesTests`, ENT-010/ENT-011, OP-019). |
| T-022 | Copia de recuperación e invalidación (`T039FileLogicLocalTests`, ENT-012, BR-F002-012/015). |
| T-023 | Lectura UTF-8 con/sin BOM y normalización de finales de línea (`FileReaderTests`, BR-F002-007/008, OP-017). |
| T-024 | Escritura atómica UTF-8 sin BOM con finales del SO (`FileWriterTests`, BR-F002-005/006, OP-018). |
| T-025 | Operación Nuevo y reemplazo de documento (`NuevoTests`, OP-008, BR-F002-001/002). |
| T-026 | Abrir por selector y arrastre con validación (`T026AbrirVerificationTests`, OP-009/010, BR-F002-003/004, ERR-006/007/008/014). |
| T-027 | Abrir reciente y eliminación de entrada no disponible (`T027AbrirRecienteLocalTests`, OP-011, BR-F002-010, ERR-006). |
| T-028 | Guardar y Guardar como con copia y recientes al frente (`T028GuardarTests`, OP-012/013, BR-F002-005/006/010). |
| T-029 | Confirmación de descarte Guardar/No guardar/Cancelar (`T029ConfirmacionDescarteTests`, OP-016, BR-F002-014/015, ERR-011). |
| T-030 | Autoguardado cada 30 s y aviso no modal (`T030AutoguardadoTests`, OP-014, BR-F002-012, ERR-012, COM-011). |
| T-031 | Recuperación automática al iniciar (`T039FileLogicLocalTests`/`T038LoggingErrorsLocalTests`, OP-015, BR-F002-013/015, ERR-013, COM-013). |
| T-032 | Menú Archivo y atajos (`MainWindowTests`/`MenuAndNoticeTests`, SC-001, COM-007). |
| T-033 | Título con nombre e indicador de cambios accesible (`T033TitleChangeIndicatorTests`, BR-F002-016, COM-015, VR-006). |
| T-034 | Resaltado de arrastre y validación de un único `.md` (`T034DragHighlightLocalTests`, COM-012, BR-F002-003, ERR-014). |
| T-035 | Diálogos SC-002 con foco y validación inline (`T035DialogsSc002LocalTests`/`DialogSurfaceTests`, ENT-013/014). |
| T-036 | Coordinador de archivo y serialización (`CommandDispatcherTests`/`MenuAndNoticeTests`, COM-007..COM-010). |
| T-037 | Persistencia de recientes y manejo de fallo (`T037RecentPersistenceLocalTests`, COM-014, ERR-015, LOG-004/005). |
| T-038 | Registro técnico y errores (`T038LoggingErrorsLocalTests`, LOG-004..LOG-008, ERR-006..ERR-015). |
| T-039 | Pruebas unitarias de lógica de archivo ≥70 % (`T039FileLogicLocalTests`, NFR-007, AC-010..AC-023, DS-002). |

## Actividades y pruebas

1. Ejecutar la suite automatizada completa con el comando anclado y exigir `exit_code=0`, sin fallos ni pruebas omitidas.
2. Comprobar el arranque: SC-001 directa y única, documento vacío sin ruta, título «Sin título», sin bloque de metadatos, menú Archivo con Nuevo, Abrir…, Recientes, Guardar y Guardar como…, Guardar deshabilitado y área de documento vacía.
3. Abrir un `.md` por selector y por arrastre: validación de extensión y cantidad, resaltado visible durante el arrastre, contenido y metadatos conservados, título y ruta actualizados, documento limpio y ruta al frente de recientes; rechazo informado sin cambiar el documento si hay varios archivos, no `.md` o ruta inaccesible.
4. Ejecutar Guardar y Guardar como: escritura UTF-8 sin BOM con finales del SO, conservación del bloque de metadatos y del salto final, desaparición del indicador, ruta al frente de recientes; copia sin modificar el original, adición de `.md`, confirmación de sobrescritura y cancelación sin cambios; fallo conservando cambios y ofreciendo Reintentar/Guardar como/Cancelar.
5. Verificar la lista de recientes: máximo 10, orden descendente, sin duplicados, nombre y carpeta padre, apertura con ratón y teclado, eliminación de la entrada no disponible con informe, y persistencia entre sesiones; submenú vacío deshabilitado.
6. Verificar el autoguardado cada 30 s con cambios: escritura de la copia de recuperación local sin diálogo ni éxito, archivo del usuario intacto, documento sigue marcado; fallo no modal con reintento en el siguiente ciclo.
7. Verificar la confirmación de descarte ante Nuevo, Abrir, reciente, arrastre y cierre: Guardar escribe y continúa (o cierra tras escribir), No guardar descarta e invalida la recuperación (o cierra e invalida), Cancelar aborta y conserva el documento (o mantiene la ventana); si Guardar falla, la operación no continúa; sin cambios el cierre es directo.
8. Verificar la recuperación automática al iniciar: restauración de contenido y ruta, documento marcado con cambios sin guardar, sin diálogo; Guardar escribe; descartar invalida la copia; copia ilegible inicia documento vacío sin tocar el original e informa.
9. Verificar UTF-8, finales de línea y metadatos: lectura con/sin BOM y CRLF/LF/CR, escritura sin BOM con finales del SO, sin añadir ni quitar el salto final, rechazo de archivo no decodificable, bloque de metadatos intacto en contenido, orden y posición.
10. Inducir fallos de E/S (permiso denegado, disco lleno, ruta inválida, archivo no legible) y comprobar informe en el idioma activo, conservación del documento y de la recuperación, archivo original intacto, acciones Reintentar/Guardar como/Cancelar o Elegir otro, y registros LOG-004/LOG-005 con nivel y datos declarados, sin contenido ni rutas completas.
11. Revisar accesibilidad (nombres accesibles, foco inicial seguro, Enter/Esc/Tab, contraste AA) sobre SC-001 y SC-002, y el indicador de cambios no dependiente solo del color.
12. Obtener las capturas visuales de SC-001 en estados A, B y C y de SC-002 en estados A, B y C mediante el comando anclado en `design-system.md`.

Comando de prueba: dotnet test project/implementation/tests/EggMdEditor.App.Tests/EggMdEditor.App.Tests.csproj

## Condiciones

- Spec v1 y Plan v1 vigentes; la corrida se registra sobre el commit de la Execution E-005.
- Tareas T-020..T-039 en `done`; build local con .NET SDK 10.0.x, sin red ni servicios externos (BR-012, NFR-004).
- SC-001 y SC-002 con mockup aprobado en `plan/mockups.md`; captura headless vía el comando anclado en `design-system.md`, que no produce decoraciones del SO (marco y botones nativos), ausentes del mockup.
- NFR-008 se contrasta con la matriz de soporte de .NET 10 (C-011); la corrida se ejecuta en Linux, la plataforma del entorno declarado.
- Cobertura unitaria de la lógica de archivo ≥70 % (NFR-007) contrastada con `evidence/coverage-run.json` si se ejecuta con cobertura.

## Evidencias a obtener

- `executions/E-005/evidence/test-run.json`, producida por `sdd-test-run`, con `exit_code=0` y `sha256` íntegro.
- `executions/E-005/evidence/visual-capture/` con la captura y el manifiesto de SC-001 en estado A (`SC-001.json`, `SC-001.png`), SC-001 en estado B (`SC-001-B.json`, `SC-001-B.png`), SC-001 en estado C con arrastre activo (`SC-001-C.json`, `SC-001-C.png`), SC-002 en estado A (`SC-002.json`, `SC-002.png`), SC-002 en estado B (`SC-002-B.json`, `SC-002-B.png`) y SC-002 en estado C (`SC-002-C.json`, `SC-002-C.png`), producidos por `sdd-visual-capture` con `sha256` íntegro.
- Observaciones de las comprobaciones funcionales, de error y de seguridad consignadas por el reviewer en `results/v1/result.md`.

## Checks visuales

Checks visuales: SC-001, SC-002
Estados visuales: SC-001: A, B, C; SC-002: A, B, C
SC-001 estado C: arrastre activo.
SC-002 estado A: confirmación de descarte; B: error de E/S; C: selector nativo.

## Criterio

Binario — `Plan cubierto = T-020..T-039 verificadas + Spec v1 cumplida + AC-010..AC-023 cumplidos + DS-002 alcanzado + 0 incidencias pendientes`, con `test-run.json` en `exit_code=0` y las capturas de SC-001:A, SC-001:B, SC-001:C (arrastre activo), SC-002:A, SC-002:B y SC-002:C con `sha256` íntegro. Cualquier tarea, AC o DS sin evidencia, cualquier estado visual sin captura o cualquier incidencia abierta deja el resultado en `Plan no cubierto`.