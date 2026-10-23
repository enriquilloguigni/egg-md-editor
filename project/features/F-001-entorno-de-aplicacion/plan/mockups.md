# Mockups — F-001 Entorno de aplicación

Superficie nativa (Avalonia): wireframes SVG. Tokens TK-xxx por nombre.

## SC-001 — Ventana principal (ventana-principal)
Layout: barra de título, menú de 6 entradas, barra de formato, tres columnas (WYSIWYG, Markdown, índice), área de documento vacía; aviso flotante junto al control apuntado. Tokens: TK-001, TK-002, TK-003, TK-004, TK-005, TK-007, TK-008.

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1280 720" width="1280" height="720">
  <rect width="1280" height="720" fill="#f5f5f5" stroke="#9e9e9e"/>
  <rect width="1280" height="32" fill="#e0e0e0" stroke="#9e9e9e"/>
  <text x="12" y="21" font-size="14" fill="#212121">Egg MD Editor</text>
  <rect y="32" width="1280" height="32" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="12" y="53" font-size="13" fill="#212121">Archivo  Edición  Ver  Formato  Herramientas  Ayuda</text>
  <rect y="64" width="1280" height="40" fill="#ffffff" stroke="#9e9e9e"/>
  <g fill="#d9d9d9" stroke="#9e9e9e">
    <rect x="8" y="72" width="24" height="24"/><rect x="40" y="72" width="24" height="24"/><rect x="72" y="72" width="24" height="24"/><rect x="104" y="72" width="24" height="24"/><rect x="136" y="72" width="24" height="24"/>
  </g>
  <rect x="8" y="56" width="96" height="20" rx="4" fill="#212121" opacity="0.85"/>
  <text x="16" y="70" font-size="11" fill="#ffffff">Negrita</text>
  <rect y="104" width="512" height="616" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="12" y="128" font-size="12" fill="#616161">WYSIWYG</text>
  <rect x="512" y="104" width="512" height="616" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="524" y="128" font-size="12" fill="#616161">Markdown</text>
  <rect x="1024" y="104" width="256" height="616" fill="#f0f0f0" stroke="#9e9e9e"/>
  <text x="1036" y="128" font-size="12" fill="#616161">Índice</text>
</svg>
```

## SC-002 — Diálogo (diálogo)
Overlay modal con elevación. Layout: fondo atenuado, cuadro centrado con título, área de formulario, acciones primaria/secundaria, validación inline. Tokens: TK-001, TK-002, TK-004, TK-005, TK-006.

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1280 720" width="1280" height="720">
  <rect width="1280" height="720" fill="#f5f5f5" stroke="#9e9e9e"/>
  <rect width="1280" height="720" fill="#000000" opacity="0.35"/>
  <rect x="320" y="180" width="640" height="360" rx="8" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="344" y="212" font-size="16" fill="#212121">Diálogo</text>
  <rect x="344" y="236" width="592" height="180" fill="#fafafa" stroke="#bdbdbd"/>
  <text x="356" y="260" font-size="12" fill="#616161">Formulario / mensaje</text>
  <rect x="736" y="480" width="104" height="32" rx="4" fill="#1976d2" stroke="#1565c0"/>
  <text x="760" y="501" font-size="12" fill="#ffffff">Aceptar</text>
  <rect x="856" y="480" width="104" height="32" rx="4" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="880" y="501" font-size="12" fill="#212121">Cancelar</text>
</svg>
```

## SC-003 — Ayuda (ayuda)
Ventana con barra de título, navegación por secciones y área de texto seleccionable. Tokens: TK-001, TK-002, TK-004, TK-007.

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 960 640" width="960" height="640">
  <rect width="960" height="640" fill="#f5f5f5" stroke="#9e9e9e"/>
  <rect width="960" height="32" fill="#e0e0e0" stroke="#9e9e9e"/>
  <text x="12" y="21" font-size="14" fill="#212121">Ayuda — Egg MD Editor</text>
  <rect x="0" y="32" width="224" height="608" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="12" y="56" font-size="12" fill="#616161">Secciones</text>
  <rect x="224" y="32" width="736" height="608" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="240" y="56" font-size="12" fill="#616161">Contenido seleccionable</text>
</svg>
```