Run: v1
Implementation: commit 0f12830668871f3e79cb9094b463225ea669394c
Fecha: 2026-10-24
Ejecutado por: agente tester
Entorno: Linux; .NET SDK 10.0.x; `dotnet test` sin `DOTNET_ROOT` (muxer `dotnet`, `UseAppHost=false` al invocar el runner); sin red ni servicios externos.
Motivo: repetición de la corrida del Run v1 tras corregir el runner de pruebas; E-001 falló por el apphost nativo (`app-launch-failed`) al no resolver el runtime instalado en `~/.dotnet` sin `DOTNET_ROOT`.
