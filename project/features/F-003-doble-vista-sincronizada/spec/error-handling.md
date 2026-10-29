# Error Handling — F-003 Doble vista sincronizada

Mensajes en el idioma activo (es/en); se muestra el texto base en español.

```
| id | condición | comportamiento | mensaje | reintento/rollback | log |
|---|---|---|---|---|---|
| ERR-016 | Fallo de sincronización entre vistas (el panel opuesto no se actualiza) | Se reintenta la sincronización; si persiste, se avisa de forma no modal y se permite seguir editando; el documento no se pierde | «No se pudo sincronizar las vistas. Sigue editando; se reintentará automáticamente.» | Reintento automático en el siguiente cambio; sin rollback | Requerido. Referencia: LOG-009 |
| ERR-017 | Documento muy grande: la sincronía supera el umbral (<200 ms / degradación perceptible) | La edición sigue disponible; la sincronización se degrada (actualización diferida) sin bloquear; se registra | «El documento es muy grande; la sincronización puede ser más lenta.» | Continuar editando; sin reintento manual | Requerido. Referencia: LOG-009 |
| ERR-018 | Posición sin correspondencia (no existe ancla equivalente en el panel destino) | No se desplaza ni se resalta el panel destino; el resto de la sincronización continúa | Sin mensaje visible; se registra | Sin reintento; se reintenta en el siguiente cambio de panel | Requerido. Referencia: LOG-011 |
| ERR-019 | Error de renderizado WYSIWYG (fórmula LaTeX o diagrama Mermaid) en el panel izquierdo | Se muestra un marcador de error en el panel izquierdo; el Markdown del panel derecho no se altera | «No se pudo renderizar este bloque en la vista WYSIWYG.» | Corregir la sintaxis en el panel derecho; sin rollback | Requerido. Referencia: LOG-009 |
```

No requerido / Fuera de alcance: cancelar un conmutador de visibilidad, ocultar/mostrar sin error y el renderizado normal no generan error; los fallos de E/S de archivo son de F-002 (ERR-006–ERR-015); los fallos de formato completo son de sus Features.