Feature: F-001 — Entorno de aplicación
Feature Spec: v2
Plan: v2

## Elementos a comprobar

| Dimensión | IDs | Comprobación |
|---|---|---|
| Funcionales | FR-009, FR-010, FR-011, FR-012, AC-001..AC-009 | Arranque directo en SC-001 con los 6 rótulos como botones, la navegación secuencial y exactamente los 12 controles de FR-012 en orden; conmutación es/en y de tema en caliente sin reinicio; navegación cíclica entre rótulos; opciones y atajos sin acción no alteran el documento. |
| Reglas | BR-001, BR-012, BR-013, BR-F001-001..BR-F001-010 | Menú y barra persistentes y completos; un único idioma activo con defecto español; idioma y tema no persistidos al cerrar; inventario de 12 controles; tema Claro/Oscuro/Sistema visible y excluyente; navegación secuencial; área de documento vacía. |
| Pantallas | SC-001, SC-002, SC-003, VR-001, VR-002 | SC-001 con menú, navegación, barra, avisos flotantes y área vacía; SC-002 y SC-003 visibles sin contenido conectado; iconos de biblioteca con nombre accesible y atribución MIT en SC-003 (NFR-009, NFR-011). |
| Datos | ENT-001..ENT-007, OP-001..OP-007 | Sesión única y efímera; preferencias en memoria; catálogo i18n es/en con fallback a español; atajos registrados; rótulo seleccionado 1–6; operaciones OP-001..OP-007. |
| Seguridad | security-permissions.md, NFR-005, NFR-010 | Sin autenticación ni elevación; sin red, telemetría ni escritura fuera del directorio de configuración; una instancia por sesión; registros solo locales por salida estándar. |
| Comunicación interna | COM-001..COM-006 | Despacho compartido menú/barra/atajo, relabelizado, aplicación de tema, aviso flotante, foco de segunda instancia y estado de menú con navegación. |
| Externos | external-services.md, NFR-004 | No aplica: 0 servicios externos; se comprueba la ausencia de red y de dependencias remotas en arranque y UI. |
| Errores | ERR-001..ERR-005, LOG-001..LOG-003 | Fallo de arranque, pintado y cierre con código de error, stderr en español y sin residuales; aborto de conmutación sin cambio parcial; fallback i18n; registros locales. |
| Estados y transiciones | ST-001..ST-011, TR-001..TR-011 | Estados iniciales, menú cerrado/desplegado, aviso oculto/visible, tema claro/oscuro/sistema, idioma es/en, rótulo seleccionado 1–6 y transiciones no permitidas. |
| Compatibilidad y licencias | NFR-008, NFR-011 | Build net10.0 y ejecución en Linux, plataforma de la corrida; dependencias con licencia MIT-compatible verificadas; atribución de iconos visible en SC-003. |
| AC y tareas del Plan | AC-001..AC-009, DS-001, T-001..T-019 | Cumplimiento de los 9 AC y de DS-001; verificación de cada tarea del Plan en la tabla siguiente. |

**Tareas del Plan y evidencia**

| Tarea | Comprobación |
|---|---|
| T-001 | Compilación en net10.0 y suite del proyecto sin errores de restauración ni de build (`SmokeTests`, `ApplicationLifecycleTests`). |
| T-002 | Tema FluentTheme claro/oscuro y recursos i18n base aplicados al arranque (`ThemeSwitchTests`). |
| T-003 | Catálogo i18n es/en con claves únicas y fallback a español (`I18nCatalogTests`, ERR-004). |
| T-004 | Sesión y preferencias en memoria: idioma, tema, menú, rótulo y aviso (`SessionPreferencesTests`). |
| T-005 | Dispatcher de comandos y registro de atajos sin acciones conectadas (`CommandDispatcherTests`, ADR-001). |
| T-006 | SC-001 con los 6 rótulos y el área de documento vacía (`MainWindowTests`, FR-011, BR-F001-008). |
| T-007 | Barra de formato con los 12 controles FR-012 en orden, icono, tooltip y nombre accesible (`FormatBarTests`, VR-001/VR-002). |
| T-008 | Conmutación de idioma en caliente y aborto sin cambio parcial (`LanguageSwitchTests`, TR-002/TR-003, ERR-003). |
| T-009 | Resolución del tema claro/oscuro/sistema heredado del SO (`ThemeSwitchTests`, TR-008/TR-009). |
| T-010 | Apertura/cierre de menú y aviso flotante (`MenuAndNoticeTests`, COM-004/COM-006, TR-004..TR-007). |
| T-011 | Atajos disponibles responden; el resto inerte sin efecto sobre el documento (`ShortcutHandlerTests`, BR-F001-003). |
| T-012 | Ventana única, foco en segunda instancia y cierre de ventana (`SingleInstanceTests`, `WindowLifecycleTests`, COM-005, OP-006, TR-001). |
| T-013 | Errores de arranque/pintado/cierre y logging (`ApplicationLifecycleTests`, `CloseErrorHandlingTests`, ERR-001..ERR-005, LOG-001..LOG-003). |
| T-014 | Superficies SC-002 y SC-003 visibles sin contenido conectado (`DialogSurfaceTests`, `HelpSurfaceTests`). |
| T-015 | 6 rótulos como botones y navegación secuencial cíclica Siguiente/Anterior sin abrir menú (`MenuNavigationTests`, OP-007, TR-010/TR-011, BR-F001-010). |
| T-016 | Entrada Herramientas → Tema con Claro/Oscuro/Sistema, estado oscuro observable (`ThemeSwitchTests`, OP-002, BR-F001-009, AC-008). |
| T-017 | Iconos de biblioteca MIT en los 12 controles y atribución visible en SC-003 (`FormatBarTests`, `HelpSurfaceTests`, VR-001, NFR-011). |
| T-018 | E2E de foco de segunda instancia, códigos de salida/stderr, ausencia de ventana usable y de procesos residuales, y no persistencia de idioma o tema (`ProcessLifecycleEndToEndTests`, `SingleInstanceTests`, AC-005/AC-006/AC-007). |
| T-019 | Sin alias/API especulativa y pruebas de comportamiento en su lugar (`CommandDispatcherTests`, `SessionPreferencesTests`, `I18nCatalogTests`). |

