using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SnowyBot.Utilities;

namespace SnowyBot
{
	public class Paginator
	{
		public readonly List<Embed> pages;
		public readonly IUserMessage message;
		public ComponentBuilder builder;
		public string[] componentData;
		public readonly int count;
		public int page;
		public int timer;
		public Paginator(List<Embed> embeds, IUserMessage message, string[] componentData, int timer)
		{
			pages = embeds;
			this.message = message;
			this.componentData = componentData;
			this.timer = timer;
			count = pages.Count;
			builder = new();
		}

		public async Task<bool> NextPage()
		{
			if (page == count - 1)
				return false;
			await message.ModifyAsync((MessageProperties p) =>
			{
				p.Embed = pages[page + 1];
				builder.WithButton(null, componentData[0], ButtonStyle.Secondary, Emote.Parse(SnowyRewind));
				builder.WithButton(null, componentData[1], ButtonStyle.Secondary, Emote.Parse(SnowyPlayBackwards));
				if (page + 1 != count - 1)
				{
					builder.WithButton(null, componentData[2], ButtonStyle.Secondary, Emote.Parse(SnowyPlay));
					builder.WithButton(null, componentData[3], ButtonStyle.Secondary, Emote.Parse(SnowyFastForward));
				}
				p.Components = builder.Build();
			}).ConfigureAwait(false);
			builder = new();
			page++;
			return true;
		}
		public async Task<bool> PreviousPage()
		{
			if (page == 0)
				return false;
			await message.ModifyAsync((MessageProperties p) =>
			{
				p.Embed = pages[page - 1];
				if (page - 1 != 0)
				{
					builder.WithButton(null, componentData[0], ButtonStyle.Secondary, Emote.Parse(SnowyRewind));
					builder.WithButton(null, componentData[1], ButtonStyle.Secondary, Emote.Parse(SnowyPlayBackwards));
				}
				builder.WithButton(null, componentData[2], ButtonStyle.Secondary, Emote.Parse(SnowyPlay));
				builder.WithButton(null, componentData[3], ButtonStyle.Secondary, Emote.Parse(SnowyFastForward));
				p.Components = builder.Build();
			}).ConfigureAwait(false);
			builder = new();
			page--;
			return true;
		}
		public async Task Forward3Pages()
		{
			if (page >= count)
				page = count;
			await message.ModifyAsync((MessageProperties p) =>
			{
				p.Embed = pages[page + 3 < pages.Count - 1 ? page + 3 : pages.Count - 1];
				builder.WithButton(null, componentData[0], ButtonStyle.Secondary, Emote.Parse(SnowyRewind));
				builder.WithButton(null, componentData[1], ButtonStyle.Secondary, Emote.Parse(SnowyPlayBackwards));
				if (page + 3 < count - 1)
				{
					builder.WithButton(null, componentData[2], ButtonStyle.Secondary, Emote.Parse(SnowyPlay));
					builder.WithButton(null, componentData[3], ButtonStyle.Secondary, Emote.Parse(SnowyFastForward));
				}
				p.Components = builder.Build();
			}).ConfigureAwait(false);
			builder = new();
			page = page + 3 > count ? count : page + 3;
		}
		public async Task Backward3Pages()
		{
			if (page <= 0)
				page = 0;
			await message.ModifyAsync((MessageProperties p) =>
			{
				p.Embed = pages[page - 3 < 0 ? 0 : page - 3];
				if (page - 3 > 1)
				{
					builder.WithButton(null, componentData[0], ButtonStyle.Secondary, Emote.Parse(SnowyRewind));
					builder.WithButton(null, componentData[1], ButtonStyle.Secondary, Emote.Parse(SnowyPlayBackwards));
				}
				builder.WithButton(null, componentData[2], ButtonStyle.Secondary, Emote.Parse(SnowyPlay));
				builder.WithButton(null, componentData[3], ButtonStyle.Secondary, Emote.Parse(SnowyFastForward));
				p.Components = builder.Build();
			}).ConfigureAwait(false);
			builder = new();
			page = page - 3 < 1 ? 1 : page - 3;
		}
	}
}
