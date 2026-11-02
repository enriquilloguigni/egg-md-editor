# Criterios de aceptación — F-004 Formato Markdown completo

Cubre: FR-002; BR-F004-001..013; NFR-003, NFR-007, NFR-009, NFR-013, NFR-016, NFR-017, NFR-019.

| ID | Criterio | Dado | Cuando | Entonces |
|---|---|---|---|---|
| AC-030 | Énfasis | selección en el panel activo | se aplica negrita, cursiva o tachado | delimitadores Markdown correctos y preview actualizado |
| AC-031 | Encabezados | encabezado existente | se promueve o degrada su nivel | nivel cambia y la jerarquía sigue válida |
| AC-032 | Listas | lista anidada o tareas `- [ ]` | se edita o renderiza | indentación correcta y checkboxes alternables |
| AC-033 | Citas | bloque `>` (anidado) | se renderiza | cita con anidamiento y estilo de cita |
| AC-034 | Código | código inline o bloque ```lang | se renderiza | resaltado de sintaxis y botón de copia |
| AC-035 | Enlaces | `[texto](url)` | se renderiza | enlace seguro (rel/target) y accesible |
| AC-036 | Imágenes | `![alt](url)` | se renderiza | imagen con alt y carga diferida |
| AC-037 | Tablas | tabla GFM | se editan filas, columnas o alineación | estructura válida y preview actualizado |
| AC-038 | Notas al pie | `[^id]` y su definición | se renderiza | nota enlazada y numerada |
| AC-039 | Fórmulas | `$...$` o `$$...$$` | se renderiza | KaTeX sin error visible |
| AC-040 | Diagramas | bloque ```mermaid | se renderiza | diagrama visible o error controlado |
| AC-041 | Escape | texto con caracteres especiales | se escapa | se muestra literal sin formatear |
| AC-042 | Barra de formato | dos paneles, uno con foco | se usa la barra de formato | afecta solo al panel activo |
| AC-043 | Sanitización | Markdown con HTML/script | se renderiza | HTML peligroso eliminado, solo seguro permitido |
| AC-044 | Rendimiento | documento de 10 000 palabras | se edita o renderiza | respuesta ≤ 2 s sin bloquear el hilo UI (NFR-003/013/016) |
| AC-045 | Accesibilidad | lector de pantalla | navega el preview | landmarks, roles y contraste conformes (NFR-009) |

DS-004 — Renderizado completo (FR-002)
Métrica: documento de 10 000 palabras renderiza en ≤ 2 s bajo carga de v1, sin bloqueo del hilo UI.

| Criterio | ID |
|---|---|
| Énfasis | AC-030 |
| Encabezados | AC-031 |
| Listas | AC-032 |
| Citas | AC-033 |
| Código | AC-034 |
| Enlaces | AC-035 |
| Imágenes | AC-036 |
| Tablas | AC-037 |
| Notas al pie | AC-038 |
| Fórmulas | AC-039 |
| Diagramas | AC-040 |
| Escape | AC-041 |
| Barra de formato | AC-042 |
| Sanitización | AC-043 |
| Rendimiento | AC-044 |
| Accesibilidad | AC-045 |
| Renderizado completo | DS-004 |