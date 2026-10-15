# Data structures — F-001 Entorno de aplicación

## Sesión de aplicación

Descripción: estado de la aplicación desde el arranque hasta cerrar la ventana `SC-001`; efímero, no persistente (FR-013 → F-030).

Atributos:
- Idioma activo  Tipo: Catálogo (Idioma)  Requerido: Sí  Defecto: Español  Siempre uno marcado (BR-007).
- Estado del menú principal  Tipo: Catálogo {Cerrado, Desplegado con entrada activa}  Requerido: Sí  Defecto: Cerrado.
- Aviso flotante visible  Tipo: Booleano  Requerido: Sí  Defecto: No  Asociado siempre a un control de la barra de formato.

Relaciones: contiene exactamente una ventana `SC-001` (BR-003); al cerrarse, la sesión termina y todos sus datos se liberan.

## Catálogo del menú principal

Descripción: estructura visual de la barra de menú persistente con el 100 % de las entradas de FR-011, sin conexión a acciones (ADR-001).

Atributos:
- Menús y entradas  Tipo: Lista ordenada de 6 menús (FR-011)  Requerido: Sí  Completitud: 100 % visibles.
- Rótulo de entrada  Tipo: Texto i18n  Requerido: Sí  Resuelto desde el catálogo de cadenas en el idioma activo.

Relaciones: un pertenece a exactamente un menú; alimenta los rótulos visibles de `SC-001`.

## Catálogo de la barra de formato

Descripción: controles de FR-012 en orden de escritura, visibles al 100 % sin conexión a acciones (ADR-001).

Atributos:
- Control  Tipo: Lista ordenada  Requerido: Sí  Identificado por su posición; un icono (VR-001) y un aviso (VR-002) por control.
- Icono  Tipo: Recurso visual  Requerido: Sí  Visible en ambos temas.
- Aviso flotante  Tipo: Texto i18n  Requerido: Sí  Se muestra al pasar el puntero (FR-012).

Relaciones: se superpone sobre el área de documento vacía de `SC-001`.

## Catálogo de cadenas i18n

Descripción: pares clave→texto por idioma que alimentan rótulos y avisos; base extensible a más idiomas (NFR-006).

Atributos:
- Clave  Tipo: Identificador  Requerido: Sí  Única por cadena.
- Textos por idioma  Tipo: Texto por código {es, en}  Requerido: Sí para es y en  Defecto de interfaz: español.

Relaciones: la sesión resuelve sus rótulos contra este catálogo en el idioma activo.

## Identificadores

- Sesión: una por ejecución · ventana por sesión: `SC-001` · cadena: clave única.
- Entrada de menú: menú + posición · control de barra: posición en el orden de escritura.

## Integridad

- Exactamente un idioma activo y exactamente una ventana por sesión (BR-003); sin entradas de menú ni controles ausentes.
- Toda acción asociada a menú o control: No requerido / Fuera de alcance en F-001 (conexión: F-028 y F-029; ADR-001).

## Catálogos y valores controlados

- `Idioma` = {Español, English}, mutuamente excluyentes; estados de menú y aviso flotante según los catálogos anteriores.

## Entradas y salidas

- Entrada: selección de idioma desde Herramientas. Salida: relabelizado en caliente de rótulos y avisos.
- Documento: No requerido / Fuera de alcance — área vacía en F-001 (FR-004 → F-002).
- Persistencia: No requerido / Fuera de alcance — nada se escribe a disco; la elección de idioma no sobrevive a la sesión (FR-013 → F-030).
