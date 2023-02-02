using System;
using System.Collections.Generic;
using System.Text;
using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models.Features;
using MCE_API_SERVER.Utils;
using Newtonsoft.Json;
using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
	[ServerHandleContainer]
	public static class TappablesController
	{
		[ServerHandle("/1/api/v1.1/tappables/{x}_{y}", Types = new string[] { "POST" })]
		public static byte[] Post(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            string body = args.Content;

            TappableRequest req = JsonConvert.DeserializeObject<TappableRequest>(body);

            Models.Player.TappableResponse response = TappableUtils.RedeemTappableForPlayer(authtoken, req);

			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}
	}
}
