namespace Dotnet.MultiCommand.Core;
public record Settings(
	bool GitOnly = false,
	bool Recursive = false,
	string Command = "ls"
);