Feature: F-001 — Entorno de aplicación
Feature Spec: v1
Plan: v1

## Elementos a comprobar

| Dimensión | IDs | Comprobación |
|---|---|---|
| Funcionales | FR-010, FR-011, FR-012, AC-001..AC-004 | Arranque directo en SC-001 con los 6 menús y el 100 % de los controles de barra; conmutación es/en en caliente sin reinicio; opciones y atajos sin acción no alteran el documento. |
| Reglas | BR-001, BR-012, BR-013, BR-F001-001..BR-F001-008 | Menú y barra persistentes y completos; un único idioma activo con defecto español; idioma no persistido al cerrar; área de documento vacía. |
| Pantallas | SC-001, SC-002, SC-003, VR-001, VR-002 | SC-001 con menú, barra, avisos flotantes y área vacía; SC-002 y SC-003 visibles sin contenido conectado; iconos con nombre accesible (NFR-009). |
| Datos | ENT-001..ENT-007, OP-001..OP-006 | Sesión única y efímera; preferencias en memoria; catálogo i18n es/en con fallback a español; atajos registrados; operaciones OP-001..OP-006. |
| Seguridad | security-permissions.md, NFR-005, NFR-010 | Sin autenticación ni elevación; sin red, telemetría ni escritura fuera del directorio de configuración; una instancia por sesión. |
| Comunicación interna | COM-001..COM-006 | Despacho compartido menú/barra/atajo, relabelizado, tema, aviso flotante, foco de segunda instancia y estado de menú. |
| Externos | external-services.md, NFR-004 | No aplica: 0 servicios externos; se comprueba la ausencia de red y de dependencias remotas en arranque y UI. |
| Errores | ERR-001..ERR-005, LOG-001..LOG-003 | Fallo de arranque, pintado y cierre con código de error, stderr en español y sin residuales; aborto de conmutación sin cambio parcial; fallback i18n; registros locales. |
| Estados y transiciones | ST-001..ST-010, TR-001..TR-009 | Estados iniciales, menú cerrado/desplegado, aviso oculto/visible, tema claro/oscuro, idioma es/en y transiciones no permitidas. |
| Compatibilidad y licencias | NFR-008, NFR-011 | Build net10.0 y ejecución en la plataforma de la corrida; dependencias con licencia MIT-compatible verificadas; atribuciones de SC-003 conectadas en F-010. |
| AC y tareas del Plan | AC-001..AC-007, DS-001, T-001..T-014 | Cumplimiento de los 7 AC y de DS-001; verificación de cada tarea del Plan en la tabla siguiente. |

**Tareas del Plan y evidencia**

| Tarea | Comprobación |
|---|---|
| T-001 | Compilación en net10.0 y suite del proyecto sin errores de restauración ni de build (`SmokeTests`, `ApplicationLifecycleTests`). |
| T-002 | Tema FluentTheme claro/oscuro y recursos i18n base aplicados al arranque (`ThemeSwitchTests`). |
| T-003 | Catálogo i18n es/en con claves únicas y fallback a español (`I18nCatalogTests`, ERR-004). |
| T-004 | Sesión y preferencias en memoria: idioma, tema, menú y aviso (`SessionPreferencesTests`). |
| T-005 | Dispatcher de comandos y registro de atajos sin acciones conectadas (`ShortcutHandlerTests`, ADR-001). |
| T-006 | SC-001 con los 6 menús al 100 %, persistente y solo visual (`MainWindowTests`, FR-011). |
| T-007 | Barra de formato con controles FR-012 en orden, icono, tooltip y nombre accesible (`FormatBarTests`, VR-001/VR-002). |
| T-008 | Conmutación de idioma en caliente y aborto sin cambio parcial (`LanguageSwitchTests`, TR-002/TR-003, ERR-003). |
| T-009 | Conmutación de tema claro/oscuro heredado del SO (`ThemeSwitchTests`, TR-008/TR-009). |
| T-010 | Apertura/cierre de menú y aviso flotante (`MenuAndNoticeTests`, COM-004/COM-006, TR-004..TR-007). |
| T-011 | Atajos disponibles responden; el resto inerte sin efecto sobre el documento (`ShortcutHandlerTests`, BR-F001-003). |
| T-012 | Ventana única, foco en segunda instancia y cierre de ventana (`SingleInstanceTests`, `WindowLifecycleTests`, COM-005, OP-006, TR-001). |
| T-013 | Errores de arranque/pintado/cierre y logging (`ApplicationLifecycleTests`, `CloseErrorHandlingTests`, ERR-001..ERR-005, LOG-001..LOG-003). |
| T-014 | Superficies SC-002 y SC-003 visibles sin contenido conectado (`DialogSurfaceTests`, `HelpSurfaceTests`). |

