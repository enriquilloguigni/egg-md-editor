# Non-Functional Requirements

NFR-001 — Rendimiento (sincronización)
Los cambios de contenido deben reflejarse en el panel opuesto en menos de 200 ms para documentos de hasta 50 000 palabras.

NFR-002 — Rendimiento (exportación)
La exportación a PDF de un documento de hasta 50 páginas debe completarse en menos de 10 s.

NFR-003 — Escalabilidad
El editor debe soportar documentos de hasta 100 000 palabras o 10 000 líneas sin degradación perceptible superior a 1 s en edición.

NFR-004 — Disponibilidad
La aplicación debe funcionar al 100 % sin conexión a red, sin dependencias de servicios externos ni validaciones remotas.

NFR-005 — Seguridad y privacidad
Ningún dato del usuario sale del equipo: sin telemetría, actualizaciones automáticas ni servicios en la nube; los archivos y preferencias se almacenan solo en local con permisos del sistema operativo.

NFR-006 — Confiabilidad
El autoguardado silencioso cada 30 s y la recuperación automática del último estado al abrir tras un cierre inesperado deben garantizar cero pérdida de datos del usuario.

NFR-007 — Mantenibilidad
La lógica de negocio debe contar con cobertura de pruebas unitarias ≥ 70 % y estar desacoplada de la capa de interfaz.

NFR-008 — Compatibilidad
La aplicación debe ejecutarse en Windows 10+, macOS 12+ y las distribuciones Linux soportadas por .NET 10, sobre el runtime .NET 10 y Avalonia UI.

NFR-009 — Accesibilidad
La interfaz debe cumplir WCAG 2.1 nivel AA (navegación por teclado, contraste, lectores de pantalla compatibles con Avalonia).

NFR-010 — Observabilidad
Los registros técnicos se escriben solo en local, con rotación y niveles configurables, sin datos personales ni envío remoto.

NFR-011 — Cumplimiento
Todas las dependencias deben ser compatibles con la licencia MIT y las atribuciones de terceros deben incluirse en la ventana Acerca de.
