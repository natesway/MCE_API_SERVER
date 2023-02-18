using MCE_API_SERVER.Attributes;
using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Login;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Controllers
{
    [ServerHandleContainer]
    public static class PlayerTokenController
    {
        [ServerHandle("/1/api/v1.1/player/tokens")]
        public static byte[] Get(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            TokenResponse returnTokens = TokenUtils.ReadTokens(authtoken);

            Log.Debug($"[{authtoken}]: Requested tokens."); // Debug since this is spammed a lot

            return Content(args, JsonConvert.SerializeObject(returnTokens), "application/json", "Cache-Control: max-age=11200");
        }

        [ServerHandle("/1/api/v1.1/player/tokens/{token}/redeem")] // TODO: Proper testing
        public static byte[] RedeemToken(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];

            Token redeemedToken = TokenUtils.RedeemToken(authtoken, new Guid(args.UrlArgs["token"]));

            return Content(args, JsonConvert.SerializeObject(redeemedToken), "application/json");
        }
    }

    [ServerHandleContainer]
    public static class PlayerRubiesController
    {
        [ServerHandle("/1/api/v1.1/player/rubies")]
        public static byte[] Get(ServerHandleArgs args)
        {
            RubyResponse obj = RubyUtils.GetNormalRubyResponse(args.Headers["Authorization"]);
            string response = JsonConvert.SerializeObject(obj);
            return Content(args, response, "application/json", "Cache-Control: max-age=11200");
        }
    }

    [ServerHandleContainer]
    public static class PlayerLanguageController
    {
        [ServerHandle("/1/api/v1.1/player/profile/language")]
        public static byte[] Get(ServerHandleArgs args)
        {
            return Ok();
        }
    }

    [ServerHandleContainer]
    public static class PlayerSplitRubiesController
    {
        [ServerHandle("/1/api/v1.1/player/splitRubies")]
        public static byte[] Get(ServerHandleArgs args)
        {
            SplitRubyResponse obj = RubyUtils.ReadRubies(args.Headers["Authorization"]);
            string response = JsonConvert.SerializeObject(obj);
            return Content(args, response, "application/json", "Cache-Control: max-age=11200");
        }
    }

    [ServerHandleContainer]
    public static class PlayerChallengesController
    {
        [ServerHandle("1/api/v1.1/player/challenges")]
        public static byte[] GetChallenges(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            ChallengesResponse challenges = ChallengeUtils.ReloadChallenges(authtoken);
            return Content(args, Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize(challenges)), "application/json");
            //return Content(args, JsonConvert.SerializeObject(challenges), "application/json");
        }

        [ServerHandle("1/api/v1.1/challenges/season/active/{challengeId}")]
        public static byte[] PutActivateChallenge(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            bool success = ChallengeUtils.ActivateChallengeForPlayer(authtoken, new Guid(args.UrlArgs["challengeId"]));

            if (success) return Ok();
            else return Unauthorized();
        }

        [ServerHandle("1/api/v1.1/challenges/timed/generate")]
        public static byte[] PostGenerateTimedChallenges(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            ChallengeUtils.GenerateTimedChallenges(authtoken);
            return Ok();
        }

        [ServerHandle("1/api/v1.1/challenges/continuous/{challengeId}/remove")]
        public static byte[] DeleteContinuousChallenge(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            ChallengeUtils.RemoveChallengeFromPlayer(authtoken, new Guid(args.UrlArgs["challengeId"]));
            return Ok();
        }
    }

    [ServerHandleContainer]
    public static class PlayerUtilityBlocksController
    {
        [ServerHandle("/1/api/v1.1/player/utilityBlocks")]
        public static byte[] Get(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            Models.Features.UtilityBlocksResponse response = UtilityBlockUtils.ReadUtilityBlocks(authtoken);
            return Content(args, JsonConvert.SerializeObject(response), "application/json");
            //return Content("{\"result\":{\"crafting\":{\"1\":{\"sessionId\":null,\"recipeId\":null,\"output\":null,\"escrow\":[],\"completed\":0,\"available\":0,\"total\":0,\"nextCompletionUtc\":null,\"totalCompletionUtc\":null,\"state\":\"Empty\",\"boostState\":null,\"unlockPrice\":null,\"streamVersion\":4},\"2\":{\"sessionId\":null,\"recipeId\":null,\"output\":null,\"escrow\":[],\"completed\":0,\"available\":0,\"total\":0,\"nextCompletionUtc\":null,\"totalCompletionUtc\":null,\"state\":\"Locked\",\"boostState\":null,\"unlockPrice\":{\"cost\":1,\"discount\":0},\"streamVersion\":4},\"3\":{\"sessionId\":null,\"recipeId\":null,\"output\":null,\"escrow\":[],\"completed\":0,\"available\":0,\"total\":0,\"nextCompletionUtc\":null,\"totalCompletionUtc\":null,\"state\":\"Locked\",\"boostState\":null,\"unlockPrice\":{\"cost\":1,\"discount\":0},\"streamVersion\":4}},\"smelting\":{\"1\":{\"fuel\":null,\"burning\":null,\"hasSufficientFuel\":null,\"heatAppliedToCurrentItem\":null,\"sessionId\":null,\"recipeId\":null,\"output\":null,\"escrow\":[],\"completed\":0,\"available\":0,\"total\":0,\"nextCompletionUtc\":null,\"totalCompletionUtc\":null,\"state\":\"Empty\",\"boostState\":null,\"unlockPrice\":null,\"streamVersion\":4},\"2\":{\"fuel\":null,\"burning\":null,\"hasSufficientFuel\":null,\"heatAppliedToCurrentItem\":null,\"sessionId\":null,\"recipeId\":null,\"output\":null,\"escrow\":[],\"completed\":0,\"available\":0,\"total\":0,\"nextCompletionUtc\":null,\"totalCompletionUtc\":null,\"state\":\"Locked\",\"boostState\":null,\"unlockPrice\":{\"cost\":1,\"discount\":0},\"streamVersion\":4},\"3\":{\"fuel\":null,\"burning\":null,\"hasSufficientFuel\":null,\"heatAppliedToCurrentItem\":null,\"sessionId\":null,\"recipeId\":null,\"output\":null,\"escrow\":[],\"completed\":0,\"available\":0,\"total\":0,\"nextCompletionUtc\":null,\"totalCompletionUtc\":null,\"state\":\"Locked\",\"boostState\":null,\"unlockPrice\":{\"cost\":1,\"discount\":0},\"streamVersion\":4}}},\"expiration\":null,\"continuationToken\":null,\"updates\":{}}", "application/json");
        }
    }

    [ServerHandleContainer]
    public static class PlayerInventoryController
    {
        [ServerHandle("/1/api/v1.1/inventory/catalogv3")]
        public static byte[] GetCatalog(ServerHandleArgs args)
        {
            return Content(args, JsonConvert.SerializeObject(StateSingleton.catalog), "application/json");
        }

        [ServerHandle("/1/api/v1.1/inventory/survival")]
        public static byte[] GetSurvivalInventory(ServerHandleArgs args)
        {
            InventoryResponse inv = InventoryUtils.ReadInventory(args.Headers["Authorization"]);
            string jsonstring = JsonConvert.SerializeObject(inv);
            return Content(args, jsonstring, "application/json");
        }

        [ServerHandle("/1/api/v1.1/inventory/survival/hotbar")]
        public static byte[] PutItemInHotbar(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            InventoryResponse.Hotbar[] newHotbar = JsonConvert.DeserializeObject<InventoryResponse.Hotbar[]>(args.Content);
            Tuple<InventoryUtils.InventoryUtilResult, InventoryResponse.Hotbar[]> returnHotbar = InventoryUtils.EditHotbar(authtoken, newHotbar);

            return Content(args, JsonConvert.SerializeObject(returnHotbar.Item2), "text/plain");
        }
    }

    [ServerHandleContainer]
    public static class PlayerSettingsController
    {
        [ServerHandle("/1/api/v1.1/settings")]
        public static byte[] Get(ServerHandleArgs args)
        {
            return Content(args, JsonConvert.SerializeObject(StateSingleton.settings), "application/json");
        }
    } // TODO: Fixed String

    [ServerHandleContainer]
    public static class PlayerRecipeController
    {
        [ServerHandle("/1/api/v1.1/recipes")]
        public static byte[] Get(ServerHandleArgs args)
        {
            string recipeString = System.IO.File.ReadAllText(SavePath_Server + StateSingleton.config.recipesFileLocation); // Since the serialized version has the properties mixed up
            return Content(args, recipeString, "application/json");
        }
    }

    [ServerHandleContainer]
    public static class JournalController
    {
        [ServerHandle("/1/api/v1.1/journal/catalog")]
        public static byte[] GetCatalog(ServerHandleArgs args)
        {
            StreamReader fs = new StreamReader(SavePath_Server + StateSingleton.config.journalCatalogFileLocation);
            return Content(args, fs.ReadToEnd(), "application/json");
        }

        [ServerHandle("/1/api/v1.1/player/journal")]
        public static byte[] GetJournal(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            Models.Features.JournalResponse resp = JournalUtils.ReadJournalForPlayer(authtoken);

            return Content(args, Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize(resp)), "application/json");
            //return Content(args, JsonConvert.SerializeObject(resp), "application/json");
        }
    }

    [ServerHandleContainer]
    public static class PlayerBoostController
    {
        [ServerHandle("/1/api/v1.1/boosts")]
        public static byte[] Get(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            var responseobj = BoostUtils.UpdateBoosts(authtoken);
            var response = JsonConvert.SerializeObject(responseobj);
            return Content(args, response, "application/json");
        }

        [ServerHandle("/1/api/v1.1/boosts/potions/{boostId}/activate")]
        public static byte[] GetRedeemBoost(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            var returnUpdates = BoostUtils.ActivateBoost(authtoken, new Guid(args.UrlArgs["boostId"]));
            return Content(args, JsonConvert.SerializeObject(returnUpdates), "application/json");
        }

        [ServerHandle("/1/api/v1.1/boosts/potions/{boostInstanceId}/deactivate")]
        public static byte[] DeactivateBoost(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            var returnUpdates = BoostUtils.RemoveBoost(authtoken, args.UrlArgs["boostInstanceId"]);
            return Content(args, JsonConvert.SerializeObject(returnUpdates), "application/json");
        }

        [ServerHandle("/1/api/v1.1/boosts/{boostInstanceId}")]
        public static byte[] DeleteBoost(ServerHandleArgs args)
        {
            string authtoken = args.Headers["Authorization"];
            var returnUpdates = BoostUtils.RemoveBoost(authtoken, args.UrlArgs["boostInstanceId"]);
            return Content(args, JsonConvert.SerializeObject(returnUpdates), "application/json");
        }
    } // TODO: In Progress

    [ServerHandleContainer]
    public static class PlayerFeaturesController
    {
        [ServerHandle("/1/api/v1.1/features")]
        public static byte[] Get(ServerHandleArgs args)
        {
            FeaturesResponse responseobj = new FeaturesResponse() { result = new FeaturesResult(), updates = new Updates() };
            string response = JsonConvert.SerializeObject(responseobj);
            return Content(args, response, "application/json");
        }
    }

    [ServerHandleContainer]
    public static class PlayerShopController
    {
        [ServerHandle("/1/api/v1.1/products/catalog")]
        public static byte[] GetProductCatalog(ServerHandleArgs args)
        {
            string catalog = System.IO.File.ReadAllText(SavePath_Server + StateSingleton.config.productCatalogFileLocation); // Since the serialized version has the properties mixed up
            return Content(args, catalog, "application/json");
        }

        [ServerHandle("/1/api/v1.1/commerce/storeItemInfo")]
        public static byte[] GetStoreItemInfo(ServerHandleArgs args)
        {
            string body = args.Content;
            StoreItemInfoResponse response = ShopUtils.getStoreItemInfo(JsonConvert.DeserializeObject<List<StoreItemInfo>>(body));
            return Content(args, JsonConvert.SerializeObject(response), "application/json");
        }

        [ServerHandle("/1/api/v1.1/commerce/purchase")]
        public static byte[] ItemPurchase(ServerHandleArgs args)
        {
            string body = args.Content;
            RubyResponse response = ShopUtils.purchase(args.Headers["Authorization"], JsonConvert.DeserializeObject<PurchaseItemRequest>(body));
            return Content(args, JsonConvert.SerializeObject(response), "application/json");
        }

        [ServerHandle("/1/api/v1.1/commerce/purchaseV2")]
        public static byte[] ItemPurchaseV2(ServerHandleArgs args)
        {
            string body = args.Content;
            SplitRubyResponse response = ShopUtils.purchaseV2(args.Headers["Authorization"], JsonConvert.DeserializeObject<PurchaseItemRequest>(body));
            return Content(args, JsonConvert.SerializeObject(response), "application/json");
        }
    } // TODO: Needs Playfab counterpart. When that is in place we can implement buildplate previews.
}
