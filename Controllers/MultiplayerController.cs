using System;
using System.Collections.Generic;
using System.Text;
using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models.Buildplate;
using MCE_API_SERVER.Models.Multiplayer;
using MCE_API_SERVER.Models.Multiplayer.Adventure;
using MCE_API_SERVER.Utils;
using Newtonsoft.Json;
using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
    [ServerHandleContainer]
    public static class MultiplayerController
    {
		#region Buildplates

		[ServerHandle("/1/api/v1.1/multiplayer/buildplate/{buildplateId}/instances")]
		public static byte[] PostCreateInstance(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            string body = args.Content;
            BuildplateServerRequest parsedRequest = JsonConvert.DeserializeObject<BuildplateServerRequest>(body);

            BuildplateServerResponse response = MultiplayerUtils.CreateBuildplateInstance(authtoken, args.UrlArgs["buildplateId"], parsedRequest.playerCoordinate).Result;
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/multiplayer/buildplate/{buildplateId}/play/instances")]
		public static byte[] PostCreatePlayInstance(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            string body = args.Content;
            BuildplateServerRequest parsedRequest = JsonConvert.DeserializeObject<BuildplateServerRequest>(body);

            BuildplateServerResponse response = MultiplayerUtils.CreateBuildplatePlayInstance(authtoken, args.UrlArgs["buildplateId"], parsedRequest.playerCoordinate).Result;
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/buildplates")]
		public static byte[] GetBuildplates(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];

            BuildplateListResponse response = BuildplateUtils.GetBuildplatesList(authtoken);
			return Content(args, Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize(response)), "application/json");
			//return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/buildplates/{buildplateId}/share")]
		public static byte[] ShareBuildplate(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            ShareBuildplateResponse response = BuildplateUtils.ShareBuildplate(Guid.Parse(args.UrlArgs["buildplateId"]), authtoken);
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/buildplates/shared/{buildplateId}")]
		public static byte[] GetSharedBuildplate(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            SharedBuildplateResponse response = BuildplateUtils.ReadSharedBuildplate(args.UrlArgs["buildplateId"]);
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/multiplayer/buildplate/shared/{buildplateId}/play/instances")]
		public static byte[] PostSharedBuildplateCreatePlayInstance(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            string body = args.Content;
            SharedBuildplateServerRequest parsedRequest = JsonConvert.DeserializeObject<SharedBuildplateServerRequest>(body);

            BuildplateServerResponse response = MultiplayerUtils.CreateSharedBuildplatePlayInstance(authtoken, args.UrlArgs["buildplateId"], parsedRequest.playerCoordinate).Result;
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/multiplayer/join/instances")]
		public static byte[] PostMultiplayerJoinInstance(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            string body = args.Content;
            MultiplayerJoinRequest parsedRequest = JsonConvert.DeserializeObject<MultiplayerJoinRequest>(body);
			Log.Information($"[{authtoken}]: Trying to join buildplate instance: id {parsedRequest.id}");

            BuildplateServerResponse response = MultiplayerUtils.GetServerInstance(parsedRequest.id);
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		#endregion

		[ServerHandle("/1/api/v1.1/multiplayer/encounters/{adventureid}/instances")]
		public static byte[] PostCreateEncounterInstance(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            string body = args.Content;
            EncounterServerRequest parsedRequest = JsonConvert.DeserializeObject<EncounterServerRequest>(body);

            BuildplateServerResponse response = MultiplayerUtils.CreateAdventureInstance(authtoken, args.UrlArgs["adventureid"], parsedRequest.playerCoordinate).Result;
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/multiplayer/adventures/{adventureid}/instances")]
		public static byte[] PostCreateAdventureInstance(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            string body = args.Content;
            BuildplateServerRequest parsedRequest = JsonConvert.DeserializeObject<BuildplateServerRequest>(body);

            BuildplateServerResponse response = MultiplayerUtils.CreateAdventureInstance(authtoken, args.UrlArgs["adventureid"], parsedRequest.playerCoordinate).Result;
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/multiplayer/encounters/state")]
		public static byte[] EncounterState(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];
            string body = args.Content;
            Dictionary<Guid, string> request = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(body);
            EncounterStateResponse response = new EncounterStateResponse { result = new Dictionary<Guid, ActiveEncounterStateMetadata> { { Guid.Parse("b7335819-c123-49b9-83fb-8a0ec5032779"), new ActiveEncounterStateMetadata { ActiveEncounterState = ActiveEncounterState.Dirty } } }, expiration = null, continuationToken = null, updates = null };
			return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/multiplayer/partitions/{worldId}/instances/{instanceId}")]
		public static byte[] GetInstanceStatus(ServerHandleArgs args)
		{
			string authtoken = args.Headers["Authorization"];

            BuildplateServerResponse response = MultiplayerUtils.CheckInstanceStatus(authtoken, Guid.Parse(args.UrlArgs["instanceId"]));
			if (response == null)
				return NoContent();
			else
				return Content(args, JsonConvert.SerializeObject(response), "application/json");
		}

		[ServerHandle("/1/api/v1.1/private/server/command")]
		public static byte[] PostServerCommand(ServerHandleArgs args)
		{
            string body = args.Content;
            ServerCommandRequest parsedRequest = JsonConvert.DeserializeObject<ServerCommandRequest>(body);

            string response = MultiplayerUtils.ExecuteServerCommand(parsedRequest);

			if (response == "ok") return Ok();
			else return Content(args, response, "application/json");
		}

		// idk what to do with it
		/*[ServerHandle("/1/api/v1.1/private/server/ws")]
		public static byte[] GetWebSocketServer()
		{
			if (HttpContext.WebSockets.IsWebSocketRequest) {
				var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
				await MultiplayerUtils.AuthenticateServer(webSocket);
			}
		}*/
	}
}
