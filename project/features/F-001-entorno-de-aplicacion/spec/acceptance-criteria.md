# Acceptance criteria — F-001 Entorno de aplicación

AC-001 — Arranque con menú y barra completos (FR-011, FR-012)
Dado: la aplicación se ejecuta localmente sin red ni servicios externos.
Cuando: el usuario lanza la aplicación.
Entonces: se abre SC-001 sin pantalla intermedia ni autenticación; muestra los 6 menús y el 100 % de los controles con icono y aviso flotante; el área de documento queda vacía (BR-F001-008); cada icono expone nombre accesible (NFR-009).

AC-002 — Idioma por defecto español y conmutación en caliente (FR-010)
Dado: SC-001 abierta.
Cuando: el usuario alterna Español/English desde Herramientas → Idioma.
Entonces: todos los rótulos, entradas y avisos se relabelizan en el idioma activo sin reinicio; se conservan ventana, estado de menú y aviso; el idioma inicial es Español (BR-013); si el catálogo destino no carga, la conmutación se aborta sin cambio parcial (ERR-003).

AC-003 — Opciones de menú y controles sin acción no afectan el documento (FR-011, FR-012)
Dado: SC-001 abierta con una opción cuya acción pertenece a otra Feature.
Cuando: el usuario abre/cierra la entrada o muestra el aviso del control.
Entonces: solo se abre/cierra la entrada o se muestra el aviso flotante; el documento y el estado de sesión quedan intactos (ADR-001, BR-F001-002).

AC-004 — Atajos de teclado disponibles responden; el resto inerte (FR-010)
Dado: SC-001 abierta.
Cuando: el usuario pulsa un atajo de teclado.
Entonces: responden los atajos de acciones disponibles en F-001 (Alt+letra, Esc, cierre); el resto permanece inerte, sin efecto observable sobre el documento (BR-F001-003).

AC-005 — Segunda ventana: foco, no nueva instancia (BR-001)
Dado: la aplicación ya en ejecución con SC-001 abierta.
Cuando: se solicita una segunda ventana.
Entonces: no se crea otra ventana; SC-001 recibe el foco (A-002, COM-005).

AC-006 — Fallo de arranque: sin ventana usable y código de error (ERR-001, ERR-002)
Dado: fallo durante la inicialización o el pintado de SC-001.
Cuando: la aplicación no puede arrancar.
Entonces: no se presenta ventana usable; el proceso finaliza con código de error y sin procesos residuales; mensaje en stderr en español (LOG-001).

AC-007 — Cierre sin procesos residuales ni persistencia de idioma (FR-010)
Dado: SC-001 abierta.
Cuando: el usuario cierra SC-001.
Entonces: la aplicación termina sin procesos residuales; la elección de idioma no se persiste y el siguiente arranque parte de Español (BR-F001-006, FR-013 → F-010).

DS-001 — Superficie completa y conmutable (FR-010, FR-011, FR-012)
Métrica:
- 100 % de las entradas y de los controles visibles en el primer arranque (tasa = 100 %).
- Conmutación es/en completa sin reinicio ni pérdida de estado (observable: ventana y estado conservados).
- 0 procesos residuales tras el cierre de SC-001.
- 0 claves i18n sin resolver en el idioma activo (fallback español si falta, ERR-004).

## Referencias de trazabilidad

| Criterio | IDs |
|---|---|
| AC-001 | FR-011, FR-012, BR-012, NFR-004, SC-001, BR-F001-008, NFR-009 |
| AC-002 | FR-010, SC-001, BR-013, ERR-003 |
| AC-003 | FR-011, FR-012, SC-001, ADR-001, BR-F001-002 |
| AC-004 | FR-010, SC-001, F-001, BR-F001-003 |
| AC-005 | BR-001, SC-001, A-002, COM-005 |
| AC-006 | ERR-001, ERR-002, SC-001, LOG-001 |
| AC-007 | FR-010, SC-001, BR-F001-006, FR-013, F-010 |
| DS-001 | FR-010, FR-011, FR-012, SC-001, ERR-004 |