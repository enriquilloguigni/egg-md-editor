# States and transitions — F-002 Gestión de archivo

## Estados
- ST-012 — Documento nuevo sin ruta y sin cambios (inicial por defecto) · ST-013 — Documento con ruta y sin cambios
- ST-014 — Documento con cambios sin guardar · ST-015 — Documento recuperado con cambios sin guardar
- ST-016 — Confirmación de descarte pendiente · ST-017 — Documento cerrado (final)

| Aspecto | Estados |
|---|---|
| Estado inicial | ST-012, o ST-015 si al arrancar existe una copia de recuperación vigente |
| Documento con ruta | ST-013 (limpio) / ST-014 (con cambios) / ST-015 (recuperado) |
| Documento sin ruta | ST-012 (limpio) / ST-014 (con cambios) |
| Cierre | ST-017 solo tras guardar o descartar los cambios pendientes |

## Transiciones
```
| id | origen → destino | evento | condición |
|---|---|---|---|
| TR-012 | ST-012 → ST-014 | Editar contenido | primer cambio |
| TR-013 | ST-013 → ST-014 | Editar contenido | cambio de contenido |
| TR-014 | ST-014 → ST-013 | Guardar exitoso | el documento tiene ruta |
| TR-015 | ST-012 → ST-013 | Guardar como exitoso | ruta válida y escritura correcta |
| TR-016 | ST-014 → ST-013 | Guardar como exitoso | ruta válida y escritura correcta |
| TR-017 | ST-013 → ST-013 | Guardar como exitoso | documento limpio; se crea copia |
| TR-018 | ST-013 → ST-012 | Nuevo | sin cambios pendientes |
| TR-019 | ST-013 → ST-013 | Abrir/Reciente/Arrastrar | sin cambios; destino válido |
| TR-020 | ST-012 → ST-013 | Abrir/Reciente/Arrastrar | sin cambios; destino válido |
| TR-021 | ST-014 → ST-016 | Nuevo/Abrir/Reciente/Arrastrar/Cerrar | cambios sin guardar |
| TR-022 | ST-016 → ST-014 | Cancelar | documento con cambios |
| TR-023 | ST-016 → ST-015 | Cancelar | documento recuperado con cambios |
| TR-024 | ST-016 → ST-013 | Guardar y continuar | guardado exitoso; se reanuda la acción pendiente |
| TR-025 | ST-016 → ST-012 | No guardar → Nuevo | descarte confirmado |
| TR-026 | ST-016 → ST-013 | No guardar → Abrir/Reciente/Arrastrar | descarte confirmado y destino válido |
| TR-027 | ST-016 → ST-017 | No guardar → Cerrar | descarte confirmado |
| TR-028 | ST-012 → ST-015 | Recuperación automática al iniciar | copia vigente y legible |
| TR-029 | ST-015 → ST-013 | Guardar exitoso | ruta disponible; si no, Guardar como |
| TR-030 | ST-015 → ST-016 | Nuevo/Abrir/Reciente/Arrastrar/Cerrar | cambios recuperados |
| TR-031 | ST-013 → ST-017 | Cerrar | sin cambios |
| TR-032 | ST-012 → ST-017 | Cerrar | documento vacío sin cambios |
| TR-033 | ST-016 → ST-017 | Guardar y cerrar | guardado exitoso |
```

## No permitidas
- Reemplazar el documento con cambios pendientes sin resolver la confirmación (BR-F002-014).
- Guardar sobre un archivo sin permiso de escritura o de solo lectura: no hay transición; permanece el estado actual (ST-012/ST-014/ST-015) (ERR-009/010).
- Restaurar una copia de recuperación obsoleta: no hay transición a ST-015 (BR-F002-015).
- Abrir más de un documento o una segunda ventana: no hay transición; rige BR-001.
- Cerrar con cambios sin decisión explícita: no hay transición a ST-017 (TR-027/TR-033).
- Abrir un archivo que no es `.md` o que no decodifica como UTF-8: no hay transición; permanece el estado actual (ERR-008/014).
