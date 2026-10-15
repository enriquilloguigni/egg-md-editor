# Security permissions — F-001 Entorno de aplicación

## Roles y operaciones permitidas

Un único perfil: el usuario local que compila y ejecuta la aplicación en su propio equipo (A-001, BR-008); sin cuentas ni roles diferenciados (process.md, Inicio).

| Operación | Ver `SC-001` con menú y barra | Conmutar idioma es/en | Menús, avisos y atajos de F-001 | Cerrar la ventana |
|---|---|---|---|---|
| Usuario local | Sí | Sí | Sí | Sí |

## Permisos requeridos

Ninguno — F-001 no exige permisos del sistema: sin acceso a disco, sin red y sin servicios externos (C-001, NFR-003); se lanza desde build local, sin instalador (BR-008).

## Restricciones de acceso

No requerido / Fuera de alcance — aplicación local de un solo usuario: el acceso equivale a poder compilar y lanzar la build (A-001); sin credenciales ni control de acceso.

## Restricciones sobre datos (ámbito)

- Protegido: solo el estado efímero de sesión en memoria —idioma activo, estado del menú y aviso flotante (data-structures.md)—; F-001 no gestiona contenido de documento.
- Ningún dato abandona el equipo del usuario: sin telemetría, sin servicios de red y sin terceros (NFR-003, C-001); nada se escribe a disco (FR-013 → F-030).
- Al cerrar `SC-001` se libera toda la sesión; no queda estado persistente.

## Condiciones especiales de autorización

No requerido / Fuera de alcance — no hay operaciones sensibles en F-001: las opciones de menú y barra están solo visibles, sin acción conectada (ADR-001), y conmutar el idioma no requiere autorización.

## Separación de responsabilidades

No requerido / Fuera de alcance — un único perfil sin privilegios diferenciados.
