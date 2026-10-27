F-002, Spec: v1

## Dependencias

### Features
- F-001 — Entorno de aplicación: ventana única, menú Archivo y comandos base.

### Componentes/módulos
- .NET 10 + Avalonia UI, MVVM, adaptador de plataforma (selector nativo, rutas, permisos) y almacenamiento local (A-008).

### Servicios externos
No requerida — 100 % local/offline (BR-012, NFR-004, NFR-005).

### Datos/estructuras
| ENT/OP | Disposición técnica |
|---|---|
| ENT-008/009 | Documento abierto y bloque de metadatos opaco en memoria; edición por F-003/F-007. |
| ENT-010/011 | Recientes (≤10, sin duplicados, orden descendente) persistidos en JSON local. |
| ENT-012 | Copia de recuperación local con marca de tiempo; obsoleta al guardar/descartar. |
| ENT-013/014 | Diálogos SC-002 (abrir/guardar como y confirmación) sobre DialogViewModel. |
| OP-008..OP-019 | Servicio de archivo: lectura/decodificación, normalización, escritura, recientes, recuperación y confirmación. |

### Configuración/infraestructura
- Directorio de configuración del usuario para recientes y copia de recuperación (A-008); temporizador de 30 s.

### Librerías/paquetes
- Solo .NET/Avalonia existentes; licencias MIT-compatibles (NFR-011).

### Otras
- ADR-001 (superficie sin acciones conectadas) se extiende con acciones de archivo; A-009 (autoguardado 30 s).

## Riesgos
- Escritura parcial o pérdida de datos en fallo de E/S: escritura atómica (temporal + rename) y pruebas de fallo.
- Decodificación UTF-8 y finales de línea mixtos: validación estricta y normalización sin alterar salto final.
- Recuperación obsoleta o corrupta al iniciar: validar marca, descartar copia obsoleta y no tocar el original.
- Accesibilidad de diálogos (WCAG 2.1 AA): foco inicial seguro, Esc=Cancelar y textos en es/en.
- Temporizador compitiendo con E/S: serializar operaciones y reintento no modal.
- Persistencia de recientes corrupta: degradar a lista vacía y registrar (ERR-015).

## Secuencia
1. Preparación: T-020, T-021, T-022 — modelo, recientes y recuperación.
2. Datos: T-023, T-024 — codificación/normalización y escritura.
3. Backend/servicios: T-025..T-031 — operaciones de archivo y confirmación.
4. Frontend/interfaz: T-032..T-035 — menú, título, arrastre y diálogos.
5. Integraciones: T-036, T-037 — coordinador y persistencia de recientes.
6. Configuración: T-038 — logging y errores.
7. Integración final: T-039 + verificación AC-010..AC-023 y DS-002.

## Tareas
- T-020 Implementar modelo de documento abierto y bloque de metadatos opaco (ENT-008, ENT-009, BR-F002-002).
- T-021 Implementar almacén local de recientes (≤10, sin duplicados, orden descendente) (ENT-010, ENT-011, OP-019).
- T-022 Implementar copia de recuperación local y su invalidación (ENT-012, BR-F002-012/015).
- T-023 Implementar lectura UTF-8 con/sin BOM y normalización de finales de línea (BR-F002-007/008, OP-017).
- T-024 Implementar escritura atómica UTF-8 sin BOM con finales del SO (BR-F002-005/006, OP-018).
- T-025 Implementar operación Nuevo y reemplazo de documento (OP-008, BR-F002-001/002).
- T-026 Implementar Abrir desde selector y por arrastre con validación (OP-009/010, BR-F002-003/004, ERR-006/007/008/014).
- T-027 Implementar Abrir reciente con eliminación de entrada no disponible (OP-011, BR-F002-010, ERR-006).
- T-028 Implementar Guardar y Guardar como con copia y recientes al frente (OP-012/013, BR-F002-005/006/010).
- T-029 Implementar confirmación de descarte Guardar/No guardar/Cancelar (OP-016, BR-F002-014/015, ERR-011).
- T-030 Implementar autoguardado cada 30 s y aviso no modal de fallo (OP-014, BR-F002-012, ERR-012, COM-011).
- T-031 Implementar recuperación automática al iniciar (OP-015, BR-F002-013/015, ERR-013, COM-013).
- T-032 Implementar menú Archivo (Nuevo, Abrir, Recientes, Guardar, Guardar como) y atajos (SC-001, COM-007).
- T-033 Implementar título con nombre e indicador de cambios accesible (BR-F002-016, COM-015, VR-006).
- T-034 Implementar resaltado de arrastre y validación de un único .md (COM-012, BR-F002-003, ERR-014).
- T-035 Implementar diálogos SC-002 (selector, confirmación, error) con foco y validación inline (ENT-013/014).
- T-036 Implementar coordinador de archivo COM-007..COM-010 y serialización de operaciones.
- T-037 Implementar persistencia de recientes y manejo de fallo (COM-014, ERR-015, LOG-004/005).
- T-038 Implementar registro técnico LOG-004..LOG-008 y errores ERR-006..ERR-015.
- T-039 Añadir pruebas unitarias de lógica de archivo ≥70 % y verificación AC-010..AC-023/DS-002.