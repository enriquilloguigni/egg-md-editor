# F-001 — Entorno de aplicación

## Objetivo

Que el usuario encuentre al abrir la aplicación una única ventana de trabajo con un menú principal persistente de seis rótulos navegables, una barra de formato con doce controles identificables, atajos de teclado, una interfaz conmutable entre español e inglés y un tema claro, oscuro o heredado del sistema, de modo que reconozca la estructura completa del editor y pueda recorrer sus opciones —los seis rótulos del menú, no sus entradas internas— aunque las acciones asociadas todavía no estén conectadas.

## Prioridad

Prioridad: P1 — feature fundacional: primera entrega del roadmap v1, sin dependencias.

## Verificación independiente

Sí — su comportamiento puede comprobarse de forma aislada del resto de Features.

## Referencias Project Spec

- `FR-009` — presentación y navegación (tema Claro/Oscuro/Sistema).
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

implementing — spec v2 y plan v2 vigentes; re-ejecución de gates en curso.