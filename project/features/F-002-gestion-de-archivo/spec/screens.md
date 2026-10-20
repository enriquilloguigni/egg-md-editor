# Screens — F-002 Gestión de archivo

F-002 trabaja sobre la ventana única `SC-001` de F-001, referenciada sin redefinición; la única
superficie nueva es el aviso modal `SC-002`. Los diálogos nativos de archivos del sistema operativo
se invocan tal cual y no se diseñan en este documento.

## SC-001 — Ventana principal (reutilizada de F-001)

Propósito en F-002: hospedar el documento de trabajo único (BR-003) y servir de superficie a las operaciones de
archivo; su definición completa está en F-001 (`project/features/F-001-entorno-de-aplicacion/spec/screens.md`).

Elementos que F-002 añade sobre SC-001:

```
Área de documento — muestra el contenido del documento de trabajo; vacía tras crear un documento nuevo.
Destino de arrastre — toda la ventana acepta soltar archivos .md (FR-004); lo que no es .md se ignora.
Sublista Recientes — despliegue de la entrada Recientes del menú Archivo: hasta 10 archivos con su
  ruta, ordenados por uso (FR-004); cada entrada abre su archivo con un clic.
Diálogos nativos del SO — Abrir y Guardar como: elección de ruta y nombre .md del documento (BR-004).
```

Campos: sin campos de formulario; las entradas de Recientes muestran la ruta del archivo (solo lectura).

Acciones disponibles en F-002: invocar Nuevo, Abrir, Guardar y Guardar como por atajo; soltar un archivo .md sobre
la ventana; pulsar una entrada de Recientes. La conexión de las entradas del menú Archivo llega con F-028 (ADR-001).

Mensajes relevantes: rótulos de Recientes y de los diálogos del sistema en el idioma activo (BR-007);
los avisos de fallo de escritura y de archivo ilegible se definen en error-handling.md.

Comportamiento dinámico: al abrir, el área de documento pasa a mostrar el archivo cargado; la lista de recientes
se reordena por uso y registra cada guardar como (FR-004); el autoguardado no produce ningún cambio visible (PD-FEAT-004).

Estado de drag-over de SC-001 (PD-FEAT-005): mientras se arrastra un archivo .md válido sobre la ventana, esta
se resalta como destino de soltado (borde y estado de destino) — la indicación visual del destino que process.md
declara para el arrastre sobre SC-001 — y vuelve a su estado normal al salir de la ventana o al soltar. Si el
arrastre no puede distinguirse o no arrastra un .md, la ventana permanece en su estado normal y el soltado de
contenido no .md se ignora tal como está especificado (FR-004, security-permissions.md); el recurso visual del
resaltado queda pendiente de visual-resources.md.

## SC-002 — Aviso modal de operación (nueva)

Propósito: resolver modalmente la operación en curso con un aviso parametrizado por ocasión — avisar de cambios
sin guardar (PD-FEAT-003) o ofrecer la recuperación del respaldo al arrancar (PD-FEAT-006, BR-F002-005).

Elementos de la interfaz:

```
Diálogo modal sobre SC-001 — bloquea la operación en curso hasta que se resuelve.
Aviso parametrizado por ocasión — patrón común de título, mensaje y acciones (i18n es/en con relabelizado en caliente, BR-007).
Título — según la ocasión; los valores canónicos están en el bloque de textos.
Mensaje — según la ocasión: nombra la operación en curso y pregunta qué hacer con los cambios, o anuncia el
  respaldo encontrado al arrancar y pregunta qué hacer con él.
Acciones — botones según la ocasión, sin campos de formulario:
  cambios sin guardar (PD-FEAT-003): «Guardar y continuar» (por defecto), «Continuar sin guardar», «Cancelar».
  ofrecimiento de recuperación (PD-FEAT-006): «Recuperar como documento de trabajo» (por defecto),
  «Eliminar respaldo», «Ahora no» (sin elección).
```

Campos: sin campos de formulario; solo los botones de la ocasión activa.

Acciones disponibles y efecto:
- Guardar y continuar: aplica Guardar (o Guardar como si el documento no tiene ruta) y continúa la operación.
- Continuar sin guardar: descarta el aviso y continúa la operación sin persistir los cambios.
- Cancelar: anula la operación en curso; el documento queda tal cual estaba.
- Recuperar como documento de trabajo (ofrecimiento): abre el contenido respaldado como documento único sin
  ruta, en «Con cambios sin guardar» (process.md), y entra en el ciclo de apertura y autoguardado (PD-FEAT-004).
- Eliminar respaldo (ofrecimiento): descarta ese fichero temporal; terminado el ofrecimiento de todos los huérfanos, el arranque continúa normal.
- Ahora no (ofrecimiento, sin elección): conserva ese fichero con permisos de BR-F002-003; su ofrecimiento se repite en cada arranque.

Textos canónicos del catálogo i18n es/en (BR-007):

```
titulo       Cambios sin guardar                           Unsaved changes
mensaje      ¿Guardar los cambios antes de {operación}?    Save changes before {operation}?
guardar      Guardar y continuar                           Save and continue
continuar    Continuar sin guardar                         Continue without saving
cancelar     Cancelar                                      Cancel
{operación}  crear un documento nuevo · abrir otro archivo · cerrar la aplicación
             create a new document · open another file · close the application

— ocasión ofrecimiento de recuperación al arranque (PD-FEAT-006, BR-F002-005) —
titulo_r     Recuperación disponible                       Recovery available
mensaje_r    Se encontró un respaldo de una sesión anterior que terminó inesperadamente. ¿Qué hacer con él?
             A backup from a previous session that ended unexpectedly was found. What to do with it?
recuperar    Recuperar como documento de trabajo           Recover as working document
eliminar     Eliminar respaldo                             Delete backup
ahora_no     Ahora no                                      Not now
```

Comportamiento dinámico: la ocasión «cambios sin guardar» aparece solo con cambios sin guardar, en los tres momentos
de PD-FEAT-003. La ocasión «ofrecimiento» aparece solo al arrancar con ficheros temporales huérfanos (PD-FEAT-006,
BR-F002-005): la detección los encuentra todos y el diálogo se presenta por cada uno, secuencialmente —diálogo
por huérfano, no lista—, con elección por fichero; sin elección («Ahora no»), cada fichero se conserva con permisos
de BR-F002-003 y su diálogo se repite en cada arranque; los fallos de lectura del huérfano viven en error-handling.md.
