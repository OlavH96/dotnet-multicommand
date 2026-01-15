using System.IO;
using Dotnet.MultiCommand.Core;
using CliWrap;
using CliWrap.Buffered;

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

    [Fact]
    public async Task Integration_PackAndRunActualCommand_WithQuotedArguments()
    {
        // Get the project directory
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Dotnet.MultiCommand"));
        var testDir = Path.Combine(Path.GetTempPath(), $"mc-integration-test-{Guid.NewGuid()}");
        
        try
        {
            // Create test directory structure
            Directory.CreateDirectory(testDir);
            var subDir = Path.Combine(testDir, "testfolder");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "test.txt"), "test content");

            // Pack the tool
            var packResult = await Cli.Wrap("dotnet")
                .WithArguments(args => args
                    .Add("pack")
                    .Add(projectDir)
                    .Add("-c")
                    .Add("Release")
                    .Add("-o")
                    .Add(Path.Combine(projectDir, "nupkg")))
                .ExecuteBufferedAsync();

            Assert.Equal(0, packResult.ExitCode);

            // Get the package file
            var nupkgDir = Path.Combine(projectDir, "nupkg");
            var nupkgFile = Directory.GetFiles(nupkgDir, "*.nupkg").FirstOrDefault();
            Assert.NotNull(nupkgFile);

            // Uninstall if already installed
            await Cli.Wrap("dotnet")
                .WithArguments(args => args.Add("tool").Add("uninstall").Add("-g").Add("dotnet-multicommand"))
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            // Install the tool globally
            var installResult = await Cli.Wrap("dotnet")
                .WithArguments(args => args
                    .Add("tool")
                    .Add("install")
                    .Add("-g")
                    .Add("dotnet-multicommand")
                    .Add("--add-source")
                    .Add(nupkgDir)
                    .Add("--version")
                    .Add("*"))
                .ExecuteBufferedAsync();

            Assert.Equal(0, installResult.ExitCode);

            // Run the actual command with quoted arguments
            var result = await Cli.Wrap("mc")
                .WithArguments(args => args
                    .Add("echo")
                    .Add("hello world"))
                .WithWorkingDirectory(testDir)
                .ExecuteBufferedAsync();

            Assert.Equal(0, result.ExitCode);
            Assert.Contains("hello world", result.StandardOutput);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }

            // Uninstall the tool
            await Cli.Wrap("dotnet")
                .WithArguments(args => args.Add("tool").Add("uninstall").Add("-g").Add("dotnet-multicommand"))
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
        }
    }
}
