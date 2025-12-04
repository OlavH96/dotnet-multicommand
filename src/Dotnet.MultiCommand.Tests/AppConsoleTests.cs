using System;
using System.IO;
using Dotnet.MultiCommand.Core;

namespace Dotnet.MultiCommand.Tests;

public class AppConsoleTests
{
    [Fact]
    public void AppConsole_WriteError_WritesToOutput()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);

        console.WriteError("Error message");

        var output = writer.ToString();
        Assert.Contains("Error message", output);
    }

    [Fact]
    public void AppConsole_WriteSuccess_WritesToOutput()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);

        console.WriteSuccess("Success message");

        var output = writer.ToString();
        Assert.Contains("Success message", output);
    }

    [Fact]
    public void AppConsole_WriteHighlighted_WritesToOutput()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);

        console.WriteHighlighted("Highlighted message");

        var output = writer.ToString();
        Assert.Contains("Highlighted message", output);
    }

    [Fact]
    public void AppConsole_WriteHeader_WritesToOutput()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);

        console.WriteHeader("Header message");

        var output = writer.ToString();
        Assert.Contains("Header message", output);
    }

    [Fact]
    public void AppConsole_WriteNormal_WritesToOutput()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);

        console.WriteNormal("Normal message");

        var output = writer.ToString();
        Assert.Contains("Normal message", output);
    }

    [Fact]
    public void AppConsole_WriteEmptyLine_WritesNewLine()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);

        console.WriteEmptyLine();

        var output = writer.ToString();
        Assert.Equal(Environment.NewLine, output);
    }

    [Fact]
    public void AppConsole_WriteVerbose_WhenVerboseTrue_WritesToOutput()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer) { Verbose = true };

        console.WriteVerbose("Verbose message");

        var output = writer.ToString();
        Assert.Contains("Verbose message", output);
    }

    [Fact]
    public void AppConsole_WriteVerbose_WhenVerboseFalse_DoesNotWrite()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer) { Verbose = false };

        console.WriteVerbose("Verbose message");

        var output = writer.ToString();
        Assert.Empty(output);
    }

    [Fact]
    public void AppConsole_TrimsNewLines_FromMessages()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);

        console.WriteNormal("Message with newline\n\r");

        var output = writer.ToString();
        Assert.Equal("Message with newline" + Environment.NewLine, output);
    }

    [Fact]
    public void AppConsole_DefaultInstance_IsNotNull()
    {
        Assert.NotNull(AppConsole.Default);
    }
}
