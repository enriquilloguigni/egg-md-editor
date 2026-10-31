using Xunit;

// La sincronización de F-003 se apoya en estado estático compartido
// (Synchronizer). Serializar la ejecución evita carreras entre pruebas
// paralelas (T-060/HV-014).
[assembly: CollectionBehavior(DisableTestParallelization = true)]