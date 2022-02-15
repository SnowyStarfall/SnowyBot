using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SnowyBot.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SnowyBot
{
	public static partial class SnowyBotUtils
	{
		public class ImgurResult
		{
			[JsonProperty("link")]
			public string Url { get; set; }
			public string Title { get; set; }
			public string Id { get; set; }
			public string Description { get; set; }
			[JsonProperty("is_album")]
			public bool IsAlbum { get; set; }
		}
		public static async Task<IEnumerable<ImgurResult>> SearchAsync(string query)
		{
			HttpClient client = new();
			var resource = $"https://api.imgur.com/3/gallery/search/top?q={query}";
			var request = new HttpRequestMessage(HttpMethod.Get, resource);
			request.Headers.Add("Authorization", $"Client-ID {DiscordService.config.ImgurToken}");

			var response = await client.SendAsync(request).ConfigureAwait(false);
			var parsedResponse = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

			if (!parsedResponse["data"].HasValues)
				return null;

			return parsedResponse["data"]
					.Select(d => d.ToObject<ImgurResult>())
					.Where(r => !r.IsAlbum);
		}
	}
}
