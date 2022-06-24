using Discord;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnowyBot.Services
{
	public static class LoggingGlobal
	{
		public static FileStream LogStream;
		static LoggingGlobal()
		{
			DateTime date = DateTime.Now;
			string path = Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.IndexOf("SnowyBot") + 9);
			string dateString = "Log-" + date.Day + "-" + date.Month + "-" + date.Year + "_";
			DirectoryInfo info = new(path + "Logs/");
			int counter = 1;
			foreach (FileInfo file in info.GetFiles())
			{
				if (file.Name.Contains(dateString))
					counter++;
			}
			dateString += counter;
			LogStream = new(path + "Logs/" + dateString + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
		}

		public static async Task LogAsync(string src, LogSeverity severity, string message, Exception exception = null)
		{
			if (severity.Equals(null))
				severity = LogSeverity.Warning;

			await Append(GetSeverityString(severity), GetConsoleColor(severity)).ConfigureAwait(false);
			await Append($" [{SourceToString(src)}] ", ConsoleColor.DarkGray).ConfigureAwait(false);

			if (!string.IsNullOrWhiteSpace(message))
				await Append($"{message}\n", ConsoleColor.White).ConfigureAwait(false);
			else if (exception == null)
				await Append($"Unknown Exception. Exception Returned Null. Source: {src} Severity: {severity}\n", ConsoleColor.DarkRed).ConfigureAwait(false);
			else if (exception.Message == null)
				await Append($"Unknown \n{exception.StackTrace}\n", GetConsoleColor(severity)).ConfigureAwait(false);
			else
				await Append($"{exception.Message ?? "Unknown"}\n{exception.StackTrace ?? "Unknown"}\n", GetConsoleColor(severity)).ConfigureAwait(false);
		}
		private static async Task Append(string message, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.Write(message);
			await LogStream.WriteAsync(new UTF8Encoding(true).GetBytes(message));
		}
		private static string SourceToString(string src)
		{
			return src.ToLower() switch
			{
				"discord" => "DISC",
				"admin" => "ADMN",
				"victoria" => "VICT",
				"audio" => "ADIO",
				"gateway" => "GTWY",
				"bot" => "RBOT",
				"rest" => "REST",
				"timer" => "TIMR",
				"resume" => "RSUM",
				"nickname" => "NICK",
				_ => src
			};
		}
		private static string GetSeverityString(LogSeverity severity)
		{
			return severity switch
			{
				LogSeverity.Critical => "CRIT",
				LogSeverity.Error => "EROR",
				LogSeverity.Warning => "WARN",
				LogSeverity.Info => "INFO",
				LogSeverity.Verbose => "VERB",
				LogSeverity.Debug => "DBUG",
				_ => "UNKN"
			};
		}
		private static ConsoleColor GetConsoleColor(LogSeverity severity)
		{
			return severity switch
			{
				LogSeverity.Critical => ConsoleColor.Red,
				LogSeverity.Error => ConsoleColor.DarkRed,
				LogSeverity.Warning => ConsoleColor.Yellow,
				LogSeverity.Info => ConsoleColor.Green,
				LogSeverity.Verbose => ConsoleColor.Blue,
				LogSeverity.Debug => ConsoleColor.DarkBlue,
				_ => ConsoleColor.White
			};
		}
	}
}
