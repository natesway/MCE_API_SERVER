using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Login;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Utils;
using Newtonsoft.Json;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
    [ServerHandleContainer]
    public static class SigninController
    {
        [ServerHandle("/1/api/v1.1/player/profile/{profileID}", Types = new string[] { "GET" })]
        public static byte[] Get(ServerHandleArgs args)
        {
            ProfileResponse response = new ProfileResponse(ProfileUtils.ReadProfile(args.UrlArgs["profileID"]));
            return Content(args, JsonConvert.SerializeObject(response), "application/json");
        }

        [ServerHandle("/api/v1.1/player/profile/{profileID}", Types = new string[] { "POST" })]
        public static byte[] Post(ServerHandleArgs args)
        {
            if (args.UrlArgs["profileID"] != "signin")
                return BadRequest();

            SigninRequest request = JsonConvert.DeserializeObject<SigninRequest>(args.Content);

            string playerid = request.sessionTicket.Split("-")[0];

            SigninResponse.ResponseTemplate response = new SigninResponse.ResponseTemplate()
            {
                result = new SigninResponse.Result()
                {
                    AuthenticationToken = playerid,
                    BasePath = "/1",
                    Tokens = TokenUtils.GetSigninTokens(playerid),
                    ClientProperties = new object(),
                    Updates = new Updates()
                },
                updates = new Updates()
            };

            string resp = JsonConvert.SerializeObject(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            Log.Information($"[{playerid}]: Logged in.");

            return Content(args, resp, "application/json");
        }
    }
}
