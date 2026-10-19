using Xunit;

// La suite construye controles de Avalonia desde pruebas unitarias sin árbol
// visual: la barra de menú (T-008) y la barra de formato (T-009) crean menús,
// botones y paneles fuera de una aplicación en marcha. La colección de
// elementos de los controles (ItemCollection) exige afinidad con el hilo del
// dispatcher principal de Avalonia, que queda asignado al primer hilo que lo
// toca; con las clases de pruebas ejecutándose en paralelo en hilos distintos,
// el recuento y la construcción del menú pueden fracasar de forma intermitente
// (Dispatcher.VerifyAccess). Se desactiva la paralelización para que toda la
// suite corra secuencial en un único hilo y la afinidad sea estable.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
