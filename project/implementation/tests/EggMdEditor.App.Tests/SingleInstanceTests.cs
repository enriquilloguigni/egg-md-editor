using System;
using System.Threading.Tasks;
using EggMdEditor.App.Startup;
using Xunit;

namespace EggMdEditor.App.Tests;

// T-012 (COM-005, BR-001, A-002): una única instancia/ventana por usuario;
// la segunda instancia pide el foco de la existente y no crea ventana.
public class SingleInstanceTests
{
    private static string NewApplicationId() => "EggMdEditor.Tests." + Guid.NewGuid().ToString("N");

    [Fact]
    public void FirstAcquisitionIsPrimaryAndASecondOneIsNot()
    {
        var applicationId = NewApplicationId();

        using var first = SingleInstanceCoordinator.Acquire(applicationId);
        using var second = SingleInstanceCoordinator.Acquire(applicationId);

        Assert.True(first.IsPrimary);
        Assert.False(second.IsPrimary);
    }

    [Fact]
    public void DifferentApplicationIdentifiersDoNotShareTheInstanceLock()
    {
        using var first = SingleInstanceCoordinator.Acquire(NewApplicationId());
        using var other = SingleInstanceCoordinator.Acquire(NewApplicationId());

        Assert.True(first.IsPrimary);
        Assert.True(other.IsPrimary);
    }

    [Fact]
    public void TheInstanceLockIsReleasedWhenThePrimaryInstanceEnds()
    {
        var applicationId = NewApplicationId();
        var first = SingleInstanceCoordinator.Acquire(applicationId);
        Assert.True(first.IsPrimary);

        first.Dispose();

        using var next = SingleInstanceCoordinator.Acquire(applicationId);
        Assert.True(next.IsPrimary);
    }

    [Fact]
    public async Task ASecondInstanceSignalsFocusToTheExistingOne()
    {
        var applicationId = NewApplicationId();
        using var primary = SingleInstanceCoordinator.Acquire(applicationId);
        using var secondary = SingleInstanceCoordinator.Acquire(applicationId);
        Assert.False(secondary.IsPrimary);

        var activation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        primary.ActivationRequested += (_, _) => activation.TrySetResult(true);

        Assert.True(secondary.TryRequestActivation(TimeSpan.FromSeconds(5)));

        var completed = await Task.WhenAny(
            activation.Task,
            Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.Same(activation.Task, completed);
        Assert.True(await activation.Task);
    }

    [Fact]
    public void SignalingWithoutARunningInstanceReportsFailure()
    {
        var applicationId = NewApplicationId();

        Assert.False(SingleInstanceCoordinator.TryRequestActivation(
            applicationId,
            TimeSpan.FromMilliseconds(250)));
    }
}
