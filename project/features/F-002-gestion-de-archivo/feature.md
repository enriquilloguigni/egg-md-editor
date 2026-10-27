# F-002 — Gestión de archivo

## Objetivo

Que el usuario pueda crear documentos nuevos, abrir archivos Markdown existentes —también soltándolos sobre la ventana— y guardarlos en el mismo sitio o como copia con otro nombre, sin preocuparse por el formato ni por perder el trabajo; que vuelva a los últimos archivos desde una lista de recientes y cuente con autoguardado silencioso y un aviso claro antes de descartar cambios pendientes. Todo se lee y escribe en UTF-8 con finales de línea coherentes con el sistema, de modo que el mismo archivo se abra igual en cualquier equipo.

## Prioridad

Prioridad: P1 — must-have del MVP: sin gestión de archivo no hay documento persistente que las demás Features puedan editar, revisar o exportar.

## Verificación independiente

Sí — crear, abrir, guardar, recientes, autoguardado y aviso de cambios se comprueban sobre el sistema de archivos y la ventana existente, sin depender de la doble vista sincronizada, el formato ni la exportación.

## Referencias Project Spec

- `FR-004` — crear, abrir, guardar y guardar como; arrastre de `.md`, recientes, autoguardado silencioso y aviso de cambios sin guardar; persistencia UTF-8 con finales de línea consistentes.
- `BR-005` — bloque de metadatos al inicio: se conserva íntegro al abrir y guardar; su edición guiada corresponde a F-007.
- `BR-007` — lectura y escritura en UTF-8 con finales de línea normalizados según el sistema operativo.
- `BR-008` — autoguardado silencioso, confirmación antes de descartar cambios, 10 recientes en orden descendente y recuperación del último estado al abrir.
- `BR-012` — aplicación 100 % local y offline: los archivos y la lista de recientes no salen del equipo.
- `NFR-004` — funcionamiento sin red ni servicios externos.
- `NFR-005` — ningún dato del usuario sale del equipo; archivos y preferencias solo en local.
- `NFR-006` — cero pérdida de datos con el autoguardado y la recuperación automática.
- `NFR-007` — lógica de archivo con cobertura unitaria ≥ 70 % y desacoplada de la interfaz.
- `NFR-008` — Windows 10+, macOS 12+ y Linux soportados por .NET 10; rutas y finales de línea por sistema.
- `NFR-009` — diálogos y avisos conformes con WCAG 2.1 AA.
- `NFR-011` — dependencias MIT-compatibles y atribuciones de terceros.
- `C-007`, `C-008` — constraints de origen: codificación UTF-8 con finales de línea consistentes y autoguardado con 10 recientes.
- `A-009` — supuesto confirmado: autoguardado cada 30 s con recuperación automática al abrir.

## Dependencias

- `F-001` — Entorno de aplicación: ventana única, menú Archivo y comandos base sobre los que se conectan las acciones de archivo; única dependencia, previa a esta Feature.

## Estado

Delimitada — pendiente de Feature Spec v1 y Plan v1.
