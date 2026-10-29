# Security-permissions — F-003 Doble vista sincronizada

## Roles y operaciones

Rol: Usuario local (identidad = cuenta del SO que ejecuta la aplicación; sin autenticación ni cuentas propias).

- Editar en el panel WYSIWYG: Sí
- Editar en el panel Markdown: Sí
- Sincronizar cambios entre paneles: Sí (automático)
- Mostrar/ocultar panel izquierdo/derecho: Sí
- Mostrar/ocultar barra de formato: Sí
- Redimensionar el separador: Sí
- Cambiar panel activo: Sí
- Acceder a red, telemetría o servicios externos: No (BR-012, NFR-004/005)
- Abrir más de un documento o una segunda instancia: No (BR-001)

## Permisos requeridos

- Lectura/escritura sobre el documento y las preferencias locales (visibilidad, ancho del separador) en el directorio de configuración del usuario (A-008, FR-013).
- Sin permisos de red, registro del sistema, cámara, micrófono ni acceso a directorios no elegidos.

## Restricciones de acceso

- Una única instancia y un único documento por sesión (BR-001); la sincronización no abre otra ventana.
- Al menos un panel permanece visible en todo momento (BR-F003-007).
- El resaltado pasajero no modifica el contenido ni requiere confirmación (BR-014).

## Restricciones sobre datos (ámbito)

- Ningún dato sale del equipo: sin telemetría ni servicios en la nube (BR-012, NFR-005).
- Las preferencias de visibilidad son locales; no contienen contenido del documento.
- Registros técnicos solo locales, sin contenido, nombre ni ruta completa del archivo (NFR-010).

## Condiciones especiales de autorización

- Ninguna: no hay autenticación, autorización por rol ni credenciales.

## Separación de responsabilidades

- UI (SC-001): presentación, entrada, foco y visibilidad; sin lógica de sincronización.
- Motor de sincronización: modelo único, árbol sintáctico, mapa de posiciones, resaltado; desacoplado de Avalonia (NFR-007).
- Adaptador de plataforma: renderizado WYSIWYG, desplazamiento y accesibilidad; el núcleo no conoce el SO.