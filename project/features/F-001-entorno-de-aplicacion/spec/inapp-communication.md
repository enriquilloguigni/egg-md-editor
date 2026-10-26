# In-app communication — F-001 Entorno de aplicación

## COM-001 — Comando compartido menú/barra/atajo
Origen: Menú principal (ENT-004), barra de formato (ENT-005) y handler de atajos (ENT-007) → Destino: dispatcher de comandos compartidos (ViewModel).
Tipo: Síncrono (invocación directa).
Información enviada/recibida: se envía la referencia de comando disponible; se recibe sin efecto observable si no hay acción conectada (ADR-001).
Propósito: un único punto de despacho para menú, barra y atajos (FR-011, FR-012); condición: ventana abierta y comando registrado.

## COM-002 — Relabelizado por idioma
Origen: Selector de idioma (ENT-003) → Destino: catálogo i18n (ENT-006) y superficies de SC-001.
Tipo: Síncrono.
Información enviada/recibida: se envía el código {es,en}; se reciben textos resueltos y relabelizado en caliente.
Propósito: conmutar idioma sin reinicio (BR-013, OP-001); condición: sesión activa.

## COM-003 — Aplicación de tema
Origen: Selector de tema (ENT-003) → Destino: superficies de UI.
Tipo: Síncrono.
Información enviada/recibida: se envía el tema {Claro,Oscuro,Sistema}; se recibe el tema efectivo aplicado (Claro/Oscuro), resolviendo Sistema del SO.
Propósito: conmutar tema claro/oscuro/sistema (OP-002, TR-008/TR-009); condición: preferencias disponibles en ENT-003.

## COM-004 — Aviso flotante de barra
Origen: Control de barra de formato (ENT-005) → Destino: aviso flotante (tooltip).
Tipo: Síncrono.
Información enviada/recibida: se envía la clave i18n del control; se recibe aviso visible/oculto.
Propósito: mostrar aviso por control al pasar el puntero (FR-012); condición: control válido en ENT-005.

## COM-005 — Enfoque de ventana única
Origen: Lanzador / segunda instancia solicitada → Destino: ventana principal SC-001 (ENT-002).
Tipo: Síncrono (señal de plataforma).
Información enviada/recibida: se envía la solicitud de foco; se recibe la ventana existente enfocada y ninguna ventana nueva.
Propósito: un único documento/ventana (BR-001, A-002); condición: aplicación ya en ejecución.

## COM-006 — Menú y navegación secuencial
Origen: Rótulo seleccionado, botones «Siguiente/Anterior» o teclado → Destino: estado del menú y rótulo seleccionado (ENT-001/ENT-004).
Tipo: Síncrono.
Información enviada/recibida: se envía dirección {Siguiente,Anterior} o activación del rótulo; se recibe rótulo seleccionado 1–6 y estado {Cerrado,Desplegado}.
Propósito: navegar cíclicamente entre los 6 rótulos y desplegar/cerrar el seleccionado (OP-003, OP-007); condición: ventana abierta.

Referencias:
- TR-004/TR-005 (propósito — apertura y cierre del menú).
- TR-010/TR-011 (propósito — avance y retroceso cíclico).
