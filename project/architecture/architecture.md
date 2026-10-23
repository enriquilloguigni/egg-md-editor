# Architecture Context — egg-md-editor

## Estilo y principios

- Arquitectura de escritorio local-first: un proceso por usuario, sin servidor, sin red ni servicios externos; todo el estado vive en el equipo del usuario (C-001, C-013, NFR-004, NFR-005).
- Un único documento por instancia, representado por un modelo único del que derivan las dos vistas sincronizadas (BR-001, C-002).
- UX de procesador de texto tradicional: menús persistentes, barra de formato y atajos sobre comandos compartidos (FR-011, FR-012).
- CommonMark + GFM como contrato de sintaxis del documento (C-009, BR-009).

## Límites y responsabilidades

- Límite del sistema: el proceso de la aplicación. No existen interfaces externas de red ni contratos con terceros (NFR-004, NFR-005); las interfaces son internas: modelo de documento, servicios y adaptadores.
- UI (menús, barra de formato, dos paneles, diálogos): entrada del usuario y presentación, sin lógica de dominio.
- Modelo de documento y sincronización: representación única, correspondencia de posiciones y reflejo bidireccional en menos de 200 ms (NFR-001).
- Servicios internos (archivo UTF-8 con finales de línea por SO, exportación a PDF, ortografía, metadatos, i18n): tras interfaces del núcleo y sin I/O de red.
- Adaptadores de plataforma (Windows, macOS, Linux): rutas, diálogos nativos, fuentes y finales de línea.

## Relaciones entre partes

- UI → ViewModel → modelo único de documento; los dos paneles leen y escriben el mismo modelo por separado.
- El parser CommonMark (con tareas y tachado GFM) es el contrato entre sintaxis y WYSIWYG y media la sincronización en ambas direcciones.
- Exportación a PDF y ortografía dependen del modelo, nunca de la UI; la corrección usa el idioma del bloque de metadatos (FR-006, FR-008).
- Metadatos: bloque delimitado al inicio con esquema fijo más campos personalizados, visible como ficha y editable como Markdown (C-005, BR-005).

## Tecnologías fundamentales

- Stack: .NET 10 (`net10.0`) y Avalonia UI multiplataforma (C-011, NFR-008); distribución como código fuente con build local.
- Librerías de dominio (Markdown, fórmulas LaTeX vía KaTeX/MathJax, diagramas Mermaid, PDF, ortografía, i18n): selección en Plan/Implementation; ninguna puede requerir red ni sustituir el stack base; licencias MIT-compatibles (NFR-011).
- Sin tecnologías de datos, red ni telemetría: solo archivos locales y preferencias del usuario (C-013, NFR-005).
- Persistencia de preferencias y diccionario personal por idioma en el directorio de configuración del usuario (A-008).

## Patrones adoptados

- MVVM en Avalonia: vistas delgadas, un ViewModel por pantalla y comandos compartidos entre menú, barra y atajos.
- Modelo único con sincronización bidireccional por eventos (sin sondeo) y mapa de correspondencia de posiciones entre paneles.
- Puerto/adaptador por plataforma y por librería con licencia: el núcleo no conoce SO, rutas ni APIs concretas.
- i18n por recursos (es, en) con inyección de cadenas, extensible más allá de v1 (A-005).
- Accesibilidad WCAG 2.1 AA sobre los controles de Avalonia (A-006, NFR-009).

## Infraestructura de ejecución

- Ejecución: aplicación de escritorio compilada y ejecutada por el usuario; sin contenedores, sin servicios desplegados y sin red (C-001).
- Plataformas v1: Windows 10+, macOS 12+ y las distribuciones Linux soportadas por .NET 10 (NFR-008); el amarre es la matriz de soporte del runtime.
- Toolchain: .NET SDK 10.0.x y TFM `net10.0`; distribución solo como código fuente, sin instaladores, binarios publicados ni auto-update (C-013).
- Hardware base mínimo (NFR-001): CPU 64 bits (x86_64/arm64) de ≥2 núcleos a ≥2,0 GHz, 8 GB de RAM, 2 GB de disco libres y pantalla ≥1280×720; sin GPU acelerada ni periféricos de red.
- Persistencia: documento y exportaciones en el sistema de archivos del usuario; preferencias y diccionario personal en el directorio de configuración del usuario.

## Decisiones vigentes en texto operativo

- Stack .NET 10 + Avalonia UI y matriz de plataformas de .NET 10: inamovibles sin un ADR aprobado por el humano.
- NFR-001 se certifica sobre el hardware base declarado; NFR-005 sobre la matriz de soporte de .NET 10; NFR-011 exige licencias MIT-compatibles.
- Idiomas de UI v1: español e inglés, default español; tema claro/oscuro, modo foco y zoom son atributos de la aplicación (A-005).
- Un documento por instancia, sin multi-ventana (A-002, BR-001).
- Autoguardado cada 30 s con recuperación automática al abrir (A-009, BR-008).

## Constraints

```
| id | description | source | status |
|---|---|---|---|
| C-001 | Interfaz de escritorio con menús clásicos y barra de formato de doble naturaleza (izquierda: efecto visual; derecha: sintaxis). | requerimiento.txt | confirmed |
| C-002 | Sincronización en tiempo real bidireccional entre ambos paneles sin botón de actualizar. | requerimiento.txt | confirmed |
| C-003 | Panel izquierdo WYSIWYG puro y panel derecho Markdown estándar editable. | requerimiento.txt | confirmed |
| C-004 | Exportación a PDF fiel al panel izquierdo. | requerimiento.txt | confirmed |
| C-005 | Metadatos en bloque delimitado al inicio (front matter), esquema fijo + campos personalizados opcionales; idioma declarado gobierna la ortografía. | requerimiento.txt | confirmed |
| C-006 | Corrección ortográfica visible en ambos paneles, con sugerencias y diccionario personal. | requerimiento.txt | confirmed |
| C-007 | Codificación UTF-8 y finales de línea consistentes. | requerimiento.txt | confirmed |
| C-008 | Autoguardado silencioso y lista de 10 archivos recientes. | requerimiento.txt | confirmed |
| C-009 | Soporte CommonMark + GFM y escape con backslash en el panel Markdown. | requerimiento.txt | confirmed |
| C-010 | Índice auto-generado a partir de encabezados H1-H6. | requerimiento.txt | confirmed |
| C-011 | Stack: Avalonia UI multiplataforma; las pruebas en múltiples plataformas quedan cubiertas por el framework. | PD-DISC-001 | confirmed |
| C-012 | Fórmulas en LaTeX (KaTeX/MathJax) y diagramas en Mermaid. | PD-DISC-003 | confirmed |
| C-013 | Sin telemetría, actualizaciones automáticas ni servicios en la nube; actualizaciones manuales. | PD-DISC-005 | confirmed |
```
