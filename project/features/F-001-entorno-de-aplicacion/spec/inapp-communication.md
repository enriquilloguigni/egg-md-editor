# Inapp communication — F-001 Entorno de aplicación

COM-001 — Conmutación del idioma de la interfaz
- **Origen:** entrada «Idioma de la interfaz» (menú Herramientas) en `SC-001`.
- **Destino:** catálogo de cadenas i18n y los rótulos y avisos de `SC-001`.
- **Tipo:** síncrona, en proceso (aplicación local 100 % offline, C-001).
- **Información:** enviada — código del idioma elegido `{es, en}` · recibida — textos resueltos por clave en ese idioma y relabelizado aplicado.
- **Propósito:** relabelizar en caliente sin reinicio, conservando ventana y estado (BR-007, NFR-006).
- **Condiciones:** siempre hay un idioma activo, defecto español, mutuamente excluyentes; un único envío por conmutación; el valor no se persiste en F-001 (FR-013 → F-030).

COM-002 — Entrada de teclado → despachador de atajos
- **Origen:** fuente de entrada de teclado de `SC-001`.
- **Destino:** despachador de atajos → acción correspondiente dentro de la aplicación.
- **Tipo:** síncrona, en proceso.
- **Información:** enviada — combinación de teclas pulsadas · recibida — ejecución de la acción si está disponible en F-001; sin respuesta si su acción aún no existe.
- **Propósito:** hacer que respondan los atajos de las acciones disponibles (FR-010) y que el resto permanezca inerte.
- **Condiciones:** solo se emite hacia acciones conectadas en F-001; los atajos de opciones aún sin acción no producen efecto observable (ADR-001).

COM-003 — Interacción con menú y barra → estado visual de `SC-001`
- **Origen:** puntero o teclado sobre la barra de menú o la barra de formato de `SC-001`.
- **Destino:** estado de la sesión (menú principal, aviso flotante) que pinta `SC-001`.
- **Tipo:** síncrona, en proceso.
- **Información:** enviada — evento de apertura o cierre de una entrada, o de puntero sobre un control · recibida — estado actualizado `{Cerrado | Desplegado con entrada activa}` y aviso visible u oculto.
- **Propósito:** mostrar el 100 % de las opciones de menú y barra solo en su parte visual, con un aviso por control (FR-011 y FR-012 en su parte visual; ADR-001).
- **Condiciones:** ningún evento genera emisión hacia una acción: el documento no cambia (ADR-001); sin mensajes propios en F-001.

COM-004 — Cierre de `SC-001` → finalización de la sesión
- **Origen:** ventana `SC-001` (evento de cierre).
- **Destino:** sesión de aplicación.
- **Tipo:** síncrona, en proceso.
- **Información:** enviada — orden de cierre · recibida — confirmación de liberación de los datos de la sesión.
- **Propósito:** terminar la ejecución sin dejar procesos residuales ni estado persistente.
- **Condiciones:** exactamente una ventana por sesión (BR-003); confirmado el cierre, cesan todos los canales internos y la elección de idioma no sobrevive a la sesión (FR-013 → F-030).

- **Asíncronos y entre procesos:** No requerido / Fuera de alcance en F-001 — todo el intercambio es síncrono y en proceso.
- **Comunicación externa y errores:** No requerido / Fuera de alcance — servicios externos en `external-services.md`, fallos de estos canales en `error-handling.md`, flujo de usuario en `process.md`.
