using System.Collections.Generic;
using System;
using Godot;

namespace HiddenWatts.Scripts.Core;

public static class Logger
{
	private static readonly Dictionary<LogLevel, string> colorMap = new()
	{
		{ LogLevel.Debug, "WHITE" },
		{ LogLevel.Info, "CYAN" },
		{ LogLevel.Warning, "YELLOW" },
		{ LogLevel.Error, "RED" }
	};

	private static void Log(LogLevel level, params object[] message)
	{
		string timeStamp = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]";
		var callingMethod = new System.Diagnostics.StackTrace().GetFrame(2)?.GetMethod();
		string logMessage = $"{timeStamp} [{level}] [{callingMethod?.DeclaringType?.Name}] [{callingMethod?.Name}]";
		GD.PrintRich([$"[color={colorMap[level]}]{logMessage}[/color]", .. message]);
	}

	public static void Debug(params object[] message)
	{
		Log(LogLevel.Debug, message);
	}

	public static void Info(params object[] message)
	{
		Log(LogLevel.Info, message);
	}

	public static void Warning(params object[] message)
	{
		Log(LogLevel.Warning, message);
	}

	public static void Error(params object[] message)
	{
		Log(LogLevel.Error, message);
	}
}
public enum LogLevel
{
	Debug,
	Info,
	Warning,
	Error
}
