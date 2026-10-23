# Security-permissions — F-001 Entorno de aplicación

## Roles y operaciones

Rol: Usuario local (identidad = cuenta del sistema operativo que ejecuta la aplicación; sin autenticación ni cuentas propias).

- Lanzar la aplicación y abrir `SC-001`: Sí
- Conmutar idioma es/en en caliente: Sí
- Conmutar tema claro/oscuro: Sí
- Abrir/cerrar entradas del menú: Sí
- Mostrar/ocultar aviso flotante: Sí
- Invocar atajos disponibles en F-001: Sí
- Cerrar la ventana: Sí
- Crear/abrir/guardar archivos de documento: No (F-004)
- Persistir preferencias entre sesiones: No (F-010, FR-013)
- Acceder a red o servicios externos: No (BR-012, NFR-004, NFR-005)

## Permisos requeridos

- Ejecución local de la build (.NET 10 + Avalonia) con los permisos del usuario del SO; sin elevación ni administrador.
- Lectura/escritura solo sobre el directorio de configuración del usuario para preferencias y diccionario personal (A-008), reservado; en F-001 dichos datos viven solo en memoria.
- Sin permisos de red, registro del sistema ni acceso a archivos fuera del alcance del usuario.

## Restricciones de acceso

- Una única instancia y ventana por usuario/sesión; una segunda solicitud de arranque enfoca la existente, no crea otra (BR-001, A-002, COM-005).
- Sin multi-usuario, sesiones concurrentes ni control de acceso por rol más allá de la identidad del SO.
- Sin elevación de privilegios ni operaciones que requieran administrador.

## Restricciones sobre datos (ámbito)

- Ningún dato sale del equipo: sin telemetría, actualizaciones automáticas ni servicios en la nube (BR-012, NFR-005).
- En F-001 no se leen ni escriben archivos de documento; el acceso a archivos se rige por los permisos del SO y se conecta en F-004.
- Preferencias (idioma, tema) y estado de menú son efímeros en F-001 y no se persisten (FR-013 → F-010).
- Registros técnicos: solo locales, con rotación y niveles configurables, sin datos personales ni envío remoto (NFR-010).

## Condiciones especiales de autorización

- Ninguna: la aplicación no autentica, no autoriza por rol y no solicita credenciales.
- El idioma de UI se elige en sesión y no requiere permiso especial (BR-013).

## Separación de responsabilidades

- UI (menús, barra, diálogos): presentación y entrada, sin lógica de dominio; el núcleo no conoce SO, rutas ni APIs concretas (puerto/adaptador, Architecture).
- Mecanismos técnicos de autenticación/autorización (JWT, OAuth, claims, middleware, policies): fuera de alcance; si se requirieran en el futuro, se definen en Architecture.
