# Acceptance Criteria — F-001 Entorno de aplicación

```
AC-001 — Arranque en ventana única en español (FR-010, BR-003)
Dado:
El usuario compila y lanza la aplicación desde el código fuente (A-001) y el catálogo i18n inicial carga correctamente.
Cuando:
El proceso inicia y crea la interfaz.
Entonces:
- Existe exactamente una ventana, SC-001, presentada completa (BR-003).
- La interfaz está en español: un único idioma activo, con Herramientas → Idioma de la interfaz marcado en Español (BR-F001-004).
- La sesión pasa de ST-001 a ST-002; no hay arranque directo en inglés (TR-001).

AC-002 — Menú principal persistente al 100 % (FR-011)
Dado:
SC-001 presentada en ST-002 o ST-003.
Cuando:
El usuario observa la barra de menú en cualquier momento de la sesión.
Entonces:
- Los 6 menús —Archivo, Edición, Ver, Formato, Herramientas, Ayuda— están visibles con el 100 % de las entradas declaradas en FR-011 (BR-F001-001).
- La barra permanece siempre visible, con el menú cerrado o desplegado con una entrada activa.
- Cada entrada muestra su rótulo resuelto en el idioma activo (BR-F001-004).

AC-003 — Barra de formato completa con icono y aviso (FR-012)
Dado:
SC-001 presentada con el área de documento vacía.
Cuando:
El usuario observa la barra de formato.
Entonces:
- Los controles de FR-012 están visibles al 100 %, en el orden de escritura (BR-F001-001).
- Cada control muestra su icono (VR-001) y, al puntero, su aviso flotante (VR-002) resuelto en el idioma activo.

AC-004 — Opción de menú sin acción conectada (FR-011)
Dado:
SC-001 en ST-002 o ST-003, con una entrada del menú cuya acción pertenece a F-028 (p. ej. Archivo → Nuevo).
Cuando:
El usuario pulsa la entrada.
Entonces:
- La entrada abre y cierra el menú sin ningún efecto sobre el documento ni sobre el estado de sesión (BR-F001-002, ADR-001).

AC-005 — Control de barra con aviso flotante y sin acción (FR-012)
Dado:
SC-001 presentada con la barra de formato visible.
Cuando:
El usuario pasa el puntero sobre un control y lo pulsa.
Entonces:
- El aviso flotante (VR-002) se muestra con el nombre del control mientras el puntero permanece sobre él y se oculta al retirarlo.
- El clic solo muestra el aviso: el documento queda intacto (BR-F001-002, ADR-001).

AC-006 — Atajos: responden los disponibles y el resto permanece inerte (FR-010)
Dado:
SC-001 en ST-002 o ST-003, con el documento intacto.
Cuando:
El usuario invoca un atajo de teclado.
Entonces:
- Si su acción está disponible en F-001, la acción correspondiente se ejecuta una vez (COM-002).
- Si su acción pertenece a otra Feature, no hay efecto observable sobre la interfaz ni el documento (BR-F001-003, ADR-001).

AC-007 — Conmutación de español a inglés en caliente (FR-010, BR-007, NFR-006)
Dado:
La sesión está en ST-002 con SC-001 abierta; un menú puede estar desplegado y un aviso flotante visible.
Cuando:
El usuario elige English en Herramientas → Idioma de la interfaz (COM-001).
Entonces:
- Todos los rótulos, entradas y avisos quedan en inglés sin reinicio, conservando ventana, estado del menú y aviso flotante (BR-F001-005).
- English queda marcado y Español desmarcado; la sesión pasa a ST-003 (TR-002).

AC-008 — Conmutación de inglés a español en caliente (FR-010, BR-007, NFR-006)
Dado:
La sesión está en ST-003 con SC-001 abierta.
Cuando:
El usuario elige Español en Herramientas → Idioma de la interfaz (COM-001).
Entonces:
- Todos los rótulos, entradas y avisos quedan en español sin reinicio, conservando ventana y estado (BR-F001-005).
- Español queda marcado y English desmarcado; la sesión pasa a ST-002 (TR-003).

AC-009 — Claves i18n sin resolver tras la conmutación (FR-010, NFR-006)
Dado:
El catálogo i18n del idioma destino contiene alguna clave sin traducir.
Cuando:
El usuario alterna el idioma y la interfaz se relabeliza.
Entonces:
- La clave sin resolver se resuelve en español, idioma defecto: ningún rótulo ni aviso aparece sin traducir (BR-F001-007, ERR-004).
- La interfaz permanece usable y el fallo queda registrado sin mensaje visible en SC-001 (LOG-002, nivel Warning).

AC-010 — Conmutación abortada sin cambio parcial (ERR-003)
Dado:
El catálogo i18n del idioma destino no carga al intentar la conmutación.
Cuando:
El usuario alterna es/en.
Entonces:
- La conmutación se aborta sin cambio parcial: permanece el idioma activo anterior en toda la UI, con la ventana y el estado intactos (BR-F001-004).
- No se muestra ningún mensaje en SC-001; el fallo solo se registra (LOG-002, nivel Error) y el usuario puede reintentar la conmutación.

AC-011 — Cierre sin procesos residuales ni persistencia (BR-F001-006)
Dado:
La sesión está en ST-002 o ST-003, con inglés como idioma activo.
Cuando:
El usuario cierra SC-001 (COM-004).
Entonces:
- El proceso termina sin procesos residuales y la sesión pasa a ST-004 (TR-004 / TR-005).
- La elección de idioma no se escribe a disco: un nuevo lanzamiento vuelve a arrancar en español (BR-F001-006, FR-013 → F-030).

AC-012 — Fallo de arranque sin ventana usable (ERR-001, ERR-002)
Dado:
Falla la inicialización del proceso o la creación o el pintado de SC-001 (p. ej. el catálogo i18n inicial no carga).
Cuando:
El usuario lanza la aplicación.
Entonces:
- No se presenta ninguna ventana usable y el proceso finaliza con código de error, sin procesos residuales; la sesión no sale de ST-001.
- La salida de error muestra en español «Error al iniciar la aplicación: <tipo y mensaje de la excepción>» y el fallo queda registrado (LOG-001, nivel Error).

AC-013 — Fallo al cerrar la ventana (ERR-005)
Dado:
Se produce una excepción al cerrar SC-001 o al liberar los datos de la sesión.
Cuando:
El usuario cierra la ventana.
Entonces:
- El proceso se termina igualmente, garantizando que no queden procesos residuales (ST-004).
- La salida de error muestra «Error al cerrar la aplicación: <tipo y mensaje de la excepción>», el fallo queda registrado (LOG-003, nivel Error) y la elección de idioma no se persiste (BR-F001-006).

AC-014 — Sin segunda ventana (BR-003)
Dado:
La aplicación ya está en ejecución con SC-001 en ST-002 o ST-003.
Cuando:
Se solicita una segunda ventana de la aplicación.
Entonces:
- No se crea otra ventana: SC-001 recibe el foco y sigue habiendo exactamente una ventana por sesión (BR-003), sin transición de estado.

AC-015 — Superficie visible acotada (BR-F001-008)
Dado:
SC-001 presentada en cualquier estado de la sesión.
Cuando:
El usuario examina la superficie de la ventana.
Entonces:
- Solo se muestran la barra de menú y la barra de formato: el área de documento está vacía, sin vistas, paneles ni contenido (FR-001, FR-004 → F-002/F-003).
- El menú contextual de Edición no se muestra (ADR-001).

DS-001 — Forma completa visible desde el primer entregable (FR-011, FR-012)
Métrica:
- Recuento en SC-001 tras el arranque: 100 % de las entradas de FR-011 y 100 % de los controles de FR-012 visibles; umbral 0 ausencias y 0 elementos no declarados.

DS-002 — Localización es/en sin cadenas sin resolver (FR-010)
Métrica:
- Tasa de claves i18n resueltas en SC-001 = 100 % tras el arranque y tras cada conmutación; umbral 0 rótulos ni avisos sin traducir (BR-F001-007).

DS-003 — Conmutación en caliente sin reinicio (FR-010)
Métrica:
- 100 % de las conmutaciones es/en completadas en la misma sesión conservando ventana y estado; umbral 0 reinicios y 0 cambios parciales (BR-F001-005, ERR-003).
```

No requerido / Fuera de alcance: AC sobre la conexión de opciones de menú, controles de barra y atajos a sus acciones (F-028, F-029; ADR-001), sobre vistas y contenido de documento (FR-001, FR-004 → F-002/F-003), sobre el menú contextual de Edición y sobre la persistencia de la elección de idioma entre sesiones (FR-013 → F-030).