## Actividades y pruebas

1. Ejecutar la suite automatizada completa con el comando anclado y exigir `exit_code=0`, sin fallos ni pruebas omitidas.
2. Comprobar el arranque: SC-001 directa y única, 6 menús, 100 % de controles con icono y aviso, área de documento vacía e idioma inicial Español.
3. Conmutar es/en en caliente conservando ventana, menú y aviso; inducir fallo del catálogo destino y verificar el aborto sin cambio parcial (ERR-003); revisar el fallback a español (ERR-004).
4. Abrir/cerrar entradas de menú, mostrar avisos flotantes, conmutar tema y pulsar atajos disponibles e inertes, verificando que el documento no cambia.
5. Solicitar una segunda instancia y verificar que no se crea otra ventana y que SC-001 recibe el foco.
6. Cerrar SC-001 y verificar terminación sin procesos residuales, código de salida correcto y ausencia de persistencia del idioma en el siguiente arranque.
7. Inducir fallo de arranque, de pintado y de cierre: código de error, mensaje en español por stderr, sin residuales y registros LOG-001..LOG-003 con nivel y datos declarados, sin telemetría ni escritura a disco.
8. Revisar accesibilidad (nombres accesibles, foco y contraste AA) sobre SC-001, SC-002 y SC-003 (NFR-009).

Comando de prueba: dotnet test project/implementation/tests/EggMdEditor.App.Tests/EggMdEditor.App.Tests.csproj

## Condiciones

- Spec v1 y Plan v1 vigentes; la corrida se registra sobre el commit de la Execution E-001.
- Build local con .NET SDK 10.0.x (`DOTNET_ROOT=$HOME/.dotnet` para el runner), sin red ni servicios externos (BR-012, NFR-004).
- SC-001, SC-002 y SC-003 con mockup aprobado en `plan/mockups.md`; captura headless vía el comando anclado en `design-system.md`, que no produce decoraciones del SO (marco y botones nativos), ausentes del mockup.
- NFR-008 se contrasta con la matriz de soporte de .NET 10 (C-011); la corrida se ejecuta en Linux, la plataforma del entorno declarado.

## Evidencias a obtener

- `executions/E-001/evidence/test-run.json`, producida por `sdd-test-run`, con `exit_code=0` y `sha256` íntegro.
- `executions/E-001/evidence/visual-capture/` con la captura y el manifiesto de SC-001, SC-002 y SC-003 en estado base, producidos por `sdd-visual-capture` con `sha256` íntegro.
- Observaciones de las comprobaciones funcionales, de error y de seguridad consignadas por el reviewer en `results/v1/result.md`.

## Checks visuales

Checks visuales: SC-001, SC-002, SC-003
Estados visuales: SC-001: A, SC-002: A, SC-003: A

## Criterio

Binario — `Plan cubierto = T-001..T-014 verificadas + Spec v1 cumplida + AC-001..AC-007 cumplidos + DS-001 alcanzado + 0 incidencias pendientes`, con `test-run.json` en `exit_code=0` y las tres capturas base (SC-001:A, SC-002:A, SC-003:A) con `sha256` íntegro. Cualquier tarea, AC o DS sin evidencia, o cualquier incidencia abierta, deja el resultado en `Plan no cubierto`.
