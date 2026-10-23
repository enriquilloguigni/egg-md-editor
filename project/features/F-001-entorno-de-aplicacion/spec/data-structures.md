# Data structures — F-001 Entorno de aplicación

## ENT-001 — Sesión de aplicación
Descripción: estado efímero del arranque al cierre de `SC-001`; no persiste (FR-013 → F-010). Relaciones: contiene una `ENT-002`; al cerrarse libera todos los datos.
Atributos: idioma activo (Catálogo Idioma, def. Español); estado del menú (Cerrado/Desplegado, def. Cerrado); aviso flotante (Booleano, def. No); ventana (`ENT-002`); preferencias (`ENT-003`).

## ENT-002 — Ventana principal
Descripción: ventana única `SC-001` (BR-001, A-002); superficie ventana-principal. Relaciones: una por sesión (`ENT-001`); recibe foco si se solicita una segunda ventana.
Atributos: estado (Abierta/Cerrada, def. Abierta); menú (`ENT-004`); barra de formato (`ENT-005`).

## ENT-003 — Preferencias de UI
Descripción: idioma y tema de la interfaz; en F-001 solo en memoria (FR-013 → F-010). Relaciones: una por sesión; persistencia entre sesiones fuera de F-001.
Atributos: idioma (Catálogo Idioma, def. Español); tema (Claro/Oscuro, def. heredado del SO).

## ENT-004 — Menú principal
Descripción: estructura visual de los 6 menús de FR-011, 100 % visibles, sin acciones (ADR-001). Relaciones: alimenta los rótulos visibles de `ENT-002`.
Atributos: menús y entradas (Lista ordenada de 6 menús, completitud 100 %); rótulo (Texto i18n, resuelto desde `ENT-006`).

## ENT-005 — Barra de formato
Descripción: controles de FR-012 en orden de escritura, 100 % visibles, sin acciones (ADR-001). Relaciones: se superpone sobre el área de documento vacía de `ENT-002`.
Atributos: control (Lista ordenada, identificado por posición; icono y aviso por control); icono (Recurso visual, visible en ambos temas); aviso flotante (Texto i18n, se muestra al pasar el puntero).

## ENT-006 — Catálogo i18n
Descripción: pares clave→texto por idioma para rótulos y avisos; base extensible (A-005). Relaciones: la sesión resuelve rótulos contra este catálogo en el idioma activo.
Atributos: clave (Identificador, única); textos por idioma (Texto por código {es,en}, requerido para es y en, def. español).

## ENT-007 — Atajo de teclado
Descripción: binding de tecla a comando disponible en F-001; el resto inerte (process.md). Relaciones: asociado a `ENT-002`; se invoca vía `OP-005`.
Atributos: combinación (Texto tecla/modificador); comando (Referencia a acción, solo los disponibles en F-001); activo (Booleano, def. Sí).

## Operaciones
- OP-001 Cambiar idioma: entrada código {es,en}; salida relabelizado en caliente; precondición sesión activa.
- OP-002 Conmutar tema: entrada tema {Claro,Oscuro}; salida tema aplicado; precondición `ENT-003`.
- OP-003 Abrir/cerrar menú: entrada entrada de menú; salida estado del menú; precondición ventana abierta.
- OP-004 Mostrar/ocultar aviso: entrada control de barra; salida aviso visible/oculto; precondición control válido en `ENT-005`.
- OP-005 Invocar atajo: entrada combinación; salida comando si disponible o sin efecto; precondición atajo registrado en `ENT-007`.
- OP-006 Cerrar ventana: entrada ninguna; salida finalización de sesión; precondición ventana abierta.

## Integridad y catálogos
- Exactamente un idioma activo y una ventana por sesión (BR-001); sin entradas ni controles ausentes.
- Acciones de menú/control: fuera de alcance en F-001 (conexión F-002, F-004, F-005, F-009, F-010; ADR-001).
- Preferencias no persisten entre sesiones en F-001 (FR-013 → F-010).
- Idioma={Español,English}; menú={Cerrado,Desplegado}; aviso={Visible,Oculto}; tema={Claro,Oscuro}.
- Identificadores: sesión por ejecución; ventana `ENT-002`; cadena por clave única; menú por menú+posición; control por posición; atajo por combinación.

## Entradas y salidas
- Entrada: selección de idioma, alternancia de tema, pulsación de atajo.
- Salida: relabelizado en caliente, tema aplicado, aviso mostrado.
- Documento y persistencia: No requerido / Fuera de alcance en F-001.
