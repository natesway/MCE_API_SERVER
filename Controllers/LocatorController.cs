using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
    [ServerHandleContainer]
    public static class LocatorController
    {
        [ServerHandle("/player/environment", "/api/v1.1/player/environment")]
        public static byte[] Get(ServerHandleArgs args)
        {
            string replyIP = StateSingleton.config.useBaseServerIP ? StateSingleton.config.baseServerIP :
                $"{"http://"/*StateSingleton.config.protocol*/}{args.Sender.Address}:{args.Sender.Port}";

            Log.Information($"{args.Sender} has issued locator, replying with {replyIP}");

            LocatorResponse.Root response = new LocatorResponse.Root()
            {
                result = new LocatorResponse.Result()
                {
                    serviceEnvironments = new LocatorResponse.ServiceEnvironments()
                    {
                        production = new LocatorResponse.Production()
                        {
                            playfabTitleId = StateSingleton.config.playfabTitleId,
                            serviceUri = replyIP,
                            cdnUri = replyIP + "/cdn",
                            //playfabTitleId = "F0DE2" //maybe make our own soon? - Mojang could kill this anytime after server sunset with no warning. 
                        }
                    },
                    supportedEnvironments = new Dictionary<string, List<string>>() { { "2020.1217.02", new List<string>() { "production" } } }
                },
                //updates = new LocatorResponse.Updates()
            };
            return Content(args, JsonConvert.SerializeObject(response), "application/json");
        }
    }
}
