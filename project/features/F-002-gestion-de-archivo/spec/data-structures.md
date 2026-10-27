# Data structures — F-002 Gestión de archivo

## ENT-008 — Documento abierto
Descripción: documento único de la instancia (BR-001); contenido lógico y estado de persistencia. Relaciones: uno por sesión; referencia un `ENT-009` opcional y una `ENT-012` mientras tenga cambios.
Atributos: identificador (Identificador, único por sesión); nombre visible (Texto, «Sin título» si no hay ruta); ruta (Ruta absoluta o ninguna); contenido (Texto Markdown); codificación (Catálogo {UTF-8}, def. UTF-8); fin de línea (Catálogo {CRLF,LF}, def. el del SO); estado de cambios (Catálogo {Limpio,Con cambios}, def. Limpio); origen (Catálogo {Nuevo,Abrir,Reciente,Arrastrar,Recuperado}); última operación (Catálogo {Ninguna,Abrir,Guardar,Guardar como,Recuperar}).

## ENT-009 — Bloque de metadatos
Descripción: bloque delimitado al inicio del documento, tratado como contenido opaco por F-002; su esquema y edición son F-007 (BR-005). Relaciones: parte de `ENT-008`; se conserva al abrir y guardar.
Atributos: presencia (Booleano); contenido íntegro (Texto, desde el delimitador inicial hasta el final del bloque); posición (Inicio); delimitadores (Texto).

## ENT-010 — Lista de recientes
Descripción: hasta 10 archivos abiertos o guardados, ordenados del más reciente al más antiguo; persiste entre sesiones (BR-008, FR-013). Relaciones: contiene 0–10 `ENT-011`; se actualiza al abrir o guardar.
Atributos: entradas (Lista ordenada, máx. 10, sin duplicados); orden (Descendente por última operación); persistencia (Local, sí).

## ENT-011 — Entrada reciente
Descripción: referencia a un archivo `.md` de la lista. Relaciones: pertenece a `ENT-010`; puede apuntar a una ruta no disponible.
Atributos: ruta (Ruta absoluta, única); nombre visible (Texto); carpeta padre (Texto); última operación (Fecha/hora); disponibilidad (Catálogo {Disponible,No disponible}, def. Disponible).

## ENT-012 — Copia de recuperación
Descripción: instantánea local del documento con cambios, escrita por el autoguardado cada 30 s; no es el archivo del usuario (BR-008, A-009). Relaciones: una vigente por `ENT-008`; se invalida al guardar o descartar.
Atributos: identificador de documento (Identificador); ruta original (Ruta o ninguna); contenido (Texto Markdown); marca de tiempo (Fecha/hora); estado (Catálogo {Vigente,Obsoleta}, def. Vigente).

## ENT-013 — Diálogo de archivo
Descripción: instancia de `SC-002` para Abrir o Guardar como. Relaciones: la abre `COM-009`; su resultado lo consume `COM-010`.
Atributos: modo (Catálogo {Abrir,Guardar como}); carpeta actual (Ruta); nombre (Texto); filtro (Texto, `*.md`); validez (Catálogo {Válido,Inválido}); mensaje de validación (Texto o ninguno).

## ENT-014 — Confirmación de descarte
Descripción: instancia de `SC-002` que decide el destino de los cambios pendientes. Relaciones: la abre `COM-009`; su resultado lo consume `COM-010`; bloquea la operación pendiente.
Atributos: documento (Referencia a `ENT-008`); acción pendiente (Catálogo {Nuevo,Abrir,Reciente,Arrastrar,Cerrar}); opción (Catálogo {Guardar,No guardar,Cancelar}, def. Guardar); resultado (Catálogo {Pendiente,Resuelta}, def. Pendiente).

## Operaciones
```
| id | operación | entrada | salida | precondición |
|---|---|---|---|---|
| OP-008 | Crear documento nuevo | comando Nuevo | documento vacío sin ruta | sin cambios pendientes o descarte resuelto |
| OP-009 | Abrir desde selector | ruta `.md` elegida | documento cargado | ruta existente, legible y UTF-8 |
| OP-010 | Abrir por arrastre | lista de rutas soltadas | documento cargado o rechazo | exactamente una ruta `.md` |
| OP-011 | Abrir reciente | entrada `ENT-011` | documento cargado o entrada eliminada | entrada seleccionada |
| OP-012 | Guardar | documento con ruta | archivo escrito y documento limpio | ruta escribible |
| OP-013 | Guardar como | carpeta y nombre | copia escrita y nueva ruta | nombre/ruta válidos |
| OP-014 | Autoguardar | documento con cambios | copia `ENT-012` vigente | temporizador de 30 s |
| OP-015 | Recuperar al iniciar | copia vigente | documento restaurado con cambios | copia legible |
| OP-016 | Confirmar descarte | acción pendiente | opción {Guardar,No guardar,Cancelar} | cambios sin guardar |
| OP-017 | Leer y decodificar | ruta | contenido lógico y fin de línea | archivo accesible |
| OP-018 | Escribir y normalizar | contenido y ruta | archivo UTF-8 con finales del SO | ruta escribible |
| OP-019 | Actualizar recientes e invalidar recuperación | ruta y resultado | lista ordenada y copia obsoleta | operación completada |
```

## Integridad y catálogos
- Exactamente un `ENT-008` por sesión; abrir o crear reemplaza, nunca coexisten (BR-001).
- `ENT-010` máx. 10, rutas únicas comparadas según el SO, orden descendente estricto; al abrir o guardar la ruta pasa al frente (BR-008).
- Una sola `ENT-012` vigente por documento; sustituida por cada autoguardado; obsoleta al guardar o descartar; nunca se restaura una obsoleta.
- Codificación UTF-8; fin de línea CRLF/LF; tras guardar, todas las líneas usan la convención del SO; no se añade ni elimina el salto final.
- `ENT-009` se conserva íntegro en contenido, orden y posición; solo se normalizan los finales de línea.
- Escritura completa o archivo original intacto; no hay estados parciales.
- Catálogos: modo {Abrir,Guardar como}; opción {Guardar,No guardar,Cancelar}; estado {Limpio,Con cambios}; recuperación {Vigente,Obsoleta}; disponibilidad {Disponible,No disponible}; origen {Nuevo,Abrir,Reciente,Arrastrar,Recuperado}.
- Identificadores: documento por sesión; reciente por ruta canónica; recuperación por documento.

## Entradas y salidas
- Entrada: comandos de menú/atajo, ruta del selector, ruta de arrastre, selección de reciente, tick de 30 s, arranque.
- Salida: contenido y ruta del documento, archivo `.md` escrito, lista de recientes, copia de recuperación, título/indicador, diálogos y registros.
- Persistencia: archivos `.md` elegidos por el usuario (UTF-8, finales del SO); recientes y recuperación solo en almacenamiento local; sin red (BR-012, NFR-005).
