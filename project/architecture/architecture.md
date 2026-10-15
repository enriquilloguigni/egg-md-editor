# Architecture Context — egg-md-editor

## Stack tecnológico

**Decisión humana (mantenedor, 2026-10-15): .NET 10 + Avalonia UI.** Este Context refleja esa decisión vigente; todo agente debe respetarla sin reabrirla.

| Área | Decisión vigente | Trazabilidad |
|---|---|---|
| Runtime y lenguaje | .NET 10 (C#), compilación `dotnet build` local en cada OS | BR-008, BR-009, NFR-005, A-001 |
| UI de escritorio | Avalonia UI (XAML + MVVM), renderizado por software, sin dependencia de GPU | FR-001, FR-009, BR-003 |
| Plataformas | Windows, macOS y Linux en las versiones soportadas por .NET 10 | BR-009, NFR-005, FR-010 |
| Parsing Markdown | Implementación conforme a CommonMark con tareas/tachado GFM dentro del proceso (sin servicios externos) | FR-002, NFR-005 |
| Exportación a PDF | Generación local dentro del proceso, fiel a la vista WYSIWYG | FR-005 |
| Ortografía | Motor local con diccionario personal por idioma en disco del usuario | FR-008, BR-005, BR-006 |
| Persistencia | Único archivo Markdown UTF-8 en disco local; guardado sin pérdida, EOL consistentes | FR-004, NFR-004, BR-004, A-002 |
| Localización | Arquitectura i18n extensible; v1 español e inglés | NFR-006, BR-007, FR-010 |
| Red | Sin cliente HTTP, sin telemetría, sin almacenamiento remoto: 100% local y offline | C-001, BR-001, NFR-002, NFR-003 |
| Dependencias | Solo paquetes NuGet con licencia permisiva compatible con MIT, atribución en archivo de terceros y en Acerca de | C-002, BR-002, NFR-007 |

Patrones: aplicación monolítica de un solo proceso; una única ventana con un único documento abierto (BR-003); los dos paneles son dos vistas del mismo documento en memoria con sincronización bidireccional en proceso (FR-001) — sin cola, red ni persistencia intermedia; MVVM en toda la UI; modelo de documento único deshacer/rehacer central (FR-007).

## Hardware base mínimo soportado (fija NFR-001)

NFR-001 exige latencia de sincronización bidireccional < 500 ms en p95 para documentos de hasta 1.000.000 de caracteres «sobre el hardware base mínimo soportado que declara Architecture». Para v1 ese hardware base queda declarado y acotado como sigue (medida de referencia y suelo de soporte):

| Recurso | Mínimo soportado |
|---|---|
| CPU | 2 núcleos físicos x86-64 o ARM64 a 2.5 GHz o superior |
| RAM | 8 GB |
| Almacenamiento | 1 GB libre en disco local |
| Pantalla | 1366×768 |
| Aceleración GPU | No requerida (renderizado por software) |
| Red | No requerida: el hardware mínimo no incluye conectividad (C-001, NFR-002) |
| SO | Versiones soportadas por .NET 10 en Windows, macOS y Linux (NFR-005, BR-009) |

La verificación de NFR-001 se ejecuta sobre esta configuración; por debajo de ella no hay compromiso de rendimiento. Mencionados en Architecture: NFR-001, NFR-002, NFR-004, NFR-005, NFR-006, NFR-007 — cada NFR conserva su redacción original en Project Spec.

## Superficies

- Ventana única de procesador (menús Archivo/Edición/Ver/Formato/Herramientas/Ayuda + barra de formato) con dos paneles: WYSIWYG a la izquierda, Markdown estándar a la derecha — FR-001, FR-009.
- Diálogos: abrir/guardar como, exportación a PDF, propiedades de metadatos, buscar/reemplazar, ir a línea, Acerca de con atribuciones — FR-004, FR-005, FR-006, FR-007, FR-010.
- Idiomas de UI: español e inglés (FR-010, BR-007). Los IDs `SC-xxx` de pantallas y `TK-xxx` de tokens se asignan en `design-system.md` (fase Design).

## Constraints

Tier S: este proyecto no tiene `constraints.md`; el bloque sustituye al tier S.

```
| id    | descripción                                                                                                                                        | vínculo                                                        | origen                        | estado    |
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------|-------------------------------|-----------|
| C-001 | La aplicación funciona 100% local y offline: sin telemetría, sin servicios de red y sin procesamiento de datos de terceros (sin nube).              | BR-001 · NFR-002 · NFR-003 · FR-004 · vision (principios)      | decisión humana 2026-10-15    | confirmed |
| C-002 | Solo licencias permisivas compatibles con MIT en las dependencias, con la atribución declarada en un archivo de terceros y en la ventana Acerca de. | BR-002 · NFR-007 · FR-010 (Acerca de con atribuciones)        | decisión humana 2026-10-15    | confirmed |
```

Restricciones de soporte derivadas sin ID propio: distribución solo-fuente (BR-008, A-001), documento único en disco local (BR-004, A-002), ventana única con un documento abierto (BR-003), y el hardware base mínimo declarado arriba como suelo de NFR-001.

## Trazabilidad de requisitos

FR cubiertos por el stack/superficies: FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-007, FR-008, FR-009, FR-010. BR: BR-001…BR-010. NFR: NFR-001…NFR-007. Ningún requisito se crea ni se modifica aquí; el mapeo FR → Features (`F-xxx`) se hace en el Roadmap, cuando los Features existan.

Comando de prueba: `dotnet build`
