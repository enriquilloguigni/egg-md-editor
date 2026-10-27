# Process

## Inicio

- **Proceso de acceso:** el usuario ya tiene abierta la ventana única `SC-001` (F-001) y accede a las operaciones desde el menú Archivo, sus atajos, la lista de recientes, el arrastre de un `.md` sobre la ventana o la recuperación automática al arrancar. No hay pantalla de acceso, autenticación ni cuentas (BR-012, NFR-004).
- **Condiciones de acceso:** aplicación local en ejecución y un único documento por instancia (BR-001); permisos de lectura/escritura del sistema operativo sobre la ruta elegida; sin elevación.
- **Datos requeridos:** para abrir, una ruta `.md` existente y legible; para guardar como, una carpeta y un nombre `.md`; para recuperar, una copia de recuperación vigente. El contenido editado y el bloque de metadatos son datos del documento.
- **Precondiciones:** `SC-001` operativa con el área de documento editable mínima de F-002 (la doble vista es F-003); lógica de archivo desacoplada de la UI (NFR-007); soporte de Windows 10+, macOS 12+ y Linux de .NET 10 (NFR-008); dependencias MIT-compatibles (NFR-011).

## Flujo principal

1. Al arrancar, el sistema comprueba si existe una copia de recuperación vigente. Si existe, restaura automáticamente contenido y ruta y marca el documento con cambios sin guardar (BR-F002-013); si no, crea un documento vacío sin ruta (BR-F002-002).
2. El usuario edita el área de documento; el título muestra el nombre y el indicador de cambios (BR-F002-016).
3. **Decisión — Nuevo/Abrir/Reciente/Arrastrar:** si hay cambios pendientes, el sistema pide confirmación en `SC-002` (Guardar / No guardar / Cancelar) antes de reemplazar el documento (BR-F002-014); si no hay cambios, la operación continúa.
4. **Abrir:** el usuario elige un `.md` en el selector nativo o lo suelta sobre la ventana; el sistema valida extensión, existencia y legibilidad, lee en UTF-8, normaliza los finales de línea a líneas lógicas y presenta el contenido (BR-F002-004, BR-F002-007/008).
5. **Guardar:** si el documento tiene ruta, escribe en ella en UTF-8 sin BOM y con finales del SO; si no tiene ruta, abre Guardar como (BR-F002-005, BR-F002-007/008).
6. **Guardar como:** el usuario elige carpeta y nombre; el sistema crea la copia, pasa a trabajar sobre ella y deja intacto el original (BR-F002-006).
7. Tras abrir o guardar con éxito, la ruta pasa al frente de la lista de recientes (máximo 10, orden descendente, sin duplicados) (BR-F002-010).
8. Mientras el documento tenga cambios, el autoguardado escribe cada 30 s una copia de recuperación local, sin diálogos; no modifica el archivo del usuario ni limpia el indicador de cambios (BR-F002-012).
9. **Cerrar:** si hay cambios pendientes, se aplica la misma confirmación; Guardar cierra tras escribir, No guardar descarta e invalida la recuperación, Cancelar mantiene la ventana (BR-F002-014/015).

## Flujos alternativos y de excepción

- **Alternativo — cancelar un diálogo:** Cancelar o Esc cierra `SC-002` sin cambio de documento, ruta, recientes ni recuperación.
- **Excepción — destino inválido:** archivo inexistente, sin permisos, no `.md`, múltiples archivos o contenido no UTF-8 → no se altera el documento actual; se informa en `SC-002` y se registra (ERR-006/007/008/014).
- **Excepción — fallo al guardar:** permiso denegado, solo lectura, disco lleno o error de E/S → el documento sigue con cambios y la recuperación se conserva; se ofrece Reintentar / Guardar como / Cancelar (ERR-009/010).
- **Excepción — fallo del autoguardado:** se conserva el estado, se avisa de forma no modal y se reintenta en el siguiente ciclo (ERR-012).
- **Excepción — recuperación ilegible:** se inicia un documento vacío sin tocar el archivo original; la copia se marca obsoleta y se registra (ERR-013).
- **Excepción — reciente no disponible:** se informa y la entrada se elimina de la lista; el documento actual no cambia (ERR-006, BR-F002-010).

## Resultado observable y finalización

- **Resultado:** el documento se crea, abre, guarda o guarda como en `.md` UTF-8 con finales del SO, conservando el bloque de metadatos; la lista de recientes refleja hasta 10 rutas; la copia de recuperación permite restaurar el último estado tras un cierre inesperado; ningún cambio se descarta sin confirmación.
- **Finalización:** al cerrar `SC-001` sin cambios, o tras resolver la confirmación, la sesión termina sin procesos residuales; la recuperación se invalida si se guardó o descartó. La exportación a PDF, los metadatos editables y la doble vista quedan fuera de F-002 (F-009, F-007, F-003).
