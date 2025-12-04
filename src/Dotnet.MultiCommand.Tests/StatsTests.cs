using Dotnet.MultiCommand.Core;

namespace Dotnet.MultiCommand.Tests;

public class StatsTests
{
    [Fact]
    public void Stats_DefaultValue_IsZero()
    {
        var stats = new Stats(0);
        Assert.Equal(0, stats.NumberOfCommandsRan);
    }

    [Fact]
    public void Stats_WithValue_UpdatesCorrectly()
    {
        var stats = new Stats(5);
        var updated = stats with { NumberOfCommandsRan = 10 };

        Assert.Equal(10, updated.NumberOfCommandsRan);
        Assert.Equal(5, stats.NumberOfCommandsRan); // Original unchanged
    }

    [Fact]
    public void Stats_Increment_WorksCorrectly()
    {
        var stats = new Stats(0);
        stats = stats with { NumberOfCommandsRan = stats.NumberOfCommandsRan + 1 };
        stats = stats with { NumberOfCommandsRan = stats.NumberOfCommandsRan + 1 };

        Assert.Equal(2, stats.NumberOfCommandsRan);
    }
}
