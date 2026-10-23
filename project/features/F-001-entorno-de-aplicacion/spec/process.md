# Process

## Inicio

- **Proceso de acceso:** el usuario compila y ejecuta la aplicación desde el código fuente en su sistema operativo (A-001, C-011). El arranque abre directamente la ventana única `SC-001`, sin pantalla intermedia, cuentas ni autenticación.
- **Condiciones de acceso:** ninguna — aplicación local 100 % offline (BR-012, NFR-004), sin autenticación ni registro; solo se requiere poder ejecutar la build local de .NET 10 + Avalonia.
- **Datos requeridos:** ninguno obligatorio. Entrada opcional del usuario: elección del idioma de UI, español o inglés (BR-013), válida mientras dure la sesión.
- **Precondiciones:** aplicación compilable conforme a Architecture; una sola ventana con un único documento abierto a la vez (BR-001, A-002); i18n de la UI activa con base extensible a más idiomas (A-005). La persistencia de la elección de idioma entre sesiones queda fuera de F-001 (FR-013 → F-010).

## Flujo principal

1. El usuario lanza la aplicación: el sistema crea la ventana única `SC-001` (BR-001, A-002) y presenta la interfaz en el idioma activo, español o inglés (BR-013).
2. `SC-001` muestra la parte visual del menú principal persistente (Archivo, Edición, Ver, Formato, Herramientas, Ayuda) y de la barra de formato con el 100 % de sus opciones visibles (FR-011, FR-012 solo en su parte visual).
3. **Decisión — idioma:** el usuario alterna es/en desde la entrada de idioma del menú → la interfaz se relabeliza en caliente, sin reinicio y conservando ventana y estado; el flujo continúa en el paso 2.
4. **Decisión — opciones:** el usuario abre una entrada del menú o pulsa un control de la barra de formato → en F-001 la acción aún no está conectada (la conexión la realizan las Features que poseen cada capacidad: F-002, F-004, F-005, F-009, F-010): el menú abre y cierra, el control muestra su aviso flotante, y no hay efecto sobre el documento.
5. El usuario invoca atajos de teclado: responden los correspondientes a acciones disponibles en F-001; el resto permanece inerte hasta la Feature que conecte su acción.
6. El usuario cierra la ventana `SC-001` → finalización (ver Resultado).

## Flujos alternativos y de excepción

- **Alternativo — cambio de idioma en cualquier punto:** desde cualquier paso, alternar es/en → relabelizado en caliente y vuelta al paso correspondiente del flujo; el cambio dura hasta el cierre de la sesión.
- **Excepción — opciones visuales sin acción:** pulsar una opción de menú o un control de la barra sin acción conectada → sin efecto funcional sobre el documento; se conserva el estado actual.
- **Excepción — atajo sin acción disponible:** pulsar un atajo cuya acción aún no existe en F-001 → sin efecto observable.
- **Excepción — segunda ventana solicitada mientras la aplicación ya corre:** no se crea otra ventana; la ventana existente `SC-001` recibe el foco (BR-001, A-002).
- **Excepción — fallo de arranque o de pintado de `SC-001`:** su manejo y mensajes viven en `error-handling.md`, sin efectos colaterales sobre este flujo.

## Resultado observable y finalización

- **Resultado:** una única ventana `SC-001` operativa con el menú principal y la barra de formato visibles al 100 % de sus opciones, interfaz en español o inglés conmutable en caliente y atajos activos; las acciones de menú y barra siguen desconectadas hasta las Features que las conectan (F-002, F-004, F-005, F-009, F-010).
- **Finalización:** el usuario cierra `SC-001` y la aplicación termina sin dejar procesos residuales; el idioma elegido no se persiste entre sesiones en F-001 (FR-013 → F-010). El guardado y el aviso de cambios sin guardar quedan fuera de este proceso (FR-004).
