# Mockups — F-002 Gestión de archivo

Superficie nativa (Avalonia): wireframes SVG. Tokens TK-xxx por nombre.

## SC-001 — Ventana principal (ventana-principal)
Superficie: ventana-principal
Estados visuales: A base — tema claro, documento nuevo, menú cerrado; B — tema oscuro; C — arrastre activo. Layout: ventana 1280×720 con barra de título «Sin título», rótulo Archivo desplegable (Nuevo, Abrir…, Recientes, Guardar, Guardar como…), submenú Recientes con hasta 10 entradas (nombre + carpeta padre) o «No hay archivos recientes» deshabilitado, área de documento editable, aviso no modal de fallo de autoguardado. Tokens: TK-001, TK-002, TK-003, TK-004, TK-005, TK-007.

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1280 720" width="1280" height="720" role="img" id="sc001a" data-surface="ventana-principal" data-state="A" data-tokens="TK-001 TK-002 TK-003 TK-004 TK-005 TK-007" font-family="Segoe UI, sans-serif">
  <title>SC-001 · A — tema claro, documento nuevo, menú cerrado</title>
  <rect width="1280" height="720" fill="#f5f5f5" stroke="#9e9e9e"/>
  <rect width="1280" height="32" fill="#e0e0e0" stroke="#9e9e9e"/>
  <text x="12" y="21" font-size="14" fill="#212121">Egg MD Editor — Sin título</text>
  <rect y="32" width="1280" height="40" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="16" y="56" font-size="13" fill="#212121">Archivo  Edición  Ver  Formato  Herramientas  Ayuda</text>
  <rect y="72" width="1280" height="648" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="16" y="96" font-size="12" fill="#616161">Área de documento editable</text>
  <rect x="0" y="690" width="1280" height="30" fill="#fff3cd" stroke="#ffc107"/>
  <text x="12" y="710" font-size="12" fill="#856404">Aviso: no se pudo autoguardar (no modal)</text>
</svg>
```

## SC-002 — Diálogo (diálogo)
Superficie: diálogo
Estados visuales: A base — confirmación de descarte; B — error de E/S; C — selector nativo. Layout: overlay modal con elevación, cuadro centrado con título, formulario, acciones primaria/secundaria, validación inline. Tokens: TK-001, TK-002, TK-004, TK-005, TK-006.

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1280 720" width="1280" height="720" role="img" id="sc002a" data-surface="diálogo" data-state="A" data-tokens="TK-001 TK-002 TK-004 TK-005 TK-006" font-family="Segoe UI, sans-serif">
  <title>SC-002 · A — confirmación de descarte</title>
  <rect width="1280" height="720" fill="#f5f5f5" stroke="#9e9e9e"/>
  <rect width="1280" height="720" fill="#000000" opacity="0.35"/>
  <rect x="320" y="180" width="640" height="320" rx="8" fill="#ffffff" stroke="#9e9e9e"/>
  <text x="344" y="212" font-size="16" fill="#212121">Confirmación</text>
  <text x="344" y="248" font-size="12" fill="#212121">El documento «Sin título» tiene cambios sin guardar. ¿Quieres guardarlos antes de continuar?</text>
  <rect x="760" y="440" width="96" height="32" rx="4" fill="#1976d2" stroke="#1565c0"/><text x="780" y="461" font-size="12" fill="#ffffff">Guardar</text>
  <rect x="864" y="440" width="96" height="32" rx="4" fill="#ffffff" stroke="#9e9e9e"/><text x="872" y="461" font-size="12" fill="#212121">No guardar</text>
</svg>
```