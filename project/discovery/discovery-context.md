## Business Objective
Proveer un editor de escritorio (Avalonia UI, multiplataforma) que unifique redacción WYSIWYG y control Markdown en un mismo documento sincronizado en tiempo real, con barra de formato estilo procesador de texto, metadatos, ortografía y exportación a PDF, para usuarios que quieren comodidad visual sin perder control de la sintaxis.
## Problem Statement
Los editores actuales obligan a elegir entre WYSIWYG sin control de sintaxis o Markdown crudo sin vista renderizada; sincronizar ambos a mano es lento y propenso a errores. Falta una herramienta de escritorio familiar (estilo Writer) que traduzca cada gesto a Markdown y mantenga ambos paneles sincronizados al instante, con metadatos, ortografía y exportación.
## Stakeholders
- Usuario final (redactor/autor de documentos Markdown).
- Equipo de desarrollo y soporte (instalación y mantenimiento).
- Administrador de TI en despliegues corporativos (asumido).
## Scope
In scope:
- Ventana de escritorio con menús Archivo/Edición/Ver/Formato/Herramientas/Ayuda y barra de formato.
- Dos paneles sincronizados en tiempo real: WYSIWYG (izquierda) y Markdown (derecha), con sincronización de posición y resaltado pasajero.
- Formato: énfasis, encabezados H1-H6 (promover/degradar), listas (viñetas, numeradas, tareas), citas, código inline/bloque, enlaces (internos y externos), imágenes, tablas, notas al pie, fórmulas (LaTeX), diagramas (Mermaid), índice auto, Insertar rápido (bloques frecuentes), escape de sintaxis con barra invertida.
- Menú contextual de edición y énfasis rápidos.
- Archivo: Nuevo/Abrir (drag&drop)/Guardar/Guardar como, 10 recientes, autoguardado, UTF-8, finales de línea, exportar a PDF.
- Metadatos (front matter) con esquema fijo (título, autor, fecha, idioma) + campos personalizados opcionales; ficha WYSIWYG y diálogo de propiedades; idioma gobierna ortografía.
- Edición: deshacer/rehacer, cortar/copiar/pegar, selección avanzada, buscar/reemplazar, ir a línea, indicador de posición (línea/columna).
- Ver: mostrar/ocultar barra/paneles, zoom, ajuste de línea, scroll sincronizado, pantalla completa, modo foco, tema, fuente, idioma, idioma de la interfaz, plegado de secciones, contador de palabras y caracteres.
- Ortografía en ambos paneles con diccionario personal.
- Ayuda: Acerca de, guía Markdown integrada, atajos.
Out of scope:
- Colaboración multi-usuario y sincronización en la nube.
- Versionado/control de versiones integrado de documentos.
- Plugins/extensibilidad de terceros.
- Móvil o web (solo escritorio).
- Importación de otros formatos (solo .md).
- Impresión directa (solo exportar a PDF).
- Telemetría, actualizaciones automáticas y servicios en la nube.
- Pruebas en múltiples plataformas (cubiertas por Avalonia; no requeridas para la solución actual).
Criterio de cierre: el proyecto termina cuando no existen nuevas features y la verificación es exitosa (project.md).
Dimensionado (confirmado, categoría M):
- Alcance (30%): 8 — 9-10 features / 3 módulos.
- Integraciones (20%): 2 — 1 integración simple.
- Riesgo regulatorio (10%): 3 — datos de usuario básico.
- Superficie (20%): 8 — 16-20 pantallas/dialogos, 1 actor.
- Novedad técnica (10%): 7 — varias tecnologías nuevas combinadas.
- Brownfield (10%): 1 — greenfield, sin código previo.
- Ponderado: 5.50 → categoría M.
## Constraints
C-001 — Interfaz de escritorio con menús clásicos y barra de formato de doble naturaleza (izquierda: efecto visual; derecha: sintaxis) (source: requerimiento.txt; status: confirmed).
C-002 — Sincronización en tiempo real bidireccional entre ambos paneles sin botón de actualizar (source: requerimiento.txt; status: confirmed).
C-003 — Panel izquierdo WYSIWYG puro y panel derecho Markdown estándar editable (source: requerimiento.txt; status: confirmed).
C-004 — Exportación a PDF fiel al panel izquierdo (source: requerimiento.txt; status: confirmed).
C-005 — Metadatos en bloque delimitado al inicio (front matter), esquema fijo + campos personalizados opcionales; idioma declarado gobierna la ortografía (source: requerimiento.txt; status: confirmed).
C-006 — Corrección ortográfica visible en ambos paneles, con sugerencias y diccionario personal (source: requerimiento.txt; status: confirmed).
C-007 — Codificación UTF-8 y finales de línea consistentes (source: requerimiento.txt; status: confirmed).
C-008 — Autoguardado silencioso y lista de 10 archivos recientes (source: requerimiento.txt; status: confirmed).
C-009 — Soporte CommonMark + GFM y escape con backslash en el panel Markdown (source: requerimiento.txt; status: confirmed).
C-010 — Índice auto-generado a partir de encabezados H1-H6 (source: requerimiento.txt; status: confirmed).
C-011 — Stack: Avalonia UI multiplataforma; las pruebas en múltiples plataformas quedan cubiertas por el framework y no son requeridas para la solución actual (source: PD-DISC-001; status: confirmed).
C-012 — Fórmulas en LaTeX (KaTeX/MathJax) y diagramas en Mermaid (source: PD-DISC-003; status: confirmed).
C-013 — Sin telemetría, actualizaciones automáticas ni servicios en la nube; actualizaciones manuales (source: PD-DISC-005; status: confirmed).
## Assumptions
A-001 — Aplicación single-user local; sin colaboración en tiempo real ni sincronización en la nube.
A-002 — Un documento por instancia; multi-ventana no soportada (resuelto OQ-005).
A-003 — Stack: Avalonia UI sobre .NET (resuelto PD-DISC-001).
A-004 — Diagramas en Mermaid y fórmulas en LaTeX (resuelto PD-DISC-003).
A-005 — Idiomas de interfaz: español e inglés, default español (resuelto OQ-002).
A-006 — Accesibilidad WCAG 2.1 AA (resuelto OQ-008).
A-007 — Exportación PDF con márgenes, orientación, encabezados/pies (resuelto OQ-009).
A-008 — Ortografía offline, español e inglés, diccionario personal (resuelto OQ-003).
A-009 — Autoguardado cada 30s con recuperación automática al abrir (resuelto OQ-004).
A-010 — Todo local, sin integraciones externas (resuelto OQ-012).
A-011 — Sin datos sensibles ni telemetría; sin obligaciones regulatorias (resuelto OQ-013).
A-012 — Proyecto greenfield, sin código base ni migración de datos (resuelto OQ-014).
