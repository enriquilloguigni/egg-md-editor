# Business Rules — F-001 Entorno de aplicación

Referencia (heredada, no reproducida): textos en Project Spec vigente.

Referencias heredadas:
- `BR-001` — un único documento por instancia
- `BR-012` — aplicación 100 % local y offline
- `BR-013` — interfaz en español o inglés (default español)
- `FR-009` — presentación y navegación (tema claro/oscuro)
- `FR-010` — localización y ayuda
- `FR-011` — menú principal persistente
- `FR-012` — barra de formato con icono y aviso (solo su parte visual en F-001, `ADR-001`)

```
| id | condición | regla | consecuencia |
|---|---|---|---|
| BR-F001-001 | Cualquier estado de la sesión en SC-001 | La barra de menú es persistente y siempre visible con los 6 rótulos de FR-011 como botones; la barra de formato muestra exactamente 12 controles en orden de escritura —1 Énfasis, 2 Encabezados, 3 Listas, 4 Cita, 5 Código, 6 Enlace, 7 Imagen, 8 Tabla, 9 Nota al pie, 10 Fórmula, 11 Diagrama, 12 Índice—, cada uno con icono y aviso flotante | La forma completa se ve desde el primer entregable, con las acciones aún desconectadas (ADR-001, ADR-002) |
| BR-F001-002 | El usuario activa un rótulo de menú o un control de barra cuya acción pertenece a otra Feature (F-002…F-010) | Solo se abre y cierra el rótulo seleccionado o se muestra el aviso flotante; «opciones» de menú designa los 6 rótulos, no entradas internas; ninguna opción se conecta a su acción en F-001 | El documento y el estado de sesión quedan intactos (ADR-001, ADR-002) |
| BR-F001-003 | El usuario invoca un atajo de teclado | Solo responden los atajos de acciones disponibles en F-001; el resto permanece inerte | Ningún atajo produce efecto observable sobre el documento mientras su acción no exista (ADR-001) |
| BR-F001-004 | Cualquier momento de la sesión, incluido el arranque | Hay exactamente un idioma activo de {Español, English}, siempre uno marcado, con Español como defecto al arranque | Todos los rótulos, entradas y avisos se resuelven en el idioma activo, sin textos sin resolver (BR-013, FR-010) |
| BR-F001-005 | El usuario alterna es/en desde Herramientas → Idioma | La interfaz se relabeliza en caliente, conservando ventana, estado del menú y aviso flotante | El cambio surte efecto inmediato sin reinicio y dura hasta el cierre de la sesión (BR-013) |
| BR-F001-006 | Finalización de la sesión (cierre de SC-001) | La elección de idioma y de tema no se escribe a disco ni sobrevive a la sesión en F-001 | Cada arranque parte del defecto español y del tema del SO; su persistencia corresponde a FR-013 → F-010 |
| BR-F001-007 | Toda cadena visible en SC-001 (rótulos de menú, entradas y avisos flotantes) | Se resuelve desde el catálogo de cadenas i18n en el idioma activo, sobre base extensible a más idiomas | Al conmutar idioma no queda ningún texto sin traducir y añadir un idioma no exige cambiar la UI (BR-013, FR-010) |
| BR-F001-008 | Superficie visible de SC-001 en F-001 | Solo se muestran el menú principal y la barra de formato; el área de documento permanece vacía, sin columnas WYSIWYG/Markdown/Índice, y el menú contextual de Edición no se muestra | Vistas, paneles, contenido de documento y menú contextual quedan fuera de F-001 (FR-001, FR-004 → F-002/F-003; ADR-001, ADR-002) |
| BR-F001-009 | El usuario elige tema en Herramientas → Tema | Las opciones visibles son Claro, Oscuro y Sistema, mutuamente excluyentes; el tema inicial se hereda del SO; la selección se aplica en caliente y no persiste en F-001 | El estado oscuro es observable y capturable; la persistencia corresponde a FR-013 → F-010 (OP-002, TR-008, TR-009) |
| BR-F001-010 | El usuario pulsa los botones de navegación secuencial | «Siguiente» aumenta el rótulo seleccionado y «Anterior» lo disminuye, recorriendo cíclicamente los 6 rótulos; la selección no abre menús ni altera el documento | La barra de rótulos es operable sin ambigüedad y sin efecto funcional sobre el documento (OP-007, TR-010, TR-011) |
```
