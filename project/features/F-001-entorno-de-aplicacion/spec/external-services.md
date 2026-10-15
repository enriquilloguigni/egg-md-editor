# External services — F-001 Entorno de aplicación

**No requerido / Fuera de alcance.**

- **Justificación:** F-001 no consume ningún servicio externo al sistema. La aplicación es 100 % offline (C-001, NFR-002): arranca desde código fuente local (A-001), sin red, sin terceros, sin autenticación y sin datos que entren o salgan del proceso; el único intercambio es interno e en proceso, definido en `inapp-communication.md` (`COM-001`–`COM-004`).
- **Alcance declarado:** sin `EXT-xxx` que definir — ni proveedor, propósito, datos enviados/recibidos, dependencias, autenticación, límites ni comportamiento ante indisponibilidad, porque no existe servicio externo que invocar ni punto de fallo por indisponibilidad externa.
- **Relación con `inapp-communication.md`:** allí los canales internos de la Feature (idioma, atajos, estado visual de `SC-001`, cierre); aquí, nada: no hay servicios externos, ni de la propia organización, que consuma esta Feature.
- **Revisión:** si una Feature posterior introduce integración externa, su propio `external-services.md` definirá los `EXT-xxx` correspondientes; este documento de F-001 queda sin cambios.
