using System;
using System.Collections.Generic;
using System.Text;
using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models.Multiplayer.Adventure;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Utils;
using Newtonsoft.Json;
using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
	[ServerHandleContainer]
	public static class AdventureScrollsController
	{
		[ServerHandle("/1/api/v1.1/adventures/scrolls")]
		public static byte[] Get(ServerHandleArgs args)
		{
            ScrollsResponse responseobj = new ScrollsResponse();
            string response = JsonConvert.SerializeObject(responseobj);
			return Content(args, response, "application/json");
		} // TODO: Fixed String

		[ServerHandle("/1/api/v1.1/adventures/scrolls/{crystalId}")]
		public static byte[] PostRedeemCrystal(ServerHandleArgs args)
		{
            string playerId = args.Headers["Authorization"];

            string body = args.Content;

            PlayerAdventureRequest req = JsonConvert.DeserializeObject<PlayerAdventureRequest>(body);
            AdventureRequestResult resp = AdventureUtils.RedeemCrystal(playerId, req, Guid.Parse(args.UrlArgs["crystalId"]));

			return Content(args, JsonConvert.SerializeObject(resp), "application/json");
		}
	}
}
