using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Features;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Utils;
using Newtonsoft.Json;
using System;
using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
    // TODO: Not done. Rewards need inventory implementation, timers for smelting process, and recipeId -> recipe time checks
    [ServerHandleContainer]
    public static class SmeltingController
    {
        [ServerHandle("/1/api/v1.1/smelting/{slot}/start")]
        public static HttpResponse PostNewSmeltingJob(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];

            string body = args.Content;

            SmeltingRequest req = JsonConvert.DeserializeObject<SmeltingRequest>(body);

            bool smeltingJob = SmeltingUtils.StartSmeltingJob(authtoken, int.Parse(args.UrlArgs["slot"]), req);

            UpdateResponse updateResponse = new UpdateResponse { updates = new Updates() };

            uint nextStreamId = GetNextStreamVersion();

            updateResponse.updates.smelting = nextStreamId;
            updateResponse.updates.inventory = nextStreamId;

            return Content(args, JsonConvert.SerializeObject(updateResponse), "application/json");
        }

        [ServerHandle("/1/api/v1.1/smelting/finish/price")]
        public static HttpResponse GetSmeltingPrice(ServerHandleArgs args)
        {
            TimeSpan remainingTime = TimeSpan.Parse(args.Query["remainingTime"]);
            CraftingPriceResponse returnPrice = new CraftingPriceResponse { result = new CraftingPrice { cost = 1, discount = 0, validTime = remainingTime }, updates = new Updates() };

            return Content(args, JsonConvert.SerializeObject(returnPrice), "application/json");
        }

        [ServerHandle("/1/api/v1.1/smelting/{slot}/finish")]
        public static HttpResponse PostSmeltingFinish(ServerHandleArgs args)
        {
            string body = args.Content;

            FinishCraftingJobRequest req = JsonConvert.DeserializeObject<FinishCraftingJobRequest>(body);

            SplitRubyResponse result = SmeltingUtils.FinishSmeltingJobNow(args.Headers["Authorization"], int.Parse(args.UrlArgs["slot"]), req.expectedPurchasePrice);
            return Content(args, JsonConvert.SerializeObject(result), "application/json");
        }

        [ServerHandle("/1/api/v1.1/smelting/{slot}")]
        public static HttpResponse GetSmeltingStatus(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];

            SmeltingSlotResponse smeltingStatus = SmeltingUtils.GetSmeltingJobInfo(authtoken, int.Parse(args.UrlArgs["slot"]));

            return Content(args, JsonConvert.SerializeObject(smeltingStatus), "application/json");
        }

        [ServerHandle("/1/api/v1.1/smelting/{slot}/collectItems")]
        public static HttpResponse GetCollectSmeltingItems(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];

            CollectItemsResponse returnUpdates = SmeltingUtils.FinishSmeltingJob(authtoken, int.Parse(args.UrlArgs["slot"]));

            return Content(args, JsonConvert.SerializeObject(returnUpdates), "application/json");
        }

        [ServerHandle("/1/api/v1.1/smelting/{slot}/stop")]
        public static HttpResponse GetStopSmeltingJob(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];

            bool returnUpdates = SmeltingUtils.CancelSmeltingJob(authtoken, int.Parse(args.UrlArgs["slot"]));

            return Accepted();
        }

        [ServerHandle("/1/api/v1.1/smelting/{slot}/unlock")]
        public static HttpResponse GetUnlockSmeltingSlot(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];

            CraftingUpdates returnUpdates = SmeltingUtils.UnlockSmeltingSlot(authtoken, int.Parse(args.UrlArgs["slot"]));

            return Content(args, JsonConvert.SerializeObject(returnUpdates), "application/json");
        }
    }
}
