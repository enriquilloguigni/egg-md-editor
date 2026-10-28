# Vision

## Propósito

Proveer un editor de escritorio multiplataforma que unifique la redacción WYSIWYG y el control directo de la sintaxis Markdown en un mismo documento sincronizado en tiempo real, con barra de formato familiar estilo procesador de texto, metadatos, corrección ortográfica y exportación a PDF, para autores de Markdown que quieren comodidad visual sin renunciar al control de la sintaxis. El producto es 100% local y sin servicios en la nube.

## Objetivo general

Construir el editor Markdown completo cubriendo el alcance íntegro del requerimiento (FUENTE-001) sin recortes: una única ventana de procesador con dos vistas del mismo documento (WYSIWYG ↔ Markdown estándar) con correspondencia bidireccional de contenido y posición, formato completo CommonMark + GFM, gestión de archivo y exportación a PDF, metadatos, edición y búsqueda, visualización, herramientas y ayuda, en interfaz español/inglés. El Roadmap solo ordena la entrega.

## Problema/oportunidad

Los editores actuales obligan a elegir entre WYSIWYG sin control de sintaxis o Markdown crudo sin vista renderizada; sincronizar ambos a mano es lento y propenso a errores. Falta una herramienta de escritorio familiar (estilo Writer) que traduzca cada gesto a Markdown y mantenga ambos paneles sincronizados al instante, con metadatos, ortografía y exportación, funcionando íntegramente offline.

## Usuarios/beneficiarios

- Autores de Markdown en general: usuarios finales del producto.
- Comunidad open source: contribuye bajo la licencia MIT del proyecto, compila desde fuente y valora la transparencia de licencias y atribuciones.
- Mantenedor del proyecto: decisor humano de la dirección de producto.
- Administrador de TI en despliegues corporativos (asumido): instalación y mantenimiento local.

## Resultado esperado

Aplicación de escritorio multiplataforma (Windows, macOS y Linux, versiones soportadas por .NET 10 / Avalonia) bajo licencia MIT, conforme al requerimiento completo FUENTE-001. Cierre = el requerimiento completo entregado sin recortes, con la doble vista sincronizada fiel como corazón del producto y sin interesados regulatorios externos (producto puramente local). Criterio de cierre heredado de Discovery: el proyecto termina cuando no existen nuevas features y la verificación es exitosa.

## Alcance conceptual

In: ventana de procesador con un único documento a la vez; dos vistas del mismo documento (WYSIWYG ↔ Markdown estándar) con correspondencia de posición; formato completo CommonMark (tareas y tachado GFM); archivo y exportación a PDF; metadatos con bloque delimitado; edición y búsqueda; visualización, herramientas y ayuda; UI en español e inglés.

Out: colaboración multi-usuario y sincronización en la nube; versionado/control de versiones integrado de documentos; plugins/extensibilidad de terceros; móvil o web; importación de otros formatos (solo .md); impresión directa (solo exportar a PDF); telemetría, actualizaciones automáticas y servicios en la nube; pruebas en múltiples plataformas (cubiertas por Avalonia).

## Principios/dirección del producto

- Fidelidad de la sincronización bidireccional del mismo documento como diferenciador central.
- UX familiar de procesador de texto tradicional, sin curva de aprendizaje de sintaxis obligatoria.
- 100% local y offline; ningún dato del usuario sale de su equipo.
- Licencias permisivas compatibles con MIT y atribución transparente.
- Un documento por instancia; sin colaboración ni cuentas.