# Discovery Context — egg-md-editor

## Business Objective

Herramienta open source (licencia MIT ya declarada) Pascal de texto para documentos Markdown con doble vista sincronizada en tiempo real (WYSIWYG ↔ Markdown estándar), dirigido a autores de Markdown en general y sin cargo. v1 es una aplicación de escritorio multi-OS (Windows, macOS y Linux) distribuida solo como código fuente con build local por el usuario. Diferenciación frente a productos similares (Zettlr, Joplin, MarkText, VS Code + preview): fidelidad de la sincronización bidireccional del mismo documento y una UX familiar estilo procesador de texto tradicional tipo OpenOffice Writer. Alcance v1 = el requerimiento completo, sin recortes; el Roadmap solo ordena la entrega.

## Problem Statement

El autor de Markdown hoy debe elegir entre editores de texto plano (control total de la sintaxis, sin previsualización cómoda) y herramientas con vista previa (panel o documento separado, reflejo no instantáneo o parcial, imposible de editar con fidelidad en ambos lados). No existe alternativa conocida que combine la familiaridad de un procesador de texto tradicional (menús, barra de formato, atajos, modo foco) con la propiedad central de un único documento Markdown editable también en su sintaxis explícita, más metadatos, ortografía, plantillas y exportación a PDF, funcionando íntegramente offline. La apuesta del proyecto es esa doble vista fiel como corazón del producto.

## Stakeholders

- Mantenedor del proyecto (decisor humano): dueño de dirección de producto y decisiones de negocio; declaró las decisiones de v1 el 2026-10-15.
- Autores de Markdown en general: usuarios finales de v1, sin cargo; también contribuidores y lectores del repositorio open source.
- Comunidad open source: contribuye bajo MIT, compila desde fuente y valora la transparencia de licencias y atribuciones.
- No hay terceros cuyos datos procese la aplicación: es un producto puramente local, sin interesados regulatorios externos.

## Scope

In (capacidades del requerimiento — todas en v1 sin recortes; el Roadmap solo ordena entrega):
- Ventana tipo procesador, única por aplicación y con un único documento abierto a la vez, con menús persistentes (Archivo, Edición, Ver, Formato, Herramientas, Ayuda) y barra de formato con iconos, tooltips y doble naturaleza: efecto visual o escritura de la sintaxis equivalente, actuando sobre la selección o preparando la estructura.
- Dos paneles que son las dos vistas del MISMO documento, sincronizados en tiempo real sin botón de actualizar ni espera: WYSIWYG puro a la izquierda y Markdown estándar a la derecha, editable, con pliegue de secciones y escape con barra invertida.
- Correspondencia de posición entre paneles: clic/salto lleva al lugar equivalente y lo resalta pasajeramente como sección en el otro panel.
- Formato completo: énfasis, delimitadores que abrazan selección, encabezados 1–6 con promover/degradar, listas de viñetas/numeradas/tareas con comportamiento al escribir (CommonMark; tareas y tachado GFM), citas anidadas, código en línea y bloque, enlaces e imágenes con diálogo y anclas internas, tablas editables con TAB, notas al pie, fórmulas en línea/en bloque, diagramas, índice automático H1–H6, insertar rápido/plantillas.
- Archivo: abrir/guardar/guardar como, drag-and-drop de .md, 10 archivos recientes, autoguardado silencioso, aviso de cambios sin guardar al crear nuevo, UTF-8 con finales de línea consistentes por sistema, y exportación a PDF fiel al panel WYSIWYG con diálogo de carpeta/nombre/opciones.
- Metadatos en bloque delimitado al inicio (ficha visual a la izquierda; editable como Markdown a la derecha) con diálogo de propiedades; el idioma del DOCUMENTO declarado ahí rige la corrección ortográfica (independiente del idioma de la UI).
- Edición: deshacer/rehacer, cortar/copiar/pegar (pegar texto formateado o Markdown), selección con doble/triple clic y Mayús+clic, buscar/reemplazar con opciones y coincidencias resaltadas, ir a línea con posición visible, menú contextual adaptado en ambos paneles con sugerencias del corrector y añadir al diccionario personal.
- Ver/Herramientas/Ayuda: mostrar/ocultar barra de formato y paneles, zoom y reset, ajuste de línea, scroll sincronizado, pantalla completa, modo foco, tema claro/oscuro, fuente del Markdown, contador de palabras/caracteres, revisión ortográfica completa en ambos paneles, Acerca de con versión, guía integrada de Markdown y atajos contados donde toca.
- Idiomas de UI v1: español e inglés, con arquitectura i18n extensible.
- Plataformas v1: escritorio en Windows, macOS y Linux, limitadas a las versiones que soporta .NET 10 (el amarre exacto del toolchain se declara en Architecture); distribución como código fuente (sin instaladores ni auto-update).

Out (excluido explícitamente; NO está en el requerimiento ni en las decisiones):
- Sincronización en la nube o almacenamiento remoto de documentos; colaboración o co-edición multi-usuario.
- Instaladores o paquetes firmados por plataforma y actualización automática de la app.
- Otras plataformas: web o móviles (navegador, iOS, Android).
- Soporte comercial, versiones de pago o SLA de producto.
- Telemetría, cuentas de usuario u cualquier servicio de red (ver C-001).
- Gestión de bibliotecas/bóvedas multi-archivo, plugins de terceros, importación desde otros formatos (p. ej., docx/HTML) y versionado interno de documentos.

## Constraints

```
| id    | description                                                                                                                                                                                                                              | source                     | status    |
| C-001 | La aplicación funciona 100% local y offline: sin telemetría, sin servicios de red y sin procesamiento de datos de terceros (sin nube).                                                                                                     | decisión humana 2026-10-15 | confirmed |
| C-002 | Solo licencias permisivas compatibles con MIT en las dependencias, con la atribución declarada en un archivo de terceros y en la ventana Acerca de.                                                                                        | decisión humana 2026-10-15 | confirmed |
```

## Assumptions

- A-001: los usuarios de v1 tienen (o pueden instalar) el toolchain necesario para compilar y ejecutar la aplicación desde el código fuente en su OS, coherente con la distribución solo-fuente.
- A-002: el documento de trabajo v1 es un único archivo Markdown UTF-8 en disco local con bloque de metadatos delimitado al inicio; no hay documentos remotos ni formatos de archivo adicionales.
