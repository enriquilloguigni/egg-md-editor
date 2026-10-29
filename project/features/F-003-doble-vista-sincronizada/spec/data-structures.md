# Data structures — F-003 Doble vista sincronizada

## ENT-015 — Vista de panel
Descripción: instancia de un panel de edición (izquierdo WYSIWYG o derecho Markdown) sobre el documento único. Relaciones: dos por sesión (izq./der.); ambas referencian el mismo `ENT-008`; una puede estar oculta.
Atributos: identificador (Identificador, único por panel); lado (Catálogo {Izquierdo, Derecho}); modo (Catálogo {WYSIWYG, Markdown}); visible (Booleano, def. verdadero); ancho (Número, porcentaje del área); posición de desplazamiento (Número, línea/columna o offset); estado de foco (Catálogo {Activo, Inactivo}, def. Inactivo); resaltado pasajero (Referencia a `ENT-018` o ninguno).

## ENT-016 — Árbol sintáctico del documento
Descripción: representación estructurada del documento según CommonMark+GFM, contrato entre sintaxis y WYSIWYG. Relaciones: uno por documento (`ENT-008`); se regenera al cambiar el contenido.
Atributos: documento (Referencia a `ENT-008`); nodos (Lista de bloques en línea/anidados); versión (Entero, se incrementa por cambio); validez (Catálogo {Válido, Parcial}, def. Válido).

## ENT-017 — Mapa de correspondencia de posiciones
Descripción: anclas entre rangos equivalentes de ambos paneles para desplazamiento sincronizado y resaltado. Relaciones: uno por sincronización; se recalcula tras cada cambio.
Atributos: origen (Catálogo {Izquierdo, Derecho}); destino (Catálogo {Izquierdo, Derecho}); anclas (Lista de pares de rangos); cobertura (Porcentaje de contenido con ancla); fecha de cálculo (Fecha/hora).

## ENT-018 — Resaltado pasajero
Descripción: marca temporal sobre un rango equivalente al cambiar de panel; no modifica contenido. Relaciones: pertenece a `ENT-015`; expira automáticamente.
Atributos: panel (Referencia a `ENT-015`); rango (Inicio/fin en el panel destino); inicio (Fecha/hora); duración (Milisegundos, def. breve); estado (Catálogo {Activo, Expirado}, def. Activo).

## ENT-019 — Preferencia de visibilidad
Descripción: preferencias de presentación de paneles y barra que persisten entre sesiones (BR-019, FR-013). Relaciones: una por usuario/sesión; se lee al iniciar y se escribe al cambiar.
Atributos: panel izquierdo visible (Booleano, def. verdadero); panel derecho visible (Booleano, def. verdadero); barra de formato visible (Booleano, def. verdadero); ancho del separador (Número); última actualización (Fecha/hora).

## Operaciones
```
| id | operación | entrada | salida | precondición |
|---|---|---|---|---|
| OP-020 | Renderizar WYSIWYG | árbol sintáctico | vista izquierda actualizada | modelo válido |
| OP-021 | Renderizar Markdown | árbol sintáctico | vista derecha actualizada | modelo válido |
| OP-022 | Sincronizar edición | cambio en un panel | modelo y panel opuesto actualizados | ambos paneles instanciados |
| OP-023 | Calcular correspondencia | modelo actualizado | mapa de posiciones | árbol válido |
| OP-024 | Aplicar resaltado pasajero | cambio de panel activo | rango resaltado y expiración | ancla encontrada |
```

## Integridad y catálogos
- Exactamente un modelo compartido (`ENT-008`) y un árbol (`ENT-016`) por sesión; ambos paneles referencian los mismos (BR-001, BR-F003-001).
- Al menos un panel visible en todo momento; no se ocultan ambos (BR-F003-007).
- El resaltado pasajero expira solo y no altera el árbol ni el contenido (BR-014, BR-F003-006).
- Catálogos: lado {Izquierdo, Derecho}; modo {WYSIWYG, Markdown}; foco {Activo, Inactivo}; árbol {Válido, Parcial}; resaltado {Activo, Expirado}.
- Identificadores: panel por lado; árbol por documento; mapa por sincronización; preferencia por usuario.