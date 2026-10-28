# Non-Functional Requirements

NFR-001 — Rendimiento (sincronización)
Los cambios de contenido deben reflejarse en el panel opuesto en menos de 200 ms para documentos de hasta 50 000 palabras.

NFR-002 — Rendimiento (exportación)
La exportación a PDF de un documento de hasta 50 páginas debe completarse en menos de 10 s.

NFR-003 — Escalabilidad (documento)
El editor debe soportar documentos de hasta 100 000 palabras o 10 000 líneas sin degradación perceptible superior a 1 s en edición.

NFR-004 — Disponibilidad (offline)
La aplicación debe funcionar al 100 % sin conexión a red, sin dependencias de servicios externos ni validaciones remotas; los diccionarios de ortografía español e inglés se incluyen para uso offline.

NFR-005 — Seguridad y privacidad
Ningún dato del usuario sale del equipo: sin telemetría, actualizaciones automáticas ni servicios en la nube; los archivos y preferencias se almacenan solo en local con permisos del sistema operativo.

NFR-006 — Confiabilidad (autoguardado)
El autoguardado silencioso cada 30 s y la recuperación automática del último estado al abrir tras un cierre inesperado deben garantizar cero pérdida de datos del usuario.

NFR-007 — Mantenibilidad
La lógica de negocio debe contar con cobertura de pruebas unitarias ≥ 70 %, estar desacoplada de la capa de interfaz y organizarse en capas independientes y testables.

NFR-008 — Compatibilidad
La aplicación debe ejecutarse en Windows 10+, macOS 12+ y las distribuciones Linux soportadas por .NET 10, sobre el runtime .NET 10 y Avalonia UI.

NFR-009 — Accesibilidad
La interfaz debe cumplir WCAG 2.1 nivel AA: navegación completa por teclado, contraste mínimo 4.5:1 para texto normal y 3:1 para texto grande, foco visible y compatibilidad con lectores de pantalla soportados por Avalonia.

NFR-010 — Observabilidad
Los registros técnicos se escriben solo en local, con rotación y niveles configurables, sin datos personales ni envío remoto.

NFR-011 — Cumplimiento (licencias)
Todas las dependencias deben ser compatibles con la licencia MIT y las atribuciones de terceros deben incluirse en la ventana Acerca de.

NFR-012 — Rendimiento (arranque)
La aplicación debe estar lista para usar en menos de 3 s tras el lanzamiento en el hardware mínimo soportado.

NFR-013 — Rendimiento (renderizado)
El renderizado de una fórmula o un diagrama insertado debe completarse en menos de 1 s.

NFR-014 — Rendimiento (búsqueda)
Buscar y reemplazar sobre un documento de hasta 100 000 palabras debe completarse en menos de 1 s.

NFR-015 — Rendimiento (índice)
La regeneración del índice tras añadir, quitar o modificar un encabezado debe completarse en menos de 500 ms.

NFR-016 — Escalabilidad (tablas)
El editor debe soportar tablas de hasta 1 000 filas y 50 columnas sin degradación perceptible superior a 1 s.

NFR-017 — Seguridad (sanitización)
Al abrir un archivo Markdown no se ejecuta código arbitrario ni se resuelven enlaces peligrosos; el contenido se trata como texto y los enlaces externos requieren confirmación del usuario.

NFR-018 — Internacionalización
La interfaz debe estar disponible en español e inglés; los documentos y la ortografía deben soportar Unicode; los diccionarios español e inglés deben estar disponibles sin conexión.

NFR-019 — Usabilidad (respuesta)
Las acciones de menú, barra de formato y atajos de teclado deben responder en menos de 100 ms.

NFR-020 — Confiabilidad (recuperación)
La recuperación automática del último estado tras un cierre inesperado debe completarse en menos de 5 s.

NFR-021 — Cumplimiento (PDF)
El PDF generado debe ser legible por lectores estándar, con texto seleccionable y metadatos básicos (título, autor).
"