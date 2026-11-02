# Data structures — F-004 Formato Markdown completo

## ENT-020 — Nodo de formato (AST común)
Descripción: nodo del árbol sintáctico (ENT-016) que representa un bloque o elemento en línea del documento. Relaciones: pertenece a ENT-016; se anida en otros nodos; referencia ENT-008.
Atributos: tipo (Catálogo {Bloque, En línea}); clase (Catálogo de tipos de formato); hijos (Lista de nodos, opcional); posición (Inicio/fin en el documento); contenido (Texto, opcional).

## ENT-021 — Encabezado
Descripción: encabezado de nivel 1 a 6 con texto y ancla de destino para enlaces internos (BR-018). Relaciones: nodo de ENT-020; pertenece a ENT-008.
Atributos: nivel (Entero 1–6, requerido); texto (Texto, requerido); ancla (Texto, derivado del texto, único por documento).

## ENT-022 — Bloque de lista
Descripción: lista de viñetas, numerada o de tareas con anidado arbitrario. Relaciones: nodo de ENT-020; contiene ítems anidados.
Atributos: tipo (Catálogo {Viñetas, Numerada, Tareas}, requerido); ítems (Lista ordenada de nodos); nivel de anidado (Entero ≥0); numeración inicial (Entero, solo si Numerada).

## ENT-023 — Cita anidada
Descripción: bloque de cita con niveles de anidado. Relaciones: nodo de ENT-020; contiene nodos hijos.
Atributos: nivel (Entero ≥1); contenido (Lista de nodos).

## ENT-024 — Código
Descripción: código en línea o en bloque, con lenguaje opcional. Relaciones: nodo de ENT-020; pertenece a ENT-008.
Atributos: modo (Catálogo {En línea, Bloque}, requerido); lenguaje (Texto, opcional); contenido (Texto, requerido).

## ENT-025 — Enlace
Descripción: enlace interno (a encabezado) o externo (URL), con texto y título opcional. Relaciones: nodo en línea de ENT-020; destino interno referencia ENT-021.
Atributos: texto (Texto, requerido); destino (Catálogo {Interno, Externo}, requerido); URL o ancla (Texto, requerido); título (Texto, opcional).

## ENT-026 — Imagen
Descripción: imagen con origen y texto alternativo obligatorio. Relaciones: nodo en línea de ENT-020.
Atributos: origen (Ruta o URL, requerido); texto alternativo (Texto, requerido); título (Texto, opcional).

## ENT-027 — Tabla
Descripción: tabla con filas, columnas y alineación por columna, dentro de los límites (NFR-016). Relaciones: nodo de ENT-020; pertenece a ENT-008.
Atributos: filas (Entero 1–1000, def. 2); columnas (Entero 1–50, def. 2); alineación (Lista por columna {Izquierda, Centro, Derecha}); celdas (Lista de texto por fila/columna).

## ENT-028 — Nota al pie
Descripción: referencia y texto de una nota al pie. Relaciones: nodo de ENT-020; pertenece a ENT-008.
Atributos: identificador (Texto, único por documento, requerido); texto (Texto, requerido).

## ENT-029 — Fórmula
Descripción: fórmula LaTeX en línea o en bloque, renderizada en el panel izquierdo (BR-011). Relaciones: nodo de ENT-020.
Atributos: modo (Catálogo {En línea, Bloque}, requerido); código LaTeX (Texto, requerido).

## ENT-030 — Diagrama
Descripción: diagrama Mermaid en bloque, renderizado en el panel izquierdo (BR-011). Relaciones: nodo de ENT-020.
Atributos: código Mermaid (Texto, requerido); estado de renderizado (Catálogo {OK, Error}, def. OK).

## ENT-031 — Texto escapado
Descripción: carácter o secuencia literal obtenida con barra invertida (BR-009). Relaciones: nodo en línea de ENT-020.
Atributos: contenido (Texto literal, requerido).

## Operaciones
```
| id | operación | entrada | salida | precondición |
|---|---|---|---|---|
| OP-025 | Promover encabezado | encabezado ENT-021 | nivel incrementado (máx. 6) | nivel < 6 |
| OP-026 | Degradar encabezado | encabezado ENT-021 | nivel decrementado (mín. 1) | nivel > 1 |
| OP-027 | Anidar bloque | nodo + contenedor | nodo anidado | contenedor admite anidado |
| OP-028 | Desanidar bloque | nodo anidado | nodo al nivel superior | nodo anidado |
| OP-029 | Insertar tabla | filas, columnas, alineación | ENT-027 insertada | dentro de límites (NFR-016) |
| OP-030 | Insertar fórmula | código LaTeX, modo | ENT-029 insertada | código no vacío |
| OP-031 | Insertar diagrama | código Mermaid | ENT-030 insertada | código no vacío |
| OP-032 | Insertar nota al pie | identificador, texto | ENT-028 insertada | identificador único |
| OP-033 | Editar celda | tabla, fila, columna, texto | celda actualizada | índices válidos |
| OP-034 | Aplicar énfasis | selección, tipo {negrita,cursiva,tachado} | nodo de énfasis aplicado | selección válida |
```

## Integridad y catálogos
- El árbol (ENT-016) y el modelo (ENT-008) son únicos por sesión; cada formato es un nodo tipado de ENT-020 (BR-001, BR-009).
- Encabezado nivel 1–6; ancla única por documento (BR-018); los enlaces internos resuelven por ancla.
- Tabla dentro de límites NFR-016; por encima se informa y degrada sin bloquear (ERR-021).
- Fórmula y diagrama renderizan en el panel izquierdo; fallo deja marcador sin alterar Markdown (BR-011, ERR-019).
- Texto alternativo de imagen obligatorio; enlaces peligrosos neutralizados al abrir (NFR-017, ERR-020).
- Catálogos: lista {Viñetas,Numerada,Tareas}; código {En línea,Bloque}; enlace {Interno,Externo}; tabla alineación {Izquierda,Centro,Derecha}; fórmula modo {En línea,Bloque}; diagrama render {OK,Error}.
- Identificadores: nodo por posición en el árbol; encabezado por ancla; tabla por posición; nota por identificador único.

## Entradas y salidas
- Entrada: selección o cursor del panel activo; código LaTeX/Mermaid; parámetros de tabla; destino de enlace e imagen.
- Salida: nodos de formato insertados/actualizados en ENT-008 y ENT-016; ambos paneles sincronizados (BR-F003-001/004).
- Persistencia: contenido Markdown del documento (ENT-008); sin almacenamiento propio de F-004.
