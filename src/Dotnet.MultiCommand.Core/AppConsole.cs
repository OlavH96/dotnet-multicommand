using System;
using System.IO;

namespace Dotnet.MultiCommand.Core;

public class AppConsole(TextWriter stdOut, TextWriter stdErr)
{
	private readonly TextWriter stdOut = stdOut;
	private readonly TextWriter stdErr = stdErr;
	public static AppConsole Default = new AppConsole(Console.Out, Console.Error);
	public bool Verbose = false;

	public void WriteError(string value)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		stdOut.WriteLine(value.TrimEnd(Environment.NewLine.ToCharArray()));
		Console.ResetColor();
	}

	public void WriteSuccess(string value)
	{
		Console.ForegroundColor = ConsoleColor.Green;
		stdOut.WriteLine(value.TrimEnd(Environment.NewLine.ToCharArray()));
		Console.ResetColor();
	}

	public void WriteHighlighted(string value)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		stdOut.WriteLine(value.TrimEnd(Environment.NewLine.ToCharArray()));
		Console.ResetColor();
	}

	public void WriteHeader(string value)
	{
		Console.ForegroundColor = ConsoleColor.Magenta;
		stdOut.WriteLine(value.TrimEnd(Environment.NewLine.ToCharArray()));
		Console.ResetColor();
	}

	public void WriteEmptyLine()
	{
		stdOut.WriteLine();
	}

	public void WriteNormal(string value)
	{
		stdOut.WriteLine(value.TrimEnd(Environment.NewLine.ToCharArray()));
	}
	public void WriteVerbose(string value)
	{
		if(!Verbose)
		{
			return;
		}
		stdOut.WriteLine(value.TrimEnd(Environment.NewLine.ToCharArray()));
	}
}