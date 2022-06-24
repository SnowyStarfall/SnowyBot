﻿using SnowyBot.Containers;
using SnowyBot.Services;
using SnowyBot.Structs;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Victoria;

namespace SnowyBot
{
	public static class Program
	{
		private static ConsoleEventDelegate handler;
		private delegate bool ConsoleEventDelegate(int eventType);
		private static async Task Main()
		{
			handler = new ConsoleEventDelegate(ConsoleEventCallback);
			SetConsoleCtrlHandler(handler, true);
			await DiscordGlobal.InitializeAsync().ConfigureAwait(false);
		}
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
		private static bool ConsoleEventCallback(int eventType)
		{
			if (eventType == 2)
			{
				Console.WriteLine("Shutting down...");

				if (DiscordGlobal.lavaNode.Players.Any())
				{
					Console.WriteLine("Saving player data...");

					LavaTable table = new();

					foreach (LavaPlayer player in DiscordGlobal.lavaNode.Players)
					{
						LavaEntry data = new();
						data.Configure(player);
						table.table.TryAdd(player.TextChannel.GuildId, data);
					}

					LavaTable.WriteToBinaryFile("C:/Users/Snowy/Documents/My Games/Terraria/ModLoader/Mod Sources/SnowyBot/Database/LavaNodeData.lava", table);
				}
			}
			return false;
		}
	}
}
