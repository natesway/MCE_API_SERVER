using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Utils;
using System.Text;
using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
    [ServerHandleContainer]
    public static class LocationController
    {
        [ServerHandle("/1/api/v1.1/locations/{latitude}/{longitude}")]
        public static HttpResponse Get(ServerHandleArgs args)
        {
            //Create our response
            LocationResponse.Root resp = TappableUtils.GetActiveLocations(double.Parse(args.UrlArgs["latitude"]), double.Parse(args.UrlArgs["longitude"]));

            //Send
            return Content(args, Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize(resp)), "application/json");
            //return Content(args, JsonConvert.SerializeObject(resp), "application/json");
        }
    }
}
