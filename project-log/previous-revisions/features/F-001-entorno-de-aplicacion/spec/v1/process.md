# process

## Inicio

Acceso:
1. El usuario lanza la aplicación desde el escritorio o el gestor de archivos del sistema operativo.
2. El sistema abre la única ventana con un documento vacío (BR-003) y carga la configuración de la interfaz: idioma español o inglés (BR-007).
3. Inicializa menús, barra de formato y atajos de teclado, y presenta la pantalla SC-001 operativa.

Condiciones de acceso: sin autenticación; aplicación local que no requiere red (NFR-002).
Datos requeridos: ninguno.
Precondiciones: sistema operativo soportado para v1 (NFR-005); ninguna entrada previa del usuario.

## Flujo principal

1. El usuario ejecuta el acceso descrito en Inicio.
2. El sistema presenta SC-001 con menús, barra de formato y atajos disponibles desde el primer momento.
3. El usuario invoca una función del entorno: decide entre el menú o su atajo de teclado equivalente; ambos caminos ejecutan el mismo comando.
4. El sistema responde en la ventana única, que se mantiene durante toda la sesión con un único documento abierto (BR-003), y el entorno permanece operativo hasta el cierre de la aplicación.

## Flujos alternativos y de excepción

- **Cambio de idioma es ↔ en:** el usuario selecciona el otro idioma en la interfaz (FR-010, BR-007); el sistema relocaliza en caliente todos los textos visibles —menús, barra de formato y ventanas abiertas— sin reiniciar ni recargar SC-001, conservando el estado del documento. La localización se apoya en la arquitectura i18n extensible (NFR-006).
- **Uso exclusivo de teclado:** el usuario presiona un atajo; el sistema ejecuta el comando equivalente del menú con idéntico resultado que su selección desde el menú.
- **Excepción de inicialización:** si el entorno no termina de inicializarse, la aplicación no queda operativa y no hay resultado observable; el comportamiento de error se detalla en `error-handling.md`.

## Resultado

Resultado: la aplicación queda en una sola ventana plenamente operativa —menús, barra de formato y atajos activos— con la interfaz en el idioma elegido y el documento listo para las demás Features de la entrega.

Finalización: la sesión termina cuando el usuario cierra la ventana; el guardado y el aviso de cambios sin guardar quedan fuera de este proceso (FR-004).
