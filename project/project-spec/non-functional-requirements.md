# Non-Functional Requirements — egg-md-editor

NFR-001 — Rendimiento
La latencia de sincronización bidireccional entre vistas deberá ser inferior a 500 ms en percentil p95 para documentos de hasta 1.000.000 de caracteres, medida sobre el hardware base mínimo soportado que declara Architecture para v1 (CQ-001).

NFR-002 — Disponibilidad
La aplicación deberá ser plenamente operable sin conectividad de red: ninguna funcionalidad de v1 podrá depender de servicios externos (C-001).

NFR-003 — Seguridad
Ningún dato del documento o del usuario deberá abandonar el equipo del usuario: sin telemetría, sin servicios de red y sin procesamiento de datos de terceros (C-001).

NFR-004 — Confiabilidad
Toda operación de guardado deberá preservar el contenido del documento sin pérdida, en UTF-8 y con finales de línea consistentes con el sistema.

NFR-005 — Compatibilidad
El sistema deberá soportar las versiones de OS soportadas por .NET 10 en Windows, macOS y Linux (CQ-002), y el parsing y renderizado Markdown serán conformes a CommonMark con tareas y tachado según GFM.

NFR-006 — Mantenibilidad
La localización de la UI deberá basarse en una arquitectura i18n extensible más allá de los idiomas de v1 (español e inglés).

NFR-007 — Cumplimiento
El proyecto se distribuirá bajo licencia MIT y solo se admitirán dependencias con licencias permisivas compatibles con MIT, con la atribución declarada en un archivo de terceros y en la ventana Acerca de (C-002, CQ-003).

Escalabilidad y observabilidad: no requerido / fuera de alcance — aplicación local de un único documento sin servicios ni telemetría (C-001, CQ-005).
