﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SnowyBot.Handlers
{
  public class CommandHandler
  {
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    /* Get Everything we need from DI. */
    public CommandHandler(IServiceProvider services)
    {
      _commands = services.GetRequiredService<CommandService>();
      _client = services.GetRequiredService<DiscordSocketClient>();
      _services = services;

      HookEvents();
    }

    /* Initialize the CommandService. */
    public async Task InitializeAsync()
    {
      await _commands.AddModulesAsync(
          assembly: Assembly.GetEntryAssembly(),
          services: _services);
    }

    /* Hook Command Specific Events. */
    public void HookEvents()
    {
      //_commands.CommandExecuted += CommandExecutedAsync;
      _commands.Log += LogAsync;
      _client.MessageReceived += HandleCommandAsync;
    }

    /* When a MessageRecived Event triggers from the Client.
          Handle the message here. */
    private Task HandleCommandAsync(SocketMessage socketMessage)
    {
      var argPos = 0;
      if (!(socketMessage is SocketUserMessage message) || message.Author.IsBot || message.Author.IsWebhook || message.Channel is IPrivateChannel)
        return Task.CompletedTask;

      if (!message.HasStringPrefix(GlobalData.Config.DefaultPrefix, ref argPos))
        return Task.CompletedTask;

      var context = new SocketCommandContext(_client, socketMessage as SocketUserMessage);

      var blacklistedChannelCheck = from a in GlobalData.Config.BlacklistedChannels
                                    where a == context.Channel.Id
                                    select a;
      var blacklistedChannel = blacklistedChannelCheck.FirstOrDefault();

      if (blacklistedChannel == context.Channel.Id)
      {
        return Task.CompletedTask;
      }
      else
      {
        _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);
        return Task.CompletedTask;
      }
    }

    //public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    //{
    //  /* command is unspecified when there was a search failure (command not found); we don't care about these errors */
    //  if (!command.IsSpecified)
    //    return;

    //  /* the command was succesful, we don't care about this result, unless we want to log that a command succeeded. */
    //  if (result.IsSuccess)
    //    return;

    //  /* the command failed, let's notify the user that something happened. */
    //  await context.Channel.SendMessageAsync($"error: {result}");
    //}

    /*Used whenever we want to log something to the Console. 
        Todo: Hook in a Custom LoggingService. */
    private Task LogAsync(LogMessage log)
    {
      Console.WriteLine(log.ToString());

      return Task.CompletedTask;
    }
  }
}
