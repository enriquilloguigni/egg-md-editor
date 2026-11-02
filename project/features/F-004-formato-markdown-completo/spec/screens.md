# Screens — F-004 Formato Markdown completo

## SC-001 — Ventana principal (F-004)

Propósito: reutilizar la ventana principal de F-001/F-002/F-003; F-004 no introduce pantallas nuevas.

El formato completo se aplica desde el menú Formato, la barra de formato (FR-012) y los atajos, actuando sobre la selección o preparando la estructura en el panel activo (BR-016, FR-002), sobre SC-001.

Elementos: menú Formato y barra de formato (12 controles de FR-012) ya presentes; indicador de panel activo; área de documento con paneles (F-003); diálogos modales sobre la superficie genérica para enlace (texto + destino interno/externo), imagen (origen + texto alternativo), tabla (filas, columnas, alineación), fórmula (código LaTeX) y diagrama (código Mermaid); escape de sintaxis con barra invertida en el panel Markdown.

Campos: Panel activo — {Izquierdo, Derecho}; Selección/cursor — rango o posición; Enlace — texto (requerido), destino (requerido: encabezado interno o URL externa), título (opcional); Imagen — origen (requerido), texto alternativo (requerido), título (opcional); Tabla — filas (1–1000, def. 2), columnas (1–50, def. 2), alineación por columna {izquierda, centro, derecha}; Fórmula — código LaTeX (requerido, en línea o bloque); Diagrama — código Mermaid (requerido); Encabezado — nivel 1–6; Lista — tipo {viñetas, numerada, tareas} con anidado; Nota al pie — identificador y texto.

Acciones: aplicar énfasis (negrita, cursiva, tachado); promover/degradar encabezado; insertar listas, citas anidadas, código en línea/en bloque; insertar enlace, imagen, tabla, nota al pie, fórmula y diagrama; editar filas, columnas y alineación de tabla; escapar sintaxis con barra invertida; cancelar cualquier diálogo sin modificar el documento; todos los controles operables por teclado con aviso flotante (NFR-009, FR-011).

Mensajes: avisos flotantes en idioma activo (es/en); errores de renderizado de fórmula/diagrama como marcador en el panel izquierdo sin alterar el Markdown (ERR-019); límites de tabla informados con degradación sin bloqueo (ERR-021); sanitización de enlaces peligrosos al abrir (ERR-020, NFR-017); sin mensajes de éxito propios.

Estados: panel activo izquierdo/derecho; selección presente/vacía; diálogo de formato abierto/cerrado y válido/inválido; tabla dentro/de los límites; renderizado de fórmula/diagrama ok/error; escape activo.

## Crítica

- Los diálogos de formato reutilizan la superficie genérica SC-002 (F-001) para foco, validación inline y Esc; F-004 no crea SC nuevo y solo concreta campos y validaciones aquí (renuncia declarada a una pantalla dedicada).
- La tabla admite hasta 1 000 filas y 50 columnas (NFR-016): la UI debe deshabilitar o degradar la inserción por encima del límite sin bloquear la edición (ERR-021).
- Los campos de fórmula/diagrama son código fuente sin vista previa en el diálogo: la previsualización queda en el panel izquierdo; si el renderizado falla, el marcador de error no debe perder el código (ERR-019).
- Accesibilidad: todos los controles de la barra y los diálogos operables por teclado, con foco visible y anuncios no solo cromáticos (NFR-009); el texto alternativo de imagen es obligatorio.