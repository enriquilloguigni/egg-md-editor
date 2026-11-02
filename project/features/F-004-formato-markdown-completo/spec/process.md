# Process — F-004 Formato Markdown completo

## Inicio

- **Proceso de acceso:** el usuario ya tiene abierta `SC-001` (F-001/F-002/F-003) con un documento cargado y accede al formato desde el menú Formato, la barra de formato (FR-012) o los atajos; actúa sobre el panel activo (BR-016, COM-021). Sin autenticación ni cuentas (BR-012).
- **Condiciones de acceso:** aplicación local en ejecución; un único documento por instancia (BR-001); panel activo identificable; lógica de formato desacoplada de la UI (NFR-007); soporte Windows 10+/macOS 12+/Linux de .NET 10 (NFR-008).
- **Datos requeridos:** el documento único (modelo compartido); selección o posición del cursor en el panel activo; para enlaces e imágenes, destino y texto alternativo; para tablas, filas, columnas y alineación; para fórmulas y diagramas, su código fuente. No se requieren datos externos.
- **Precondiciones:** F-003 completada (ambos paneles sobre el modelo único); parser CommonMark+GFM disponible (C-009, BR-009); sanitización aplicada al abrir (NFR-017); modelo único compartido (BR-001, C-002).

## Flujo principal

1. El usuario selecciona texto o posiciona el cursor en el panel activo y elige un control de formato (menú, barra o atajo) (BR-016, FR-012).
2. El sistema identifica el panel activo y el rango de selección (COM-021).
3. Si hay selección, aplica el formato sobre ella (efecto visual en el panel izquierdo, sintaxis equivalente en el derecho); si no, prepara la estructura correspondiente (BR-016, BR-009).
4. El cambio se escribe en el modelo único y el panel opuesto se sincroniza automáticamente (BR-F003-001/004, COM-017).
5. Énfasis: negrita, cursiva y tachado sobre la selección (BR-009).
6. Encabezados: aplica nivel 1–6 o promueve/degrada el bloque actual; los enlaces internos se resuelven por el encabezado de destino (BR-018).
7. Listas de viñetas, numeradas y de tareas con anidado; citas anidadas por niveles (BR-009).
8. Código en línea y en bloque; enlaces internos y externos; imágenes con texto alternativo (BR-009, BR-018, NFR-017).
9. Tablas: inserción y edición de filas, columnas y alineación dentro de los límites (NFR-016).
10. Notas al pie, fórmulas en línea y en bloque (LaTeX) y diagramas (Mermaid) se insertan y renderizan en el panel izquierdo (BR-011, NFR-013).
11. Escape de sintaxis con barra invertida en el panel Markdown (BR-009).
12. El control de índice de la barra delega su generación a su Feature (FR-003); no altera el contenido aquí.

## Flujos alternativos y de excepción

- **Alternativo — sin selección:** se prepara la estructura correspondiente y el cursor queda en posición de edición (BR-016).
- **Alternativo — panel derecho activo:** se inserta la sintaxis equivalente; el panel izquierdo refleja el resultado (BR-016, COM-021).
- **Alternativo — cancelar enlace/imagen:** no se modifica el documento.
- **Excepción — documento muy grande:** el formato se aplica sin bloquear; la sincronización/renderizado puede degradarse y se registra (ERR-017, NFR-001/NFR-003).
- **Excepción — error de renderizado de fórmula o diagrama:** marcador de error en el panel izquierdo; el Markdown no se altera (ERR-019, BR-011).
- **Excepción — sanitización al abrir:** los enlaces peligrosos se neutralizan o requieren confirmación; no se ejecuta código (ERR-020, NFR-017).
- **Excepción — tabla muy grande:** se informa y se limita o degrada sin bloquear la edición (ERR-021, NFR-016).

## Resultado observable y finalización

- **Resultado:** el usuario aplica el conjunto completo de formato Markdown sobre la selección o prepara la estructura; ambos paneles reflejan el cambio en tiempo real; los enlaces internos resuelven por encabezado; las fórmulas y diagramas se renderizan; el documento sigue siendo CommonMark+GFM válido (FR-002, BR-009, BR-016, BR-018).
- **Finalización:** al cerrar `SC-001` se conservan las preferencias (BR-019); la exportación a PDF, los metadatos editables, el índice automático y la edición avanzada quedan fuera de F-004 (F-005, F-006, FR-003, F-007).
