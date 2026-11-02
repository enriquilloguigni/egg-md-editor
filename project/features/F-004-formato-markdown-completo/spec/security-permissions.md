# Security-permissions — F-004 Formato Markdown completo

## Roles y operaciones

Rol: Usuario local (identidad = cuenta del SO que ejecuta la aplicación; sin autenticación ni cuentas propias).

- Aplicar formato completo (énfasis, encabezados, listas, citas, código, enlaces, imágenes, tablas, notas, fórmulas, diagramas, escape): Sí
- Abrir y renderizar archivos Markdown con sanitización: Sí
- Confirmar enlaces externos antes de resolverlos: Sí (NFR-017)
- Ejecutar código arbitrario desde el Markdown: No
- Resolver enlaces peligrosos (javascript:, data:, file:) sin confirmación: No
- Cargar imágenes desde red o URLs remotas: No
- Acceder a red, telemetría o servicios externos: No (BR-012, NFR-004/005)
- Abrir más de un documento o una segunda instancia: No (BR-001)

## Permisos requeridos

- Lectura del archivo `.md` elegido por el usuario, con permisos del SO; sin elevación.
- Lectura de imágenes solo desde rutas locales accesibles por el usuario (sin red).
- Ejecución local de renderizadores (CommonMark, KaTeX/MathJax, Mermaid) en sandbox sin eval ni acceso a red.
- Sin permisos de red, registro del sistema, cámara, micrófono ni acceso a directorios no elegidos.

## Restricciones de acceso

- Una única instancia y un único documento por sesión (BR-001); el formato actúa sobre el panel activo (BR-016).
- Al abrir, el contenido se trata como texto: no se ejecuta código ni se resuelven enlaces peligrosos; se neutralizan o requieren confirmación (NFR-017, ERR-020).
- Los enlaces externos requieren confirmación explícita del usuario antes de abrirse (NFR-017).
- Las fórmulas (LaTeX) y los diagramas (Mermaid) se renderizan en sandbox sin eval; un fallo deja marcador sin alterar el Markdown (BR-011, ERR-019).
- Las imágenes se cargan solo desde rutas locales; no se descargan recursos remotos ni se ejecutan scripts incrustados.
- Sin elevación de privilegios ni operaciones que requieran administrador.

## Restricciones sobre datos (ámbito)

- Ningún dato sale del equipo: sin telemetría, actualizaciones automáticas ni servicios en la nube (BR-012, NFR-005).
- El Markdown y sus recursos se procesan y almacenan solo en local; nada se envía a terceros.
- Registros técnicos solo locales, sin contenido, nombre ni ruta completa del archivo (NFR-010).
- Las licencias de renderizadores deben ser MIT-compatibles (NFR-011).

## Condiciones especiales de autorización

- Ninguna: no hay autenticación, autorización por rol ni credenciales.
- La lectura/escritura y la resolución de enlaces dependen de los permisos del SO y de la confirmación del usuario; un fallo se trata como ERR-020 (sanitización) o ERR-007 (permisos).

## Separación de responsabilidades

- UI (SC-001): presentación, entrada, foco, confirmación de enlaces externos; sin lógica de sanitización.
- Motor de formato/parser: tratamiento del contenido como texto, neutralización de enlaces peligrosos, renderizado en sandbox de fórmulas/diagramas; desacoplado de Avalonia (NFR-007).
- Adaptador de plataforma: selector de archivos, resolución de rutas locales e imágenes; el núcleo no conoce el SO.
- Mecanismos técnicos de autenticación/autorización: fuera de alcance; si se requirieran, se definen en Architecture.