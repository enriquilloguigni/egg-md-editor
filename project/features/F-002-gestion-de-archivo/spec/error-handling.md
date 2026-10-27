# Error Handling — F-002 Gestión de archivo

Mensajes en el idioma activo (es/en); se muestra el texto base en español.

```
| id | condición | comportamiento | mensaje | reintento/rollback | log |
|---|---|---|---|---|---|
| ERR-006 | Abrir un `.md` inexistente o una ruta no disponible (selector, reciente o arrastre) | No se abre ni se altera el documento; la entrada reciente se elimina; el foco vuelve a SC-001 | «No se pudo abrir «<nombre>»: el archivo no existe o la ruta no está disponible.» | Reintentar/Elegir otro/Cancelar; sin rollback necesario | Requerido. Referencia: LOG-004 |
| ERR-007 | Permiso denegado o archivo no legible al abrir | No se abre; el documento actual permanece; no se modifica el archivo | «No se pudo abrir «<nombre>»: no tienes permiso de lectura o el archivo no es legible.» | Reintentar/Elegir otro/Cancelar | Requerido. Referencia: LOG-004 |
| ERR-008 | El archivo no decodifica como UTF-8 válido (incluye UTF-16) | Se rechaza la apertura; no se convierte ni se escribe; el documento actual no cambia | «No se pudo abrir «<nombre>»: el archivo no está en UTF-8.» | Elegir otro/Cancelar; no hay reintento útil | Requerido. Referencia: LOG-004 |
| ERR-009 | Guardar sobre archivo de solo lectura, sin permiso de escritura o carpeta no escribible | No se escribe; el documento sigue con cambios y la recuperación se conserva | «No se pudo guardar «<nombre>»: no tienes permiso de escritura o el archivo es de solo lectura.» | Reintentar/Guardar como/Cancelar | Requerido. Referencia: LOG-005 |
| ERR-010 | Fallo de escritura por disco lleno, E/S o interrupción | El archivo original no queda a medias; el documento sigue con cambios y la recuperación se conserva | «No se pudo guardar «<nombre>»: <causa>. El documento no se ha modificado.» | Reintentar/Guardar como/Cancelar | Requerido. Referencia: LOG-005 |
| ERR-011 | Nombre o ruta inválidos en Guardar como (vacío, extensión distinta de `.md`, caracteres no válidos) | Validación inline en SC-002; no se escribe ni se cierra el diálogo | «Escribe un nombre válido con extensión .md.» | Corregir el nombre/Cancelar; sin rollback | No requerido |
| ERR-012 | El autoguardado no puede escribir la copia de recuperación | No se interrumpe la edición; se avisa de forma no modal; se reintenta en el siguiente ciclo | «No se pudo guardar la copia de recuperación. Guarda el documento para no perder cambios.» | Reintento automático a los 30 s; Guardar manual disponible | Requerido. Referencia: LOG-006 |
| ERR-013 | La copia de recuperación es ilegible o corrupta al iniciar | Se inicia un documento vacío; no se toca el archivo original; la copia se marca obsoleta | «No se pudo recuperar el último estado. Se ha abierto un documento vacío; tu archivo original no se ha modificado.» | Aceptar; abrir el archivo original; sin rollback | Requerido. Referencia: LOG-007 |
| ERR-014 | Arrastre sin `.md`, con varios archivos o con ruta no accesible | Se rechaza la operación completa; ningún archivo se abre; el documento actual no cambia | «Suelta un único archivo .md.» | Repetir con un `.md` válido/Cancelar | Requerido. Referencia: LOG-004 |
| ERR-015 | No se puede persistir/leer la lista de recientes o invalidar la recuperación | La operación principal continúa; la lista queda en memoria o la copia se marca obsoleta; sin mensaje modal | Ninguno visible; se registra | Reintento en la siguiente operación; sin rollback | Requerido. Referencia: LOG-004/LOG-005/LOG-008 |
```

No requerido / Fuera de alcance: cancelar un diálogo, cerrar sin cambios, Guardar sobre un documento limpio y las validaciones de la propia UI no generan error; los fallos de arranque o cierre de la ventana son de F-001 (ERR-001–ERR-005).
