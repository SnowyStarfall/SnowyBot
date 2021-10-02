using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SnowyBot.Services
{
  /* A Static Logging Service So it Can Be Used Throughout The Whole Bot Anywhere We Want. */
  public static class LoggingService
  {
    /* The Standard Way Log */
    public static async Task LogAsync(string src, LogSeverity severity, string message, Exception exception = null)
    {
      if (severity.Equals(null))
      {
        severity = LogSeverity.Warning;
      }
      await Append($"{GetSeverityString(severity)}", GetConsoleColor(severity)).ConfigureAwait(false);
      await Append($" [{SourceToString(src)}] ", ConsoleColor.DarkGray).ConfigureAwait(false);

      if (!string.IsNullOrWhiteSpace(message))
        await Append($"{message}\n", ConsoleColor.White).ConfigureAwait(false);
      else if (exception == null)
      {
        await Append("Uknown Exception. Exception Returned Null.\n", ConsoleColor.DarkRed).ConfigureAwait(false);
      }
      else if (exception.Message == null)
        await Append($"Unknown \n{exception.StackTrace}\n", GetConsoleColor(severity)).ConfigureAwait(false);
      else
        await Append($"{exception.Message ?? "Unknown"}\n{exception.StackTrace ?? "Unknown"}\n", GetConsoleColor(severity)).ConfigureAwait(false);
    }

    /* The Way To Log Critical Errors*/
    public static async Task LogCriticalAsync(string source, string message, Exception exc = null)
        => await LogAsync(source, LogSeverity.Critical, message, exc).ConfigureAwait(false);

    /* The Way To Log Basic Infomation */
    public static async Task LogInformationAsync(string source, string message)
        => await LogAsync(source, LogSeverity.Info, message).ConfigureAwait(false);

    /* Format The Output */
    private static async Task Append(string message, ConsoleColor color)
    {
      await Task.Run(() =>
      {
        Console.ForegroundColor = color;
        Console.Write(message);
      }).ConfigureAwait(false);
    }

    /* Swap The Normal Source Input To Something Neater */
    private static string SourceToString(string src)
    {
      return src.ToLower() switch
      {
        "discord" => "DISCD",
        "victoria" => "VICTR",
        "audio" => "AUDIO",
        "admin" => "ADMIN",
        "gateway" => "GTWAY",
        "blacklist" => "BLAKL",
        "lavanode_0_socket" => "LAVAS",
        "lavanode_0" => "LAVA#",
        "bot" => "BOTWN",
        _ => src,
      };
    }

    /* Swap The Severity To a String So We Can Output It To The Console */
    private static string GetSeverityString(LogSeverity severity)
    {
      return severity switch
      {
        LogSeverity.Critical => "CRIT",
        LogSeverity.Debug => "DBUG",
        LogSeverity.Error => "EROR",
        LogSeverity.Info => "INFO",
        LogSeverity.Verbose => "VERB",
        LogSeverity.Warning => "WARN",
        _ => "UNKN",
      };
    }

    /* Return The Console Color Based On Severity Selected */
    private static ConsoleColor GetConsoleColor(LogSeverity severity)
    {
      return severity switch
      {
        LogSeverity.Critical => ConsoleColor.Red,
        LogSeverity.Debug => ConsoleColor.Magenta,
        LogSeverity.Error => ConsoleColor.DarkRed,
        LogSeverity.Info => ConsoleColor.Green,
        LogSeverity.Verbose => ConsoleColor.DarkCyan,
        LogSeverity.Warning => ConsoleColor.Yellow,
        _ => ConsoleColor.White,
      };
    }
  }
}
