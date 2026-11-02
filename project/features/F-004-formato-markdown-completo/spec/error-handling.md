# Error Handling — F-004 Formato Markdown completo

Mensajes en el idioma activo (es/en); se muestra el texto base en español.

```
| id | condición | comportamiento | mensaje | reintento/rollback | log |
|---|---|---|---|---|---|
| ERR-020 | Sanitización bloqueada al abrir Markdown (enlace/código peligroso, NFR-017) | El contenido se trata como texto; el enlace peligroso se neutraliza o requiere confirmación; no se ejecuta código | «Se ha neutralizado contenido potencialmente peligroso al abrir el documento.» | Abrir de todos modos como texto / cancelar; sin rollback | Requerido. Referencia: LOG-014 |
| ERR-021 | Tabla fuera de límites al insertar o editar (más de 1 000 filas o 50 columnas, NFR-016) | Se informa y se degrada la inserción sin bloquear la edición; el documento no se pierde | «La tabla supera el límite (1 000 filas × 50 columnas); se ha limitado la inserción.» | Ajustar dimensiones y reintentar; sin rollback | Requerido. Referencia: LOG-015 |
| ERR-022 | Error de parseo CommonMark/GFM (el parser no construye el árbol sintáctico) | El error se muestra y el documento sigue editable en el panel Markdown; el contenido no se pierde | «No se pudo analizar el documento como Markdown; revisa la sintaxis.» | Corregir la sintaxis y reintentar; sin rollback | Requerido. Referencia: LOG-012 |
| ERR-023 | Enlace externo no confirmado por el usuario | No se abre el enlace; el documento y el foco permanecen intactos | Ninguno visible; la acción se cancela | Volver a activar el enlace y confirmar | No requerido |
| ERR-024 | Imagen local no encontrada o no legible | Se muestra un marcador de imagen rota; el Markdown no se altera | «No se pudo cargar la imagen «<ruta>».» | Corregir la ruta o elegir otra imagen | No requerido |
```

No requerido / Fuera de alcance: el renderizado normal y la cancelación de diálogos no generan error; los fallos de E/S de archivo son de F-002; la sincronización entre vistas es F-003; la degradación por documento grande y el error de renderizado de fórmula/diagrama reutilizan errores de F-003; los fallos de arranque o cierre son F-001.

Referencias:
- ERR-001–ERR-005
- ERR-006–ERR-015
- ERR-016
- ERR-017
- ERR-019
- ERR-020
- ERR-021
- ERR-022
- ERR-023
- ERR-024
- LOG-012
- LOG-013
- LOG-014
- LOG-015
- BR-009
- BR-011
- FR-002
- NFR-003
- NFR-013
- NFR-016
- NFR-017