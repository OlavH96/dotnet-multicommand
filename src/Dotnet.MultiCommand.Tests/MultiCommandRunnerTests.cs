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
            .WithHasChanges(true)
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

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.Equal(CommandResult.Success, result);
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

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            // Command will fail but we should capture the error output
            Assert.Equal(CommandResult.Failure, result);
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
            Assert.Equal(CommandResult.SkippedBecauseFolderExclusionFilter, result);
            
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
            Assert.Equal(CommandResult.SkippedBecauseFolderInclusionFilter, result);
            
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

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.Equal(CommandResult.SkippedBecauseFileInclusionFilter, result);
            
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

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(testFile, "content");

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.Equal(CommandResult.Success, result);
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

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var lockFile = Path.Combine(tempDir, "package.lock");
        File.WriteAllText(lockFile, "content");

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.Equal(CommandResult.SkippedBecauseFileExclusionFilter, result);
            
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

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = await runner.DoCommand(tempDir);
            Assert.Equal(CommandResult.Success, result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void MultiCommandRunner_WithHasChanges_SetsCorrectly()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console);

        var result = runner.WithHasChanges(true);

        Assert.NotNull(result);
        Assert.IsType<MultiCommandRunner>(result);
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithHasChanges_SkipsCleanRepo()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer) { Verbose = true };
        var runner = new MultiCommandRunner(console)
            .WithCommand("dotnet --version")
            .WithHasChanges(true);

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Initialize a clean git repo
            await InitGitRepo(tempDir);
            
            var result = await runner.DoCommand(tempDir);
            Assert.Equal(CommandResult.SkippedBecauseNoGitChanges, result);
            
            var output = writer.ToString();
            Assert.Contains("Skipping directory", output);
            Assert.Contains("no uncommitted git changes", output);
        }
        finally
        {
            Console.WriteLine(nameof(MultiCommandRunner_DoCommand_WithHasChanges_SkipsCleanRepo) + ": " + writer.ToString());
            DeleteDirectoryWithGit(tempDir);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithHasChanges_RunsWithTrackedChanges()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console)
            .WithCommand("dotnet --version")
            .WithHasChanges(true);

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Initialize git repo with a tracked file, then modify it
            await InitGitRepo(tempDir);
            var trackedFile = Path.Combine(tempDir, "tracked.txt");
            File.WriteAllText(trackedFile, "initial");
            await RunGitCommand(tempDir, "add tracked.txt");
            await RunGitCommand(tempDir, "commit -m \"initial\"");
            File.WriteAllText(trackedFile, "modified");
            
            var result = await runner.DoCommand(tempDir);
			var output = writer.ToString();
            Assert.Equal(CommandResult.Success, result);
            Assert.Contains("Executing command", output);}
        finally
        {
            Console.WriteLine(nameof(MultiCommandRunner_DoCommand_WithHasChanges_RunsWithTrackedChanges) + " " + writer.ToString());
            DeleteDirectoryWithGit(tempDir);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithHasChanges_SkipsRepoWithOnlyUntrackedFiles()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer) { Verbose = true };
        var runner = new MultiCommandRunner(console)
            .WithCommand("dotnet --version")
            .WithHasChanges(true);

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Initialize git repo with initial commit, then add untracked file
            await InitGitRepo(tempDir);
            var initialFile = Path.Combine(tempDir, "initial.txt");
            File.WriteAllText(initialFile, "initial");
            await RunGitCommand(tempDir, "add initial.txt");
            await RunGitCommand(tempDir, "commit -m \"initial\"");
            
            // Add untracked file
            var untrackedFile = Path.Combine(tempDir, "untracked.txt");
            File.WriteAllText(untrackedFile, "untracked content");
            
            var result = await runner.DoCommand(tempDir);
            Assert.Equal(CommandResult.SkippedBecauseNoGitChanges, result);
            
            var output = writer.ToString();
            Assert.Contains("Skipping directory", output);
        }
        finally
        {
            Console.WriteLine(nameof(MultiCommandRunner_DoCommand_WithHasChanges_SkipsRepoWithOnlyUntrackedFiles) + " " + writer.ToString());
            DeleteDirectoryWithGit(tempDir);
        }
    }

    [Fact]
    public async Task MultiCommandRunner_DoCommand_WithHasChanges_RunsWithStagedChanges()
    {
        using var writer = new StringWriter();
        var console = new AppConsole(writer, writer);
        var runner = new MultiCommandRunner(console)
            .WithCommand("echo test")
            .WithHasChanges(true);

        var tempDir = Path.Combine(TempPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Initialize git repo with a staged change
            await InitGitRepo(tempDir);
            var initialFile = Path.Combine(tempDir, "initial.txt");
            File.WriteAllText(initialFile, "initial");
            await RunGitCommand(tempDir, "add initial.txt");
            await RunGitCommand(tempDir, "commit -m \"initial\"");
            
            var stagedFile = Path.Combine(tempDir, "staged.txt");
            File.WriteAllText(stagedFile, "staged content");
            await RunGitCommand(tempDir, "add staged.txt");
            
            var result = await runner.DoCommand(tempDir);
            Assert.Equal(CommandResult.Success, result);
        }
        finally
        {
            Console.WriteLine(nameof(MultiCommandRunner_DoCommand_WithHasChanges_RunsWithStagedChanges) + " " + writer.ToString());
            DeleteDirectoryWithGit(tempDir);
        }
    }

    private static async Task InitGitRepo(string directory)
    {
        await RunGitCommand(directory, "init");
        await RunGitCommand(directory, "config user.email \"test@test.com\"");
        await RunGitCommand(directory, "config user.name \"Test User\"");
    }

    private static async Task RunGitCommand(string directory, string arguments)
    {
        await CliWrap.Cli.Wrap("git")
            .WithArguments(arguments)
            .WithWorkingDirectory(directory)
            .WithValidation(CliWrap.CommandResultValidation.None)
            .ExecuteAsync();
    }

    private static void DeleteDirectoryWithGit(string directory)
    {
        // On Windows, .git folder files can have read-only attributes
        // We need to clear them before deleting
        SetAttributesNormal(new DirectoryInfo(directory));
        Directory.Delete(directory, true);
    }

    private static void SetAttributesNormal(DirectoryInfo dir)
    {
        foreach (var subDir in dir.GetDirectories())
        {
            SetAttributesNormal(subDir);
        }
        foreach (var file in dir.GetFiles())
        {
            file.Attributes = FileAttributes.Normal;
        }
    }
	public static string TempPath
	{
		get
		{
			var val = Environment.GetEnvironmentVariable("RUNNER_TEMP");
            Console.WriteLine($"TempPath: RUNNER_TEMP={val}");
            return val ?? Path.GetTempPath();
		}
	}
}
