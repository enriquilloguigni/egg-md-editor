# F-001 — Entorno de aplicación

## Objetivo

Que la aplicación arranque como entorno base en una sola ventana, con menú principal persistente, barra de formato, atajos de teclado, interfaz conmutable español/inglés sobre i18n extensible y tema claro/oscuro, mostrando la forma completa de menú y barra (sus opciones visibles) sin conectarlas aún a sus acciones.

## Prioridad

Prioridad: P1 — feature fundacional: primera entrega del roadmap v1, sin dependencias.

## Verificación independiente

Sí — su comportamiento puede comprobarse de forma aislada del resto de Features.

## Referencias Project Spec

- `FR-010` — localización y ayuda (interfaz es/en, guía, atajos, Acerca de).
- `FR-011` — menú principal persistente (Archivos, Edición, Ver, Formato, Herramientas, Ayuda); en esta Feature solo su parte visual.
- `FR-012` — barra de formato con icono y aviso flotante; en esta Feature solo su parte visual.
- `BR-001` — un único documento por instancia.
- `BR-012` — aplicación 100 % local y offline.
- `BR-013` — interfaz en español o inglés (default español).
- `NFR-004` — funcionamiento sin red ni servicios externos.
- `NFR-005` — sin telemetría ni datos fuera del equipo.
- `NFR-008` — ejecución en Windows 10+, macOS 12+ y Linux soportados por .NET 10.
- `NFR-009` — accesibilidad WCAG 2.1 AA.
- `NFR-011` — dependencias MIT-compatibles y atribuciones en Acerca de.

## Dependencias

Ninguna — F-001 no depende de ninguna otra Feature (roadmap v1).

## Estado

planned — feature.md redactado; spec pendiente.