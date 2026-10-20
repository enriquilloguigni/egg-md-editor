# F-002 — Gestión de archivo

## Objetivo

Que el usuario pueda crear, abrir, guardar, guardar como y autoguardar de forma silenciosa un único documento Markdown UTF-8, abrir archivos .md por arrastre sobre la ventana, reabrir con un clic cualquiera de una lista de archivos recientes limitada a 10 archivos y ordenada por uso, y recibir aviso de cambios sin guardar.

## Prioridad

Prioridad: P1 — segunda entrega del roadmap v3 y base de la cadena de documento: F-003, F-008 y F-012 dependen de esta gestión de archivo, y F-028 la conecta al menú Archivo.

## Verificación independiente

Sí — crear, abrir (con y sin arrastre), guardar, guardar como, autoguardar, recientes y aviso de cambios sin guardar se comprueban de forma aislada sobre la ventana e i18n entregados por F-001, sin requerir las vistas de edición (F-003) ni ninguna otra Feature.

## Referencias Project Spec

- `FR-004` — núcleo funcional de la Feature; texto en Project Spec v2 (referencia, no copia).
- `NFR-004` — confiabilidad: todo guardado preserva el contenido sin pérdida, en UTF-8 y con finales de línea consistentes con el sistema.
- `BR-003` — un único documento abierto a la vez: el objeto que esta Feature crea, abre y guarda.
- `BR-004` — el documento de trabajo es un único archivo Markdown UTF-8 en disco local; la dimensión de su bloque de metadatos corresponde a F-008.
- `BR-001` — la gestión de archivo es 100 % local y offline, sin servicios de red (C-001).
- `BR-007` — los textos de usuario de la Feature (recientes, aviso, diálogos) se resuelven del catálogo i18n es/en entregado por F-001.
- `BR-010` — alcance completo sin recortes: este scope deriva de la fila F-002 del roadmap v3.

## Dependencias

- F-001 — Entorno de aplicación (completada): ventana única, comandos compartidos y base i18n (es/en) donde encaja la gestión de archivo; dependencia declarada en roadmap v3 y satisfecha.
- Ninguna otra: las Features que consumen el documento (F-003, F-008, F-012, F-028) dependen de F-002; no la habilitan.

## Scope

- Crear documento nuevo.
- Abrir documentos Markdown, incluido por arrastre de archivos .md sobre la ventana.
- Guardar y guardar como, con persistencia UTF-8 y finales de línea consistentes por sistema operativo.
- Autoguardado silencioso del documento.
- Aviso de cambios sin guardar (FR-004).
- Lista de archivos recientes: máximo 10 archivos, ordenada por uso y abrible con un clic.

## Out of scope

- Edición y formato del contenido del documento (F-003, F-004, F-005).
- Bloque de metadatos del documento (F-008).
- Exportación a PDF y su entrada de menú (F-010).
- Conexión funcional de estas acciones a las entradas del menú Archivo —su parte visual ya está entregada por F-001— (F-028).
- Persistencia de preferencias del usuario (F-030).

## Estado

planned — feature.md redactado; spec pendiente.
