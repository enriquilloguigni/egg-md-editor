# Criterios de aceptación — F-004 Formato Markdown completo

AC-030 — Énfasis (FR-002)

Dado: selección en el panel activo.
Cuando: se aplica negrita, cursiva o tachado.
Entonces: delimitadores Markdown correctos y preview actualizado.

AC-031 — Encabezados (FR-002)

Dado: encabezado existente.
Cuando: se promueve o degrada su nivel.
Entonces: nivel cambia y la jerarquía sigue válida.

AC-032 — Listas (FR-002)

Dado: lista anidada o tareas `- [ ]`.
Cuando: se edita o renderiza.
Entonces: indentación correcta y checkboxes alternables.

AC-033 — Citas (FR-002)

Dado: bloque `>` (anidado).
Cuando: se renderiza.
Entonces: cita con anidamiento y estilo de cita.

AC-034 — Código (FR-002)

Dado: código inline o bloque ```lang.
Cuando: se renderiza.
Entonces: resaltado de sintaxis y botón de copia.

AC-035 — Enlaces (FR-002, BR-018, NFR-017)

Dado: `[texto](url)`.
Cuando: se renderiza.
Entonces: enlace seguro (rel/target) y accesible.

AC-036 — Imágenes (FR-002, NFR-009)

Dado: `![alt](url)`.
Cuando: se renderiza.
Entonces: imagen con alt y carga diferida.

AC-037 — Tablas (FR-002, NFR-016)

Dado: tabla GFM.
Cuando: se editan filas, columnas o alineación.
Entonces: estructura válida y preview actualizado.

AC-038 — Notas al pie (FR-002)

Dado: `[^id]` y su definición.
Cuando: se renderiza.
Entonces: nota enlazada y numerada.

AC-039 — Fórmulas (FR-002, BR-011, NFR-013)

Dado: `$...$` o `$$...$$`.
Cuando: se renderiza.
Entonces: KaTeX sin error visible.

AC-040 — Diagramas (FR-002, BR-011, NFR-013)

Dado: bloque ```mermaid.
Cuando: se renderiza.
Entonces: diagrama visible o error controlado.

AC-041 — Escape (FR-002, BR-009)

Dado: texto con caracteres especiales.
Cuando: se escapa.
Entonces: se muestra literal sin formatear.

AC-042 — Barra de formato (FR-012, BR-016)

Dado: dos paneles, uno con foco.
Cuando: se usa la barra de formato.
Entonces: afecta solo al panel activo.

AC-043 — Sanitización (FR-002, NFR-017)

Dado: Markdown con HTML/script.
Cuando: se renderiza.
Entonces: HTML peligroso eliminado, solo seguro permitido.

AC-044 — Rendimiento (NFR-003, NFR-013, NFR-016)

Dado: documento de 10 000 palabras.
Cuando: se edita o renderiza.
Entonces: respuesta ≤ 2 s sin bloquear el hilo UI.

AC-045 — Accesibilidad (NFR-009)

Dado: lector de pantalla.
Cuando: navega el preview.
Entonces: landmarks, roles y contraste conformes.

DS-004 — Renderizado completo (FR-002)

Métrica: documento de 10 000 palabras renderiza en ≤ 2 s bajo carga de v1, sin bloqueo del hilo UI.

```
| Criterio | IDs |
|---|---|
| AC-030 | FR-002, BR-F004-001, BR-009, BR-016 |
| AC-031 | FR-002, BR-F004-002, BR-009, BR-018 |
| AC-032 | FR-002, BR-F004-003, BR-009 |
| AC-033 | FR-002, BR-F004-004, BR-009 |
| AC-034 | FR-002, BR-F004-005, BR-009 |
| AC-035 | FR-002, BR-F004-006, BR-018, NFR-017 |
| AC-036 | FR-002, BR-F004-007, NFR-009, BR-012 |
| AC-037 | FR-002, BR-F004-008, NFR-016, ERR-021 |
| AC-038 | FR-002, BR-F004-009, BR-009 |
| AC-039 | FR-002, BR-F004-010, BR-011, NFR-013, ERR-019 |
| AC-040 | FR-002, BR-F004-011, BR-011, NFR-013, ERR-019 |
| AC-041 | FR-002, BR-F004-012, BR-009 |
| AC-042 | FR-002, FR-012, BR-F004-013, BR-016, COM-021 |
| AC-043 | FR-002, NFR-017, ERR-020, LOG-014 |
| AC-044 | NFR-003, NFR-013, NFR-016, NFR-019, ERR-017 |
| AC-045 | NFR-009, SC-001 |
| DS-004 | FR-002, NFR-003, NFR-013, NFR-016 |
```