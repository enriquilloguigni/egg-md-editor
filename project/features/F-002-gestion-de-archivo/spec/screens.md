# Screens — F-002 Gestión de archivo

F-002 trabaja sobre la ventana única `SC-001` de F-001, referenciada sin redefinición; la única
superficie nueva es el aviso modal `SC-002`. Los diálogos nativos de archivos del sistema operativo
se invocan tal cual y no se diseñan en este documento.

## SC-001 — Ventana principal (reutilizada de F-001)

Propósito en F-002: hospedar el documento de trabajo único (BR-003) y servir de superficie a las
operaciones de archivo; su definición completa está en F-001
(`project/features/F-001-entorno-de-aplicacion/spec/screens.md`).

Elementos que F-002 añade sobre SC-001:

```
Área de documento — muestra el contenido del documento de trabajo; vacía tras crear un documento nuevo.
Destino de arrastre — toda la ventana acepta soltar archivos .md (FR-004); lo que no es .md se ignora.
Sublista Recientes — despliegue de la entrada Recientes del menú Archivo: hasta 10 archivos con su
  ruta, ordenados por uso (FR-004); cada entrada abre su archivo con un clic.
Diálogos nativos del SO — Abrir y Guardar como: elección de ruta y nombre .md del documento (BR-004).
```

Campos: sin campos de formulario; las entradas de Recientes muestran la ruta del archivo (solo lectura).

Acciones disponibles en F-002: invocar Nuevo, Abrir, Guardar y Guardar como por atajo; soltar un
archivo .md sobre la ventana; pulsar una entrada de Recientes. La conexión de las entradas del menú
Archivo llega con F-028 (ADR-001).

Mensajes relevantes: rótulos de Recientes y de los diálogos del sistema en el idioma activo (BR-007);
los avisos de fallo de escritura y de archivo ilegible se definen en error-handling.md.

Comportamiento dinámico: al abrir, el área de documento pasa a mostrar el archivo cargado; la lista
de recientes se reordena por uso y registra cada guardar como (FR-004); el autoguardado no produce
ningún cambio visible en SC-001 (PD-FEAT-004).

## SC-002 — Aviso de cambios sin guardar (nueva)

Propósito: avisar de cambios sin guardar antes de crear un documento nuevo, abrir otro archivo o
cerrar la aplicación, y resolver la operación con una de tres opciones (PD-FEAT-003).

Elementos de la interfaz:

```
Diálogo modal sobre SC-001 — bloquea la operación en curso hasta que se resuelve.
Título — «Cambios sin guardar» (i18n).
Mensaje — nombra la operación en curso y pregunta qué hacer con los cambios (i18n).
Botón «Guardar y continuar» — opción por defecto.
Botón «Continuar sin guardar».
Botón «Cancelar».
```

Campos: sin campos de formulario; solo los tres botones.

Acciones disponibles y efecto:
- Guardar y continuar: aplica Guardar (o Guardar como si el documento no tiene ruta) y continúa la operación.
- Continuar sin guardar: descarta el aviso y continúa la operación sin persistir los cambios.
- Cancelar: anula la operación en curso; el documento queda tal cual estaba.

Textos canónicos del catálogo i18n es/en (BR-007):

```
titulo       Cambios sin guardar                           Unsaved changes
mensaje      ¿Guardar los cambios antes de {operación}?    Save changes before {operation}?
guardar      Guardar y continuar                           Save and continue
continuar    Continuar sin guardar                         Continue without saving
cancelar     Cancelar                                      Cancel
{operación}  crear un documento nuevo · abrir otro archivo · cerrar la aplicación
             create a new document · open another file · close the application
```

Comportamiento dinámico: aparece solo si hay cambios sin guardar y en los tres momentos declarados
(PD-FEAT-003); el mensaje se adapta a la operación en curso; resuelto, desaparece y la operación
continúa o queda cancelada; sus textos siguen el idioma activo con relabelizado en caliente (BR-007).
El respaldo temporal del autoguardado complementa este aviso, no lo sustituye (PD-FEAT-004).
