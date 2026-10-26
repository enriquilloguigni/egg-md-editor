# Process

## Inicio

- **Proceso de acceso:** el usuario compila y ejecuta la aplicación desde el código fuente en su sistema operativo (A-001, C-011). El arranque abre directamente la ventana única `SC-001`, sin pantalla intermedia, cuentas ni autenticación.
- **Condiciones de acceso:** ninguna — aplicación local 100 % offline (BR-012, NFR-004), sin autenticación ni registro; solo se requiere poder ejecutar la build local de .NET 10 + Avalonia.
- **Datos requeridos:** ninguno obligatorio. Entrada opcional: idioma de UI (español o inglés, BR-013) y tema (Claro/Oscuro/Sistema); el tema inicial se hereda del SO y ambas elecciones duran la sesión.
- **Precondiciones:** aplicación compilable conforme a Architecture; una sola ventana con un único documento abierto a la vez (BR-001, A-002); i18n de la UI activa con base extensible (A-005). La persistencia de idioma y tema entre sesiones queda fuera de F-001 (FR-013 → F-010).

## Flujo principal

1. El usuario lanza la aplicación: el sistema crea la ventana única `SC-001` (BR-001, A-002) y presenta la interfaz en el idioma activo (BR-013) y con el tema resuelto del SO (Claro u Oscuro).
2. `SC-001` muestra los seis rótulos del menú como botones —Archivo, Edición, Ver, Formato, Herramientas, Ayuda—, dos botones de navegación secuencial y la barra de formato con los doce controles de FR-012 en orden de escritura, con icono y aviso flotante; el área de documento queda vacía (FR-011, FR-012; ADR-002).
3. **Decisión — idioma:** el usuario alterna es/en desde Herramientas → Idioma → la interfaz se relabeliza en caliente, sin reinicio y conservando ventana y estado; el flujo continúa en el paso 2.
4. **Decisión — tema:** el usuario abre Herramientas → Tema y elige Claro, Oscuro o Sistema → el tema se aplica en caliente; Sistema resuelve al tema del SO; la elección no persiste y el estado oscuro queda observable para captura (OP-002, TR-008/TR-009).
5. **Decisión — navegación secuencial:** el usuario pulsa el botón «Siguiente» o «Anterior» → el rótulo seleccionado avanza o retrocede cíclicamente entre los seis (Archivo → Edición → … → Ayuda → Archivo, y a la inversa); la selección no abre el menú ni altera el documento (OP-007, TR-010/TR-011).
6. **Decisión — opciones:** el usuario activa el rótulo seleccionado o pulsa un control de la barra → «opciones» designa los seis rótulos, no entradas internas; en F-001 la acción aún no está conectada (la conexión la realizan F-002, F-004, F-005, F-009 y F-010): el rótulo abre y cierra, el control muestra su aviso y no hay efecto sobre el documento.
7. El usuario invoca atajos de teclado: responden los disponibles en F-001; el resto permanece inerte hasta la Feature que conecte su acción.
8. El usuario cierra la ventana `SC-001` → finalización (ver Resultado).

## Flujos alternativos y de excepción

- **Alternativo — cambio de idioma o tema en cualquier punto:** desde cualquier paso, alternar es/en o elegir Claro/Oscuro/Sistema → cambio en caliente y vuelta al paso correspondiente; dura hasta el cierre de la sesión.
- **Alternativo — navegación en los extremos:** pulsar «Siguiente» en Ayuda lleva a Archivo; pulsar «Anterior» en Archivo lleva a Ayuda; no hay tope ni error (OP-007).
- **Excepción — opciones visuales sin acción:** activar el rótulo seleccionado o un control de la barra sin acción conectada → sin efecto funcional sobre el documento; se conserva el estado actual.
- **Excepción — atajo sin acción disponible:** pulsar un atajo cuya acción aún no existe en F-001 → sin efecto observable.
- **Excepción — segunda ventana solicitada mientras la aplicación ya corre:** no se crea otra ventana; la ventana existente `SC-001` recibe el foco (BR-001, A-002).
- **Excepción — fallo de arranque o de pintado de `SC-001`:** su manejo y mensajes viven en `error-handling.md`, sin efectos colaterales sobre este flujo.

## Resultado observable y finalización

- **Resultado:** una única ventana `SC-001` operativa con los seis rótulos del menú como botones, navegación secuencial, la barra de formato con los doce controles, área de documento vacía, interfaz es/en y tema Claro/Oscuro/Sistema conmutables en caliente; las acciones de menú y barra siguen desconectadas hasta las Features que las conectan.
- **Finalización:** el usuario cierra `SC-001` y la aplicación termina sin procesos residuales; idioma y tema no se persisten entre sesiones en F-001 (FR-013 → F-010). El guardado y el aviso de cambios sin guardar quedan fuera de este proceso (FR-004).
