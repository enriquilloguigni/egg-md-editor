# Architecture Context — egg-md-editor

## Estilo y principios

- Arquitectura de escritorio local-first: un proceso por usuario, sin servidor, sin red y sin servicios externos (C-001); todo el estado vive en el equipo del usuario.
- Una sola ventana con un único documento abierto a la vez (CQ-005), representado por un modelo único del que derivan las dos vistas.
- UX de procesador de texto tradicional: menús persistentes, barra de formato y atajos sobre comandos compartidos.

## Límites y responsabilidades

- Límite del sistema: el proceso de la aplicación. No existen interfaces externas de red ni contratos con terceros (C-001, NFR-002, NFR-003); las interfaces son internas: modelo de documento, servicios y adaptadores.
- UI (menús, barra de formato, dos paneles, diálogos): entrada del usuario y presentación, sin lógica de dominio.
- Modelo de documento y sincronización: representación única, correspondencia de posiciones y reflejo bidireccional <500 ms p95 (NFR-001).
- Servicios internos (archivo UTF-8 con finales de línea por SO, exportación a PDF, ortografía, metadatos, i18n): tras interfaces del núcleo y sin I/O de red.
- Adaptadores de plataforma (Windows, macOS, Linux): rutas, diálogos nativos, fuentes y finales de línea.

## Relaciones entre partes

- UI → ViewModel → modelo único de documento; los dos paneles leen y escriben el mismo modelo por separado.
- El parser CommonMark (con tareas y tachado GFM) es el contrato entre sintaxis y WYSIWYG y media la sincronización en ambas direcciones.
- Exportación PDF y ortografía dependen del modelo, nunca de la UI; la corrección usa el idioma del bloque de metadatos (FR-006, FR-008).

## Tecnologías fundamentales

- Stack: .NET 10 (`net10.0`) y Avalonia UI — decisión humana vigente, base de las tres plataformas con distribución solo-código-fuente.
- Librerías de dominio (Markdown, fórmulas, diagramas, PDF, ortografía, i18n): selección condicionada por C-002; ninguna puede requerir red ni sustituir el stack base.
- Sin tecnologías de datos, red ni telemetría: solo archivos locales y preferencias del usuario.

## Patrones adoptados

- MVVM en Avalonia: vistas delgadas, un ViewModel por pantalla y comandos compartidos entre menú, barra y atajos.
- Modelo único con sincronización bidireccional por eventos (sin sondeo) y mapa de correspondencia de posiciones entre paneles.
- Puerto/adaptador por plataforma y por librería con licencia: el núcleo no conoce SO, rutas ni APIs concretas.
- i18n por recursos (es, en) con inyección de cadenas, extensible más allá de v1 (NFR-006).

## Infraestructura de ejecución

- Ejecución: aplicación de escritorio compilada y ejecutada por el usuario; sin contenedores, sin servicios desplegados y sin red (C-001).
- Plataformas v1: Windows, macOS y Linux en las versiones de la matriz de soporte oficial de .NET 10 (x64 y arm64); el amarre es esa matriz del runtime, no una lista propia del producto (CQ-002, NFR-005, BR-009).
- Toolchain: .NET SDK 10.0.x y TFM `net10.0`; distribución solo como código fuente con build local, sin instaladores, binarios publicados ni auto-update.
- Hardware base mínimo soportado para v1 (fija NFR-001): CPU 64 bits (x86_64/arm64) de ≥2 núcleos a ≥2,0 GHz, 8 GB de RAM, 2 GB de disco libres y pantalla ≥1280×720; sin GPU acelerada ni periféricos de red.
- Persistencia: documento y exportaciones en el sistema de archivos del usuario; preferencias y diccionario personal por idioma (CQ-004) en el directorio de configuración del usuario.

## Decisiones vigentes en texto operativo

- Stack .NET 10 + Avalonia UI y matriz de plataformas de .NET 10: inamovibles sin un ADR aprobado por el humano.
- NFR-001 se certifica sobre el hardware base declarado en Infraestructura de ejecución; NFR-005 sobre la matriz de soporte de .NET 10; NFR-007 exige licencias permisivas compatibles con MIT (C-002).
- Idiomas de UI v1: español e inglés sobre i18n extensible; tema claro/oscuro, modo foco y zoom son atributos de la aplicación.
- Comando de prueba: dotnet test project/implementation/egg-md-editor.slnx
- La solución .NET (aplicación + pruebas unitarias) vive en el árbol único `project/implementation/` (§11) y Verification exige exit_code=0.

## Constraints

```
| id    | description | source | status |
|---|---|---|---|
| C-001 | La aplicación funciona 100% local y offline: sin telemetría, sin servicios de red y sin procesamiento de datos de terceros (sin nube). | heredada de project/discovery/discovery-context.md (decisión humana 2026-10-15) | confirmed |
| C-002 | Solo licencias permisivas compatibles con MIT en las dependencias, con la atribución declarada en un archivo de terceros y en la ventana Acerca de. | heredada de project/discovery/discovery-context.md (decisión humana 2026-10-15) | confirmed |
| C-003 | Todo el código fuente del proyecto se redacta en inglés: identificadores, comentarios y documentación inline; la regla cubre también el código existente, aplicada retroactivamente por refactor ya ejecutado y publicado (completado el 2026-10-19). No altera la gobernanza de los textos de producto (la UI se resuelve del catálogo i18n es/en según Spec, BR-F001-007) ni el contenido stderr/LOG que la Spec fija en español. | decisión humana 2026-10-19 (PD-GEN-003 adopción; PD-GEN-004 retroactividad) | confirmed |
```
