using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SnowyBot.Services;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
  public class AudioModule : ModuleBase<SocketCommandContext>
  {
    public LavaLinkAudio AudioService { get; set; }

    [Command("Join")]
    public async Task JoinAndPlay()
        => await ReplyAsync(embed: await AudioService.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel).ConfigureAwait(false)).ConfigureAwait(false);

    [Command("Leave")]
    public async Task Leave()
        => await ReplyAsync(embed: await AudioService.LeaveAsync(Context.Guild).ConfigureAwait(false)).ConfigureAwait(false);

    [Command("Play")]
    public async Task Play([Remainder] string search)
        => await ReplyAsync(embed: await AudioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search).ConfigureAwait(false)).ConfigureAwait(false);

    [Command("Stop")]
    public async Task Stop()
        => await ReplyAsync(embed: await AudioService.StopAsync(Context.Guild).ConfigureAwait(false)).ConfigureAwait(false);

    [Command("List")]
    public async Task List()
        => await ReplyAsync(embed: await AudioService.ListAsync(Context.Guild).ConfigureAwait(false)).ConfigureAwait(false);

    [Command("Skip")]
    public async Task Skip()
        => await ReplyAsync(embed: await AudioService.SkipTrackAsync(Context.Guild).ConfigureAwait(false)).ConfigureAwait(false);

    [Command("Volume")]
    public async Task Volume(int volume)
        => await ReplyAsync(await AudioService.SetVolumeAsync(Context.Guild, volume).ConfigureAwait(false)).ConfigureAwait(false);

    [Command("Pause")]
    public async Task Pause()
        => await ReplyAsync(await AudioService.PauseAsync(Context.Guild).ConfigureAwait(false)).ConfigureAwait(false);

    [Command("Resume")]
    public async Task Resume()
        => await ReplyAsync(await AudioService.ResumeAsync(Context.Guild).ConfigureAwait(false)).ConfigureAwait(false);
  }
}
