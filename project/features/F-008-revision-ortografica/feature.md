# F-008 — Revisión ortográfica

## Objetivo

Que el usuario pueda revisar la ortografía del documento en ambas vistas según el idioma declarado en los metadatos, con subrayado de las palabras desconocidas, sugerencias de corrección y un diccionario personal ampliable que persiste entre sesiones, funcionando completamente sin conexión.

## Prioridad

Prioridad: P1 — must-have del MVP: la ortografía es parte del alcance central de v1 y depende del idioma declarado en los metadatos (F-007).

## Verificación independiente

Sí — la revisión ortográfica se verifica en ambos paneles con subrayado, sugerencias y diccionario personal sin depender de exportación a PDF.

## Referencias Project Spec

- `FR-008` — Revisión ortográfica.
- `BR-005` — el idioma declarado en los metadatos gobierna la ortografía.
- `BR-006` — la corrección ortográfica se muestra en ambos paneles con sugerencias y diccionario personal que persiste entre sesiones.
- `BR-007` — lectura y escritura en UTF-8 con finales de línea normalizados según el sistema operativo.
- `BR-012` — aplicación 100 % local y offline.
- `NFR-004` — funcionamiento sin red ni servicios externos; diccionarios español e inglés incluidos para uso offline.
- `NFR-005` — ningún dato del usuario sale del equipo.
- `NFR-007` — lógica de ortografía con cobertura unitaria ≥ 70 % y desacoplada de la interfaz.
- `NFR-008` — Windows 10+, macOS 12+ y Linux soportados por .NET 10.
- `NFR-009` — interfaz y diálogos conformes con WCAG 2.1 AA.
- `NFR-011` — dependencias MIT-compatibles y atribuciones de terceros.
- `NFR-018` — soporte Unicode e internacionalización; diccionarios español e inglés disponibles sin conexión.
- `C-006` — corrección ortográfica visible en ambos paneles, con sugerencias y diccionario personal.
- `C-007` — codificación UTF-8 y finales de línea consistentes.
- `A-008` — persistencia del diccionario personal por idioma en el directorio de configuración del usuario.

## Dependencias

- `F-003` — Doble vista sincronizada: la ortografía se muestra en ambos paneles sobre el mismo documento; dependencia directa.
- `F-007` — Metadatos del documento: el idioma declarado en el bloque de metadatos gobierna el diccionario y la corrección; dependencia directa.

## Estado

planned
