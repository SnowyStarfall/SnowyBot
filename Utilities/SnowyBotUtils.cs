using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyBot
{
  public static class SnowyBotUtils
  {
    public static bool ValidCommand(string command)
    {
      command = command.ToLower();
      switch (command)
      {
        case "join":
        case "play":
        case "list":
        case "pause":
        case "resume":
        case "playing":
        case "seek":
        case "jump":
        case "loop":
        case "qremove":
        case "shuffle":
        case "volume":
        case "stop":
        case "leave":
        case "question":
        case "8ball":
        case "a":
        case "info":
        case "ratewaifu":
        case "jumbo":
        case "awoo":
        case "char add":
        case "char view":
        case "char delete":
        case "prefix":
          return true;
        default:
          return false;
      }
    }
  }
}
