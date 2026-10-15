# Vision — egg-md-editor

## Propósito

Herramienta open source (licencia MIT) de escritorio para escribir documentos Markdown con una única vista del documento editable con doble fidelidad: WYSIWYG y sintaxis Markdown estándar, sincronizadas en tiempo real, 100% offline y sin cargo.

## Objetivo general

Construir la v1 de un editor Pascal de texto para Markdown con UX de procesador tradicional estilo OpenOffice Writer (menús, barra de formato, atajos, modo foco) que cubra el requerimiento completo sin recortes; el Roadmap solo ordena la entrega.

## Problema/oportunidad

El autor de Markdown hoy elige entre editores de texto plano (control total de la sintaxis, sin previsualización cómoda) y herramientas con vista previa (reflejo no instantáneo o parcial, imposible de editar con fidelidad en ambos lados). Ninguna alternativa conocida combina la familiaridad de un procesador de texto tradicional con un único documento Markdown editable también en su sintaxis explícita, más metadatos, ortografía, plantillas y exportación a PDF, funcionando íntegramente offline.

## Usuarios/beneficiarios

- Autores de Markdown en general: usuarios finales de v1, sin cargo; también lectores y posibles contribuidores del repositorio.
- Comunidad open source: contribuye bajo MIT, compila desde fuente y valora la transparencia de licencias y atribuciones.
- Mantenedor del proyecto: decisor humano de la dirección de producto.

## Resultado esperado

Aplicación de escritorio multi-OS (Windows, macOS y Linux, versiones soportadas por .NET 10) distribuida solo como código fuente con build local por el usuario. Cierre de v1 = el requerimiento completo entregado sin recortes, con la doble vista sincronizada fiel como corazón del producto y sin interesados regulatorios externos (producto puramente local).

## Alcance conceptual

In: ventana de procesador con un único documento a la vez; dos vistas del mismo documento (WYSIWYG ↔ Markdown estándar) con correspondencia de posición; formato completo CommonMark (tareas y tachado GFM); archivo y exportación a PDF; metadatos con bloque delimitado; edición y búsqueda; visualización, herramientas y ayuda; UI en español e inglés.
Out: nube y colaboración, instaladores y auto-update, web y móvil, soporte comercial, telemetría y cuentas, bibliotecas multi-archivo, plugins, importación desde otros formatos y versionado interno de documentos.

## Principios/dirección del producto

- Fidelidad de la sincronización bidireccional del mismo documento como diferenciador central.
- UX familiar de procesador de texto tradicional, sin curva de aprendizaje de sintaxis obligatoria.
- 100% local y offline (C-001); ningún dato del usuario sale de su equipo.
- Licencias permisivas compatibles con MIT y atribución transparente (C-002).
- Distribución solo-fuente, reproducible por el usuario en su OS (A-001).
