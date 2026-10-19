# Mockups — F-001 Entorno de aplicación

v1

SC-001 — Ventana principal · superficie: ventana-principal
Refs: FR-010, FR-011 y FR-012 (parte visual, ADR-001), BR-003, BR-007, NFR-006 · VR-001, VR-002

## Disposición por zonas

```
+--------------------------------------------------------------------+
| Z1  Barra de título: [nombre de la aplicación]            —  □  ✕  |
+--------------------------------------------------------------------+
| Z2  Archivo   Edición   Ver   Formato   Herramientas   Ayuda       |
+--------------------------------------------------------------------+
| Z3  [ic] [ic] [ic▾] [ic] [ic] [ic] [ic] [ic] [ic] [ic] [ic] [ic]   |
|      énfasis · encabezados 1–6 · listas · cita · código · enlace · |
|      imagen · tabla · nota al pie · fórmula · diagrama · índice    |
+--------------------------------------------------------------------+
| Z4  Área de documento — vacía en F-001 (sin vistas ni paneles)     |
|                                                                    |
|      (los avisos flotantes VR-002 se pintan sobre esta zona)       |
+--------------------------------------------------------------------+
```

## Zonas

- Z1 Barra de título — nombre de la aplicación; única ventana de la aplicación (BR-003). Rótulo TK-002 sobre colores TK-001.
- Z2 Barra de menú — entradas Archivo · Edición · Ver · Formato · Herramientas · Ayuda, siempre visibles, con el 100 % de los elementos de FR-011 agrupados bajo cada entrada (ADR-001). El menú contextual de Edición no se representa. Altura y espaciado TK-004; rótulos TK-002; colores TK-001.
- Z3 Barra de formato — un icono (VR-001) por control de FR-012 en orden de escritura: énfasis (negrita, cursiva, tachado) · encabezados 1–6 con desplegable y promover/degradar · listas (viñetas, numeradas, tareas) · cita · código en línea y bloque · enlace · imagen · tabla · nota al pie · fórmula · diagrama · índice. Un aviso flotante (VR-002) por control. Glyph, altura de barra y espaciado TK-004; contraste AA en ambos temas TK-001.
- Z4 Área de documento — vacía: sin vistas ni paneles (FR-001 → F-003) ni contenido de documento (FR-004 → F-002); fondo TK-001. TK-003 no aplica en F-001 (no hay paneles).

## Única entrada con valores

- Herramientas → Idioma de la interfaz: selección mutuamente excluyente, siempre una marcada; valores Español | English; defecto Español; la elección no se persiste en F-001 (FR-013 → F-030).

## Estados

- A — Arranque (por defecto): idioma español, menú cerrado, aviso flotante oculto, ventana única visible (BR-003).
- B — Menú desplegado: una entrada de Z2 abierta; en Herramientas, Idioma de la interfaz con el idioma activo marcado y exclusivo (TK-004, estado de selección).
- C — Aviso flotante visible: burbuja de una línea (VR-002) superpuesta a Z4 junto al control de Z3 apuntado; visible mientras el puntero permanece, oculto al retirarlo.
- Control de Z2/Z3: reposo · foco (navegación por teclado) · selección, según TK-004; sin estado de error en F-001.

## i18n y opciones sin acción

- Relabelizado en caliente: al conmutar es/en, rótulos de Z1–Z3 y avisos cambian sin reinicio, conservando ventana y estado (BR-007); catálogo es/en extensible a más idiomas (FR-010, NFR-006).
- Opciones sin acción conectada (ADR-001): misma apariencia que las conectadas, sin marcas de deshabilitado; abren/cierran el menú o muestran su aviso, sin efecto sobre el documento.

Comando de captura (ventana-principal): sdd-visual-capture ventana-principal SC-001 <salida>
