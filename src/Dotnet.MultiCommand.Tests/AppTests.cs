using System.IO;
using Dotnet.MultiCommand.Core;

namespace Dotnet.MultiCommand.Tests;

public class AppTests
{
    [Fact]
    public void App_Execute_WithNoArguments_ReturnsError()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute();

        Assert.Equal(0xbad, result);
        var output = writer.ToString();
        Assert.Contains("No command specified", output);
    }

    [Fact]
    public void App_Execute_WithHelpFlag_ReturnsSuccess()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("--help");

        Assert.Equal(0, result);
    }

    [Fact]
    public void App_Execute_WithVersionFlag_ReturnsSuccess()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("--version");

        Assert.Equal(0, result);
    }

    [Fact]
    public void App_Execute_WithCommand_Succeeds()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        // Use a simple command that should work on all platforms
        var result = app.Execute("echo", "test");

        Assert.Equal(0, result);
    }

    [Fact]
    public void App_Execute_WithGitFlag_ParsesCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("-g", "echo", "test");

        Assert.Equal(0, result);
    }

    [Fact]
    public void App_Execute_WithRecursiveFlag_ParsesCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("-r", "echo", "test");

        Assert.Equal(0, result);
    }

    [Fact]
    public void App_Execute_WithVerboseFlag_EnablesVerbose()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("--verbose", "echo", "test");

        Assert.Equal(0, result);
        Assert.True(console.Verbose);
    }

    [Fact]
    public void App_Execute_WithIncludeFilter_ParsesCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("-i", "Test", "echo", "test");

        Assert.Equal(0, result);
    }

    [Fact]
    public void App_Execute_WithExcludeFilter_ParsesCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("-e", "Example", "echo", "test");

        Assert.Equal(0, result);
    }

    [Fact]
    public void App_Execute_WithAllFlags_ParsesCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("-g", "-r", "--verbose", "-i", "Test", "-e", "Example", "echo", "test");

        Assert.Equal(0, result);
    }

    [Fact]
    public void App_Execute_WithArgumentContainingSpaces_AddsQuotes()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("git", "commit", "-m", "my commit message");

        Assert.Equal(0, result);
        var output = writer.ToString();
        // Verify the command was constructed with quotes around the message
        Assert.Contains("\"my commit message\"", output);
    }

    [Fact]
    public void App_Execute_WithArgumentWithoutSpaces_NoQuotesAdded()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var app = new App(console);

        var result = app.Execute("echo", "test");

        Assert.Equal(0, result);
        var output = writer.ToString();
        // Verify the command doesn't have unnecessary quotes
        Assert.Contains("echo test", output);
        Assert.DoesNotContain("\"test\"", output);
    }
}
