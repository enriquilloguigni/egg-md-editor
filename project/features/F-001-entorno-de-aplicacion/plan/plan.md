# Plan — F-001 Entorno de aplicación

F-001, Spec: v1

## Dependencias

- Features: No requerida — F-001 es la primera entrega del roadmap v3, sin dependencias de otras Features.
- Componentes: No requerida — no hay código previo en el repositorio; nace con F-001.
- Servicios externos: No requerida — aplicación 100 % offline (C-001); la Spec declara ausencia de servicios externos.
- Datos: No requerida — sin persistencia; los catálogos de F-001 se crean y viven en memoria.
- Configuración: comando de prueba `dotnet test` en la raíz del repositorio y arranque sin variables de entorno ni red (architecture.md, C-001).
- Librerías: .NET SDK 10.0.x (TFM `net10.0`), Avalonia UI y framework de pruebas de la solución, con licencias compatibles con MIT (C-002).
- Otras: `plan/mockups.md` de `SC-001`, redactado por el diseñador, exigido por el gate PLAN_REVIEW.

## Riesgos

- Relabelizado en caliente en Avalonia sin reinicio (AC-007, AC-008): bindings estáticos que no refrescan rótulos ni avisos → resolver toda cadena desde una fuente observable notificada al conmutar y testear la conmutación conservando estado (DS-003).
- Desviación del recuento del 100 % de FR-011/FR-012 (DS-001): entradas o controles ausentes o de más → catálogos como fuente única de la UI y test automático de recuento frente a las listas declaradas.
- Conflicto de las combinaciones asignadas con los atajos que conectarán F-028/F-029 → registro único de combinaciones en el despachador y revisión de ese registro al conectar cada acción.
- Solicitud de segunda ventana acotada a proceso único por la Spec, sin canal interprocesal → disparador en proceso conforme a Spec; si se requiere atender un segundo proceso lanzado, escalar el cambio antes de implementarlo.
- Atajos, avisos flotantes y cierre con comportamiento distinto por SO (Windows/macOS/Linux) → gestos por plataforma y verificación en la matriz de soporte de .NET 10 (NFR-005).

## Secuencia

1. Preparación — solución .NET 10 + Avalonia y proyecto de pruebas en la raíz, con `dotnet test` operativo (T-001). Sin dependencias.
2. Datos — catálogos de sesión, menú y barra, y catálogo i18n es/en (T-002, T-003). Depende de 1.
3. Backend — servicio de idioma con abortado y fallback, despachador de atajos, disparador de segunda ventana, errores y logs (T-004–T-007). Depende de 2.
4. Frontend — SC-001 con ventana única, menú 100 %, barra de formato 100 %, entrada de idioma y área de documento vacía (T-008–T-010). Depende de 2.
5. Integraciones — cableado en SC-001 de COM-001–COM-004: relabelizado, atajos, estado visual y cierre (T-011). Depende de 3 y 4.
6. Configuración — arranque en español sin escritura a disco ni red, `dotnet test` en la raíz (T-012). Depende de 5.
7. Integración final — pruebas de recuento, localización, atajos y cierre con `exit_code=0` (T-013). Depende de 6.

## Tareas

- T-001 Crear en la raíz del repositorio la solución .NET 10 con el proyecto de aplicación Avalonia (`net10.0`) y el proyecto de pruebas, dejando `dotnet test` operativo.
- T-002 Crear los catálogos de sesión (idioma, menú, aviso) y las listas ordenadas de menú (FR-011) y de barra de formato (FR-012) como fuente única de recuento.
- T-003 Crear el catálogo de cadenas i18n clave→texto es/en, extensible a más idiomas, con defecto español y resolución de claves ausentes en español (BR-F001-007, ERR-004).
- T-004 Implementar el servicio de conmutación de idioma (COM-001) con abortado sin cambio parcial (ERR-003) y registro LOG-002.
- T-005 Implementar el despachador de atajos (COM-002) con las combinaciones asignadas en este Plan: `Ctrl+Shift+L` (macOS `Cmd+Shift+L`) alterna es/en y `Ctrl+Q` (macOS `Cmd+Q`) cierra la aplicación; cualquier otro atajo permanece inerte (BR-F001-003).
- T-006 Implementar el disparador de solicitud de segunda ventana (AC-014): coordinador en proceso que, con SC-001 existente, la enfoca sin crear otra ventana.
- T-007 Implementar los errores y registros de arranque y de cierre (ERR-001, ERR-002, ERR-005 → LOG-001, LOG-003) con salida de error en español por salida estándar.
- T-008 Construir SC-001 con ventana única y barra de menú persistente al 100 % de las entradas de FR-011, visibles y sin acción conectada (ADR-001).
- T-009 Construir la barra de formato al 100 % de los controles de FR-012 en orden de escritura, con icono (VR-001) y aviso flotante (VR-002) por control.
- T-010 Añadir la entrada Herramientas → Idioma de la interfaz (Español | English) con selección mutuamente excluyente, y dejar el área de documento vacía (AC-015).
- T-011 Cablear en SC-001 los canales COM-001–COM-004: relabelizado en caliente, atajos, estado visual de menú y avisos, y cierre con liberación de la sesión (COM-004).
- T-012 Fijar la configuración de ejecución: arranque siempre en español sin escritura a disco (BR-F001-006), sin acceso a red (C-001) y con el comando `dotnet test` en la raíz.
- T-013 Escribir las pruebas de recuento, localización, atajos y cierre (AC-001–AC-015, DS-001–DS-003) y dejar `dotnet test` con `exit_code=0`.
