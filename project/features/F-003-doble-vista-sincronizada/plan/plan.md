F-003, Spec: v1

## Dependencias

### Features
- F-002 — Gestión de archivo (documento único y carga/guarda UTF-8).

### Componentes/módulos
- .NET 10 + Avalonia UI, MVVM, parser CommonMark+GFM (C-009), adaptador de plataforma (A-008).

### Servicios externos
No requerida — 100 % local/offline (BR-012, NFR-004, NFR-005).

### Datos/estructuras
| ENT/OP | Disposición técnica |
|---|---|
| ENT-015/016 | Vista de panel (izq./der.) y árbol sintáctico compartido del documento. |
| ENT-017/018 | Mapa de correspondencia de posiciones y resaltado pasajero (no modifica contenido). |
| ENT-019 | Preferencias de visibilidad y ancho del separador, persistidas. |
| OP-020/021 | Renderizado WYSIWYG y Markdown desde el árbol. |
| OP-022..OP-024 | Sincronización, correspondencia y resaltado. |

### Configuración/infraestructura
- Directorio de configuración del usuario para preferencias (A-008, ENT-019); ancho del separador persistente.

### Librerías/paquetes
- Parser Markdown CommonMark+GFM con licencia MIT-compatible (NFR-011); sin red.

### Otras
- ADR-001 (superficie sin acciones conectadas) se extiende con Ver → conmutadores; C-002/C-009.

## Riesgos
- Sincronía lenta en documentos grandes: degradación diferida sin bloquear y registro (ERR-017, NFR-001/NFR-003).
- Fallo de sincronización entre vistas: reintento y aviso no modal; el documento no se pierde (ERR-016).
- Posición sin ancla equivalente: no desplazar ni resaltar; registrar (ERR-018).
- Error de renderizado WYSIWYG (LaTeX/Mermaid): marcador en panel izquierdo, Markdown intacto (ERR-019).
- Accesibilidad del splitter y resaltado: foco visible, teclado, prefers-reduced-motion (NFR-009).
- Ocultar ambos paneles: impedir la transición; al menos un panel visible (BR-F003-007).

## Secuencia
1. Preparación: T-040, T-041, T-042 — árbol, paneles y renderizado.
2. Sincronización: T-043, T-044, T-045, T-046 — eventos, correspondencia, resaltado, scroll.
3. Interfaz: T-047, T-048, T-049, T-050, T-051, T-052 — splitter, visibilidad, preferencias, barra, foco.
4. Robustez: T-053, T-054, T-055, T-056 — degradación, ancla, logs, accesibilidad.
5. Integración final: T-057, T-058 — preferencias al iniciar y pruebas AC-024..AC-029/DS-003.

## Tareas
- T-040 Implementar parser CommonMark+GFM y árbol sintáctico del documento (ENT-016, BR-F003-008, C-009).
- T-041 Instanciar los dos paneles (izq. WYSIWYG, der. Markdown) sobre el mismo modelo (ENT-015, BR-F003-001/002/003).
- T-042 Implementar renderizado WYSIWYG (OP-020) y Markdown (OP-021) con marcador de error (ERR-019).
- T-043 Implementar sincronización bidireccional por eventos sin sondeo (OP-022, COM-016/017, BR-F003-004).
- T-044 Implementar motor de correspondencia de posiciones (ENT-017, OP-023, COM-018, BR-F003-005).
- T-045 Implementar resaltado pasajero con expiración (ENT-018, OP-024, COM-019, BR-F003-006, BR-014, VR-007).
- T-046 Implementar desplazamiento sincronizado entre paneles (FR-009, COM-018).
- T-047 Implementar layout de doble panel con separador redimensionable (VR-008, NFR-009).
- T-048 Implementar conmutadores de visibilidad de paneles con restricción de al menos uno (ST-018–ST-020, TR-034–TR-037, COM-020, BR-F003-007).
- T-049 Implementar conmutador de visibilidad de barra de formato (ST-021/ST-022, TR-038/039, COM-021, BR-F003-007, BR-016).
- T-050 Implementar persistencia de preferencias de visibilidad y ancho (ENT-019, BR-019, FR-013).
- T-051 Implementar barra de formato actuando sobre el panel activo (COM-021, BR-016, VR-009).
- T-052 Implementar indicador de panel activo (VR-009, NFR-009).
- T-053 Implementar degradación de rendimiento y reintento de sincronía (ERR-016/017, LOG-009, NFR-001/NFR-003).
- T-054 Implementar manejo de posición sin ancla (ERR-018, LOG-011).
- T-055 Implementar registro técnico LOG-009/LOG-010/LOG-011.
- T-056 Implementar accesibilidad de la doble vista (NFR-009, AC-029, VR-007–VR-009).
- T-057 Cargar preferencias de visibilidad al iniciar la sesión (ENT-019, BR-019).
- T-058 Pruebas unitarias de sincronización ≥70 % y verificación AC-024..AC-029/DS-003.