# PD-ROAD-001 — Re-score del dimensionado C1 (Alcance) tras el Roadmap

id: PD-ROAD-001
artifact: project/roadmap/roadmap.md (v1) + project/project.md (Dimensión) + project/discovery/discovery-context.md
blocks: ROADMAP_APPROVED (re-score del dimensionado, D46)
impact: medium

## Decisión
Ajustar el criterio C1 (Alcance) del dimensionado al número real de Features del Roadmap v1 (10), que se sitúa en el descriptor del valor 8 (9–10 features), frente al valor 10 (>15 features) registrado en project.md y al anotado en discovery-context.md (10 — 9-10 features).

## Pregunta concreta
¿Se acepta re-puntuar C1 de 10 a 8 (sin cambio de categoría, ponderado pasa de 6.10 a 5.50, sigue M) o se prefiere re-agrupar/aumentar las Features para sostener C1=10?

## Cursos de acción
a) Re-puntuar C1=8 y actualizar project.md y tier en el checkpoint (recomendado): refleja el alcance real, sin cambio de categoría.
b) Re-agrupar en menos Features (≈5-6) o añadir más (>15) para sostener C1=10: mayor riesgo de sobre/sub-granularización.
c) Mantener C1=10 y aceptar la sobreestimación: deja el dimensionado incoherente con el Roadmap.

## Propuesta
Opción (a): el Roadmap agrupa por valor observable end-to-end (10 Features) y el descriptor de valor 8 encaja; no hay cambio de categoría (M).

## Efecto de elegir
Actualiza project.md (Dimensión) y el checkpoint (tier.score) con ADR; las Features afectadas mantienen sus IDs; desbloquea ROADMAP_APPROVED.