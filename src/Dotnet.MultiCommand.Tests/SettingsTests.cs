using System;
using Dotnet.MultiCommand.Core;

namespace Dotnet.MultiCommand.Tests;

public class SettingsTests
{
    [Fact]
    public void Settings_DefaultValues_AreCorrect()
    {
        var settings = new Settings();

        Assert.False(settings.GitOnly);
        Assert.False(settings.Recursive);
        Assert.Equal("ls", settings.Command);
        Assert.Null(settings.FolderInclusionFilter);
        Assert.Null(settings.FolderExclusionFilter);
    }

    [Fact]
    public void Settings_WithGitOnly_UpdatesCorrectly()
    {
        var settings = new Settings();
        var updated = settings with { GitOnly = true };

        Assert.True(updated.GitOnly);
        Assert.False(settings.GitOnly); // Original unchanged
    }

    [Fact]
    public void Settings_WithRecursive_UpdatesCorrectly()
    {
        var settings = new Settings();
        var updated = settings with { Recursive = true };

        Assert.True(updated.Recursive);
    }

    [Fact]
    public void Settings_WithCommand_UpdatesCorrectly()
    {
        var settings = new Settings();
        var updated = settings with { Command = "git status" };

        Assert.Equal("git status", updated.Command);
    }

    [Fact]
    public void Settings_WithFilters_UpdatesCorrectly()
    {
        var settings = new Settings();
        var updated = settings with 
        { 
            FolderInclusionFilter = "Test",
            FolderExclusionFilter = "Example"
        };

        Assert.Equal("Test", updated.FolderInclusionFilter);
        Assert.Equal("Example", updated.FolderExclusionFilter);
    }

    [Fact]
    public void Settings_WithFileFilters_UpdatesCorrectly()
    {
        var settings = new Settings();
        var updated = settings with 
        { 
            FileInclusionFilter = "package.json",
            FileExclusionFilter = ".lock"
        };

        Assert.Equal("package.json", updated.FileInclusionFilter);
        Assert.Equal(".lock", updated.FileExclusionFilter);
    }

    [Fact]
    public void Settings_ToString_ContainsAllValues()
    {
        var settings = new Settings(
            GitOnly: true,
            Recursive: true,
            Command: "git status",
            FolderInclusionFilter: "Test",
            FolderExclusionFilter: "Example",
            FileInclusionFilter: "package.json",
            FileExclusionFilter: ".lock"
        );

        var result = settings.ToString();

        Assert.Contains("GitOnly", result);
        Assert.Contains("Recursive", result);
        Assert.Contains("git status", result);
        Assert.Contains("Test", result);
        Assert.Contains("Example", result);
        Assert.Contains("package.json", result);
        Assert.Contains(".lock", result);
    }
}
