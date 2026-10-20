# Security permissions — F-002 Gestión de archivo

## Roles y operaciones permitidas

Un único perfil: el usuario local que compila y ejecuta la aplicación en su propio equipo; sin cuentas, roles diferenciados ni autenticación — producto de escritorio monousuario y 100 % local (BR-001, C-001; process.md, Inicio).

| Operación | Crear documento nuevo | Abrir .md (diálogo, arrastre, recientes) | Guardar / Guardar como | Resolver aviso `SC-002` |
|---|---|---|---|---|
| Usuario local | Sí | Sí | Sí | Sí |

El autoguardado silencioso no es una operación del usuario: la aplicación la ejecuta por sí misma mientras haya cambios sin guardar (PD-FEAT-004).

## Permisos requeridos

Del sistema operativo, ejercidos bajo la cuenta local del usuario, sin elevación de privilegios:

- Lectura de archivos .md del disco local para abrir por diálogo, arrastre o recientes (FR-004, BR-004).
- Escritura en la ruta que el usuario elige al guardar o guardar como, en UTF-8 (NFR-004).
- Crear, sobrescribir y eliminar el fichero de respaldo en el directorio de temporales del SO, y nada más en él (PD-FEAT-004, BR-001).

El acceso queda limitado a los archivos que el propio usuario elige o suelta sobre la ventana. Un permiso denegado por el SO se manifiesta como fallo de lectura o de escritura y se trata en error-handling.md.

## Restricciones de acceso

No requerido / Fuera de alcance — sin control de acceso ni credenciales: poder utilizar la Feature equivale a poder lanzar la aplicación en el equipo local (BR-001, C-001).

## Restricciones sobre datos (ámbito)

- Lectura: solo archivos .md del disco local elegidos en diálogo, arrastrados sobre `SC-001` o pulsados en recientes (BR-004, FR-004); lo que no es .md no entra por arrastre.
- Escritura: únicamente el archivo asignado al documento y el fichero temporal de respaldo (NFR-004, PD-FEAT-004); el registro de recientes solo lo actualiza la aplicación (FR-004).
- La Feature no elimina archivos del usuario: el único borrado es el fichero temporal al finalizar su ciclo (PD-FEAT-004).
- Ningún dato del documento abandona el equipo: sin red ni terceros (BR-001, C-001).

## Condiciones especiales de autorización

No requerido / Fuera de alcance — no hay operaciones sensibles ni privilegiadas: las acciones operan sobre datos del propio usuario y los diálogos nativos de archivos no requieren autorización adicional (referencia: external-services.md).

## Separación de responsabilidades

No requerido / Fuera de alcance — un único perfil sin privilegios diferenciados.
