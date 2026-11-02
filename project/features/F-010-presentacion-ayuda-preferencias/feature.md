# F-010 — Presentación, ayuda y preferencias

## Objetivo

Que el usuario pueda personalizar la presentación del editor —mostrar u ocultar las vistas y la barra de formato, ajustar el zoom con restablecimiento, activar el ajuste de línea, sincronizar el desplazamiento, entrar en pantalla completa o modo foco, elegir tema claro/oscuro y fuente del Markdown, plegar secciones y consultar contadores de palabras y caracteres—, acceder a la ayuda integrada —guía de Markdown, listado de atajos y ventana Acerca de con versión y atribuciones— y recuperar todas esas preferencias en la siguiente sesión, de modo que el editor se adapte a su forma de trabajar y recuerde su configuración.

## Prioridad

Prioridad: P1 — must-have del MVP: presentación, ayuda y persistencia de preferencias cierran el alcance central de v1.

## Verificación independiente

Sí — la presentación, la ayuda y la persistencia de preferencias se verifican sobre la ventana principal y sus superficies sin depender de formato completo, edición avanzada ni exportación.

## Referencias Project Spec

- `FR-009` — Presentación y navegación.
- `FR-010` — Localización y ayuda.
- `FR-013` — Persistencia de preferencias.
- `BR-012` — aplicación 100 % local y offline.
- `BR-013` — interfaz en español o inglés (default español).
- `BR-014` — el resaltado de correspondencia entre paneles es pasajero y no modifica el contenido.
- `BR-019` — las preferencias de presentación persisten entre sesiones.
- `NFR-004` — funcionamiento sin red ni servicios externos.
- `NFR-005` — ningún dato del usuario sale del equipo.
- `NFR-007` — lógica de negocio con cobertura unitaria ≥ 70 % y desacoplada de la interfaz.
- `NFR-008` — Windows 10+, macOS 12+ y Linux soportados por .NET 10.
- `NFR-009` — interfaz conforme con WCAG 2.1 AA.
- `NFR-011` — dependencias MIT-compatibles y atribuciones de terceros.
- `NFR-018` — internacionalización de la interfaz en español e inglés.
- `NFR-019` — acciones de menú, barra y atajos responden en menos de 100 ms.

## Dependencias

- `F-001` — Entorno de aplicación: ventana única, menú principal persistente, barra de formato y atajos sobre los que se aplican la presentación y la ayuda; dependencia directa.
- `F-003` — Doble vista sincronizada: paneles cuya visibilidad, desplazamiento y zoom se presentan y persisten; dependencia directa.

## Estado

planned
