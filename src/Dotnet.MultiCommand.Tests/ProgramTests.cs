namespace Dotnet.MultiCommand.Tests;

public class ProgramTests
{
    [Fact]
    public void Program_Main_WithHelp_ReturnsSuccess()
    {
        var result = Program.Main(new[] { "--help" });
        Assert.Equal(0, result);
    }
}
