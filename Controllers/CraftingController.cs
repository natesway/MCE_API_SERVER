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
    [ServerHandleContainer]
    public static class CraftingController
    {
        [ServerHandle("/1/api/v1.1/crafting/{slot}/start")]
        public static byte[] PostNewCraftingJob(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            int slot = int.Parse(args.UrlArgs["slot"]);

            string body = args.Content;

            CraftingRequest req = JsonConvert.DeserializeObject<CraftingRequest>(body);

            bool craftingJob = CraftingUtils.StartCraftingJob(authtoken, slot, req);


            CraftingUpdates updateResponse = new CraftingUpdates { updates = new Updates() };

            uint nextStreamId = GetNextStreamVersion();

            updateResponse.updates.crafting = nextStreamId;
            updateResponse.updates.inventory = nextStreamId;

            return Content(args, JsonConvert.SerializeObject(updateResponse), "application/json");
            //return Accepted(Content(returnUpdates, "application/json"));
        }

        [ServerHandle("/1/api/v1.1/crafting/finish/price")]
        public static byte[] GetCraftingPrice(ServerHandleArgs args)
        {
            TimeSpan remainingTime = TimeSpan.Parse(args.Query["remainingTime"]);
            CraftingPriceResponse returnPrice = new CraftingPriceResponse { result = new CraftingPrice { cost = 1, discount = 0, validTime = remainingTime }, updates = new Updates() };

            return Content(args, JsonConvert.SerializeObject(returnPrice), "application/json");
        }

        [ServerHandle("/1/api/v1.1/crafting/{slot}/finish")]
        public static byte[] PostCraftingFinish(ServerHandleArgs args)
        {
            string body = args.Content;
            int slot = int.Parse(args.UrlArgs["slot"]);

            FinishCraftingJobRequest req = JsonConvert.DeserializeObject<FinishCraftingJobRequest>(body);

            SplitRubyResponse result = CraftingUtils.FinishCraftingJobNow(args.Headers["Authorization"], slot, req.expectedPurchasePrice);
            return Content(args, JsonConvert.SerializeObject(result), "application/json");
        }

        [ServerHandle("/1/api/v1.1/crafting/{slot}")]
        public static byte[] GetCraftingStatus(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            int slot = int.Parse(args.UrlArgs["slot"]);

            CraftingSlotResponse craftingStatus = CraftingUtils.GetCraftingJobInfo(authtoken, slot);

            return Content(args, JsonConvert.SerializeObject(craftingStatus), "application/json");
            //return Accepted(Content(returnTokens, "application/json"));
        }

        [ServerHandle("/1/api/v1.1/crafting/{slot}/collectItems")]
        public static byte[] GetCollectCraftingItems(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            int slot = int.Parse(args.UrlArgs["slot"]);

            CollectItemsResponse returnUpdates = CraftingUtils.FinishCraftingJob(authtoken, slot);

            return Content(args, JsonConvert.SerializeObject(returnUpdates), "application/json");
            //return Accepted(Content(returnTokens, "application/json"));
        }

        [ServerHandle("/1/api/v1.1/crafting/{slot}/stop")]
        public static byte[] GetStopCraftingJob(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            int slot = int.Parse(args.UrlArgs["slot"]);

            CraftingSlotResponse returnUpdates = CraftingUtils.CancelCraftingJob(authtoken, slot);

            //return Accepted();

            return Content(args, JsonConvert.SerializeObject(returnUpdates), "application/json");
            //return Accepted(Content(returnTokens, "application/json"));
        }

        [ServerHandle("/1/api/v1.1/crafting/{slot}/unlock")]
        public static byte[] GetUnlockCraftingSlot(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            int slot = int.Parse(args.UrlArgs["slot"]);

            CraftingUpdates returnUpdates = CraftingUtils.UnlockCraftingSlot(authtoken, slot);

            return Content(args, JsonConvert.SerializeObject(returnUpdates), "application/json");
        }
    }
}
