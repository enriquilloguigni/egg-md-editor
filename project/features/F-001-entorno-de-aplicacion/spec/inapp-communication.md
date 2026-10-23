# In-app communication — F-001 Entorno de aplicación

## COM-001 — Comando compartido menú/barra/atajo
Origen: Menú principal (ENT-004), barra de formato (ENT-005) y handler de atajos (ENT-007).
Destino: Dispatcher de comandos compartidos (ViewModel).
Tipo: Síncrono (invocación directa).
Información enviada: referencia de comando disponible.
Información recibida: sin efecto observable si no hay acción conectada (ADR-001).
Propósito: un único punto de despacho para menú, barra y atajos (FR-011, FR-012).
Condiciones: ventana abierta; comando registrado en ENT-007.

## COM-002 — Relabelizado por idioma
Origen: Selector de idioma (ENT-003).
Destino: Catálogo i18n (ENT-006) y superficies de SC-001.
Tipo: Síncrono.
Información enviada: código {es,en}.
Información recibida: textos resueltos y relabelizado en caliente.
Propósito: conmutar idioma sin reinicio (BR-013, OP-001).
Condiciones: sesión activa.

## COM-003 — Aplicación de tema
Origen: Selector de tema (ENT-003).
Destino: superficies de UI.
Tipo: Síncrono.
Información enviada: tema {Claro,Oscuro}.
Información recibida: tema aplicado.
Propósito: conmutar tema claro/oscuro (OP-002).
Condiciones: preferencias disponibles.

## COM-004 — Aviso flotante de barra
Origen: Control de barra de formato (ENT-005).
Destino: Aviso flotante (tooltip).
Tipo: Síncrono.
Información enviada: clave i18n del control.
Información recibida: aviso visible/oculto.
Propósito: mostrar aviso por control al pasar el puntero (FR-012).
Condiciones: control válido en ENT-005.

## COM-005 — Enfoque de ventana única
Origen: Lanzador / segunda instancia solicitada.
Destino: Ventana principal SC-001 (ENT-002).
Tipo: Síncrono (señal de plataforma).
Información enviada: solicitud de foco.
Información recibida: ventana existente recibe el foco; no se crea otra.
Propósito: un único documento/ventana (BR-001, A-002).
Condiciones: aplicación ya en ejecución.

## COM-006 — Apertura y cierre de menú
Origen: Entrada de menú o teclado.
Destino: Estado del menú (ENT-001/ENT-004).
Tipo: Síncrono.
Información enviada: entrada de menú.
Información recibida: estado {Cerrado,Desplegado}.
Propósito: desplegar/cerrar menú (OP-003).
Condiciones: ventana abierta.
