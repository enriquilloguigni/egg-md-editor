# Security-permissions — F-002 Gestión de archivo

## Roles y operaciones

Rol: Usuario local (identidad = cuenta del sistema operativo que ejecuta la aplicación; sin autenticación ni cuentas propias).

- Crear documento nuevo: Sí
- Abrir `.md` por selector, reciente o arrastre: Sí
- Guardar y Guardar como: Sí
- Consultar la lista de recientes: Sí
- Autoguardar y recuperar: Sí
- Confirmar o cancelar el descarte: Sí
- Acceder a red, telemetría o servicios externos: No (BR-012, NFR-004/005)
- Elevar privilegios o ejecutar como administrador: No
- Abrir más de un documento o una segunda instancia: No (BR-001)
- Leer/escribir fuera de las rutas elegidas y del estado local: No

## Permisos requeridos

- Lectura y escritura sobre los archivos `.md` que el usuario elige, con los permisos del SO; sin elevación.
- Lectura/escritura del almacén local de recientes y de la copia de recuperación, en el directorio de configuración/datos del usuario (A-008, FR-013).
- Sin permisos de red, registro del sistema, cámara, micrófono ni acceso a directorios no elegidos.

## Restricciones de acceso

- Una única instancia y un único documento por sesión (BR-001); abrir o crear reemplaza, no abre otra ventana.
- El acceso a archivos se limita a la ruta elegida en el selector, arrastrada o seleccionada de recientes; no hay exploración automática de carpetas.
- Un archivo de solo lectura o sin permiso de escritura no se sobrescribe; se ofrece Guardar como.
- La aplicación no solicita credenciales ni usa cuentas propias.

## Restricciones sobre datos (ámbito)

- Ningún dato sale del equipo: sin telemetría, actualizaciones automáticas ni servicios en la nube (BR-012, NFR-005).
- Los archivos `.md` se escriben en UTF-8 con finales del SO (BR-007); el contenido y el bloque de metadatos no se envían a terceros.
- Recientes y copia de recuperación son locales; la copia contiene contenido del documento y se invalida al guardar o descartar (BR-F002-015).
- Registros técnicos solo locales, sin contenido, nombre ni ruta completa del archivo; sin datos personales ni envío remoto (NFR-010).
- La lista de recientes contiene rutas absolutas; se conserva solo en local y se limita a 10 entradas (BR-008, FR-013).

## Condiciones especiales de autorización

- Ninguna: no hay autenticación, autorización por rol ni credenciales.
- La lectura/escritura depende exclusivamente de los permisos del SO sobre la ruta elegida; un fallo se trata como ERR-007 o ERR-009.
- El usuario puede cancelar cualquier diálogo sin efecto sobre el documento ni los permisos.

## Separación de responsabilidades

- UI (`SC-001`, `SC-002`): presentación, entrada y confirmación; sin lógica de archivo.
- Coordinador/servicio de archivo: validación, codificación, normalización, recientes y recuperación; desacoplado de Avalonia (NFR-007).
- Adaptador de plataforma/SO: selector nativo, rutas y permisos; el núcleo no conoce el SO concreto (Architecture).
