using Xunit;

// The suite builds Avalonia controls from unit tests without a visual tree:
// the menu bar (T-008) and the format bar (T-009) create menus, buttons and
// panels outside a running application. The controls' item collection
// (ItemCollection) requires affinity with the Avalonia main dispatcher
// thread, which is assigned to the first thread touching it; with test
// classes running in parallel on different threads, the menu count and
// construction can fail intermittently (Dispatcher.VerifyAccess).
// Parallelization is disabled so the whole suite runs sequentially on a
// single thread and the affinity is stable.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
