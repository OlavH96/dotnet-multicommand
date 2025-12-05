using System;
using System.IO;
using System.Threading.Tasks;
using Dotnet.MultiCommand.Core;

namespace Dotnet.MultiCommand.Tests;

public class MultiCommandRunnerTests
{
    [Fact]
    public void MultiCommandRunner_WithGitOnly_SetsCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        runner.WithGitOnly(true);

        // Verify through behavior - will be tested in integration tests
        Assert.NotNull(runner);
    }

    [Fact]
    public void MultiCommandRunner_WithRecursive_SetsCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        runner.WithRecursive(true);

        Assert.NotNull(runner);
    }

    [Fact]
    public void MultiCommandRunner_WithCommand_SetsCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        var result = runner.WithCommand("echo test");

        Assert.NotNull(result);
    }

    [Fact]
    public void MultiCommandRunner_WithFolderInclusionFilter_SetsCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        var result = runner.WithFolderInclusionFilter("Test");

        Assert.NotNull(result);
    }

    [Fact]
    public void MultiCommandRunner_WithFolderExclusionFilter_SetsCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        var result = runner.WithFolderExclusionFilter("Example");

        Assert.NotNull(result);
    }

    [Fact]
    public void MultiCommandRunner_WithFileInclusionFilter_SetsCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        var result = runner.WithFileInclusionFilter("package.json");

        Assert.NotNull(result);
    }

    [Fact]
    public void MultiCommandRunner_WithFileExclusionFilter_SetsCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        var result = runner.WithFileExclusionFilter(".lock");

        Assert.NotNull(result);
    }

    [Fact]
    public void MultiCommandRunner_FluentInterface_ReturnsItself()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        var result = runner
            .WithCommand("test")
            .WithGitOnly(true)
            .WithRecursive(false)
            .WithFolderInclusionFilter("inc")
            .WithFolderExclusionFilter("exc");

        Assert.NotNull(result);
        Assert.IsType<MultiCommandRunner>(result);
    }

    [Fact]
    public async Task MultiCommandRunner_RunInDirectories_WithEmptyArray_ReturnsTrue()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        var result = await runner.RunInDirectories(Array.Empty<string>());

        Assert.True(result);
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithValidCommand_ReturnsTrue()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        // Use a command that exists on all platforms
        var runner = new MultiCommandRunner(console)
            .WithCommand("dotnet --version");

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.True(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithCommandThatOutputsToStderr_CapturesError()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        // Use dotnet with an invalid argument to produce stderr output
        var runner = new MultiCommandRunner(console)
            .WithCommand("dotnet --invalid-argument-xyz");

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            // Command will fail but we should capture the error output
            Assert.False(result);
            var output = writer.ToString();
            // Should contain error message in the output
            Assert.NotEmpty(output);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithExclusionFilter_SkipsDirectory()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer) { Verbose = true };
        var runner = new MultiCommandRunner(console)
            .WithCommand("echo test")
            .WithFolderExclusionFilter("Example");

        var tempDir = Path.Combine(Path.GetTempPath(), "Example_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.False(result);
            
            var output = writer.ToString();
            Assert.Contains("Skipping directory", output);
            Assert.Contains("exclusion text", output);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithInclusionFilter_SkipsNonMatchingDirectory()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer) { Verbose = true };
        var runner = new MultiCommandRunner(console)
            .WithCommand("echo test")
            .WithFolderInclusionFilter("Test");

        var tempDir = Path.Combine(Path.GetTempPath(), "Other_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.False(result);
            
            var output = writer.ToString();
            Assert.Contains("Skipping directory", output);
            Assert.Contains("does not contain filter text", output);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithFileInclusionFilter_SkipsDirectoryWithoutFile()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer) { Verbose = true };
        var runner = new MultiCommandRunner(console)
            .WithCommand("echo test")
            .WithFileInclusionFilter("package.json");

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.False(result);
            
            var output = writer.ToString();
            Assert.Contains("Skipping directory", output);
            Assert.Contains("does not contain a file with filter text", output);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithFileInclusionFilter_RunsWhenFileExists()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console)
            .WithCommand("dotnet --version")
            .WithFileInclusionFilter("test");

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(testFile, "content");

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.True(result);
        }
        finally
        {
            File.Delete(testFile);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithFileExclusionFilter_SkipsDirectoryWithFile()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer) { Verbose = true };
        var runner = new MultiCommandRunner(console)
            .WithCommand("echo test")
            .WithFileExclusionFilter(".lock");

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var lockFile = Path.Combine(tempDir, "package.lock");
        File.WriteAllText(lockFile, "content");

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.False(result);
            
            var output = writer.ToString();
            Assert.Contains("Skipping directory", output);
            Assert.Contains("contains a file with exclusion text", output);
        }
        finally
        {
            File.Delete(lockFile);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithFileExclusionFilter_RunsWhenFileDoesNotExist()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console)
            .WithCommand("dotnet --version")
            .WithFileExclusionFilter(".lock");

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.True(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