## Actividades y pruebas

1. Ejecutar la suite automatizada completa con el comando anclado y exigir `exit_code=0`, sin fallos ni pruebas omitidas.
2. Comprobar el arranque: SC-001 directa y única, 6 rótulos como botones, 2 botones de navegación, los 12 controles en orden con icono y aviso, área de documento vacía, idioma inicial Español y tema resuelto del SO.
3. Conmutar es/en en caliente conservando ventana, menú y aviso; inducir fallo del catálogo destino y verificar el aborto sin cambio parcial (ERR-003); revisar el fallback a español (ERR-004).
4. Abrir Herramientas → Tema y comprobar las tres opciones mutuamente excluyentes, la aplicación en caliente y la resolución de Sistema desde el SO; capturar el estado oscuro (AC-008, OP-002, TR-008/TR-009).
5. Pulsar «Siguiente» y «Anterior» desde los extremos y verificar el recorrido cíclico 1→6→1, la marca del rótulo seleccionado y que no se abre ningún menú ni cambia el documento (AC-009, OP-007, TR-010/TR-011).
6. Abrir/cerrar rótulos, mostrar avisos flotantes y pulsar atajos disponibles e inertes, verificando que el documento no cambia (AC-003, AC-004, BR-F001-002/003).
7. Solicitar una segunda instancia y verificar que no se crea otra ventana y que SC-001 recibe el foco (AC-005, COM-005).
8. Cerrar SC-001 y verificar terminación sin procesos residuales, código de salida correcto y ausencia de persistencia del idioma y del tema en el siguiente arranque (AC-007, ERR-005, BR-F001-006).
9. Inducir fallo de arranque, de pintado y de cierre: código de error, mensaje en español por stderr, sin residuales y registros LOG-001..LOG-003 con nivel y datos declarados, sin telemetría ni escritura a disco (AC-006, ERR-001..ERR-005).
10. Revisar accesibilidad (nombres accesibles, foco y contraste AA) sobre SC-001, SC-002 y SC-003 (NFR-009), y la atribución de la biblioteca de iconos en SC-003 (NFR-011).
11. Obtener las capturas visuales de SC-001 en estados A y B, SC-002 en A y SC-003 en A mediante el comando anclado en `design-system.md`.

Comando de prueba: dotnet test project/implementation/tests/EggMdEditor.App.Tests/EggMdEditor.App.Tests.csproj

## Condiciones

- Spec v2 y Plan v2 vigentes; la corrida se registra sobre el commit de la Execution E-003.
- Build local con .NET SDK 10.0.x; `dotnet test` sin `DOTNET_ROOT`; sin red ni servicios externos (BR-012, NFR-004).
- Tareas T-001..T-019 en `done`; ADR-002 aplicado a Spec, Plan y mockups.
- SC-001, SC-002 y SC-003 con mockup v2 aprobado en `plan/mockups.md`; captura headless vía el comando anclado en `design-system.md`, que no produce decoraciones del SO (marco y botones nativos), ausentes del mockup.
- NFR-008 se contrasta con la matriz de soporte de .NET 10 (C-011); la corrida se ejecuta en Linux, la plataforma del entorno declarado.

## Evidencias a obtener

- `executions/E-003/evidence/test-run.json`, producida por `sdd-test-run`, con `exit_code=0` y `sha256` íntegro.
- `executions/E-003/evidence/visual-capture/` con la captura y el manifiesto de SC-001 en estado A (`SC-001.json`, `SC-001.png`), SC-001 en estado B (`SC-001-B.json`, `SC-001-B.png`), SC-002 en estado A (`SC-002.json`, `SC-002.png`) y SC-003 en estado A (`SC-003.json`, `SC-003.png`), producidos por `sdd-visual-capture` con `sha256` íntegro.
- Observaciones de las comprobaciones funcionales, de error y de seguridad consignadas por el reviewer en `results/v2/result.md`.

## Checks visuales

Checks visuales: SC-001, SC-002, SC-003
Estados visuales: SC-001: A, B; SC-002: A; SC-003: A

## Criterio

Binario — `Plan cubierto = T-001..T-019 verificadas + Spec v2 cumplida + AC-001..AC-009 cumplidos + DS-001 alcanzado + 0 incidencias pendientes`, con `test-run.json` en `exit_code=0` y las capturas de SC-001:A, SC-001:B, SC-002:A y SC-003:A con `sha256` íntegro. Cualquier tarea, AC o DS sin evidencia, cualquier estado visual sin captura o cualquier incidencia abierta deja el resultado en `Plan no cubierto`.
