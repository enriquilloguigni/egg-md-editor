# Business Rules

| ID | Regla | Fuente |
|---|---|---|
| BR-001 | La aplicación opera sobre un único documento por instancia; no se abren múltiples documentos en la misma ventana. | A-002 |
| BR-002 | Los dos paneles reflejan el mismo documento en tiempo real; no existe botón de actualizar. | C-002 |
| BR-003 | El panel izquierdo es WYSIWYG puro (sin sintaxis visible) y el derecho muestra Markdown estándar editable. | C-003 |
| BR-004 | La exportación a PDF reproduce fielmente el panel izquierdo (encabezados, listas, tablas, código, notas, fórmulas, diagramas). | C-004 |
| BR-005 | Los metadatos viven en un bloque delimitado al inicio con esquema fijo (título, autor, fecha, idioma) más campos personalizados opcionales; el idioma declarado gobierna la ortografía; el bloque es editable a mano en el panel derecho y se presenta como ficha en el izquierdo. | C-005 |
| BR-006 | La corrección ortográfica se muestra en ambos paneles con sugerencias y diccionario personal que persiste entre sesiones. | C-006, A-008 |
| BR-007 | Los archivos se leen y escriben en UTF-8; los finales de línea se normalizan según el sistema operativo. | C-007 |
| BR-008 | El autoguardado silencioso se ejecuta cada 30 segundos sin diálogos; los cambios sin guardar se confirman antes de descartar; la lista de recientes contiene como máximo los 10 últimos archivos en orden descendente; al abrir se recupera automáticamente el último estado autoguardado si quedaron cambios sin guardar. | C-008, A-009 |
| BR-009 | El documento cumple CommonMark + GFM; la barra invertida escapa caracteres literales en el panel Markdown. | C-009 |
| BR-010 | El índice se genera automáticamente a partir de los encabezados H1-H6 y se actualiza al cambiarlos. | C-010 |
| BR-011 | Las fórmulas (LaTeX) y los diagramas (Mermaid) se renderizan en el panel izquierdo y se incluyen en la exportación a PDF. | C-012 |
| BR-012 | La aplicación es 100% local y offline: ningún dato del usuario sale del equipo; sin telemetría, actualizaciones automáticas ni servicios en la nube; las actualizaciones son manuales. | C-013, A-010, A-011 |
| BR-013 | La interfaz se presenta en español o inglés según la preferencia del usuario (default español). | A-005 |
| BR-014 | El resaltado de correspondencia entre paneles es pasajero y no modifica el contenido. | C-002 |
| BR-015 | La aplicación no imprime directamente ni importa formatos distintos de .md; no ofrece colaboración multi-usuario, sincronización en la nube, plugins ni móvil/web. | Out of scope |
| BR-016 | La barra de formato actúa sobre el fragmento seleccionado o prepara la estructura si no hay selección; su efecto depende del panel activo (efecto visual en el izquierdo, sintaxis equivalente en el derecho); cada control muestra icono y aviso flotante. | C-001 |
| BR-017 | Deshacer, rehacer, cortar, copiar y pegar operan sobre el documento único, independientemente del panel activo; el portapapeles entrega texto formateado o Markdown según el destino. | requerimiento.txt |
| BR-018 | Los enlaces a secciones del mismo documento se resuelven por el encabezado de destino, componiendo la aplicación la ancla correspondiente; el salto funciona en el panel WYSIWYG y en el PDF exportado. | requerimiento.txt |
| BR-019 | Las preferencias de presentación (tema claro/oscuro, zoom y restablecimiento, visibilidad de paneles y barra de formato, fuente del Markdown, idioma de interfaz) persisten entre sesiones. | requerimiento.txt |
| BR-020 | Los modos de visualización (pantalla completa, modo foco, plegado de secciones, ajuste de línea, zoom) cambian solo la presentación y no modifican el contenido del documento. | requerimiento.txt |
| BR-021 | La exportación a PDF se configura antes de generar: el usuario elige carpeta, nombre y opciones (márgenes, orientación, encabezados y pies). | A-007 |
