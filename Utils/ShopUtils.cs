using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Buildplate;
using MCE_API_SERVER.Models.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MCE_API_SERVER.Utils
{
    public static class ShopUtils
    {
        public static Dictionary<Guid, StoreItemInfo> readShopItemDictionary()
        {
            string filepath = Util.SavePath_Server + StateSingleton.config.ShopItemDictionaryFileLocation;
            string storeItemsJson = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<Dictionary<Guid, StoreItemInfo>>(storeItemsJson);
        }

        public static void processPurchase(string playerId, PurchaseItemRequest request)
        {
            try {
                StoreItemInfo itemToPurchase = StateSingleton.shopItems[request.itemId];
                if (itemToPurchase.storeItemType == StoreItemType.Items) {
                    foreach (var item in itemToPurchase.inventoryCounts) {
                        InventoryUtils.AddItemToInv(playerId, item.Key, item.Value);
                    }
                }
                else {
                    BuildplateUtils.AddToPlayer(playerId, itemToPurchase.id);
                }
                RubyUtils.RemoveRubiesFromPlayer(playerId, request.expectedPurchasePrice);
            }
            catch {
                Log.Error("Error: Failed to process shop order for item id: " + request.itemId);
                return;
            }
        }

        public static RubyResponse purchase(string playerId, PurchaseItemRequest request)
        {
            processPurchase(playerId, request);
            return RubyUtils.GetNormalRubyResponse(playerId);
        }

        public static SplitRubyResponse purchaseV2(string playerId, PurchaseItemRequest request)
        {
            processPurchase(playerId, request);
            return RubyUtils.ReadRubies(playerId);
        }

        public static StoreItemInfoResponse getStoreItemInfo(List<StoreItemInfo> request)
        {
            List<StoreItemInfo> result = new List<StoreItemInfo>();
            for (int i = 0; i < request.Count; i++) {
                if (request[i].storeItemType == StoreItemType.Buildplates) {
                    BuildplateData buildplate = BuildplateUtils.ReadBuildplate(request[i].id);
                    StoreItemInfo itemFromMap = StateSingleton.shopItems.FirstOrDefault(match => match.Value.id == request[i].id).Value;
                    if (buildplate != null) {
                        result.Add(new StoreItemInfo()
                        {
                            id = request[i].id,
                            storeItemType = request[i].storeItemType,
                            status = StoreItemStatus.Found,
                            streamVersion = request[i].streamVersion,
                            buildplateWorldDimension = buildplate.dimension,
                            buildplateWorldOffset = buildplate.offset,
                            model = buildplate.model,
                            featuredItem = itemFromMap?.featuredItem,
                            inventoryCounts = itemFromMap?.inventoryCounts
                        });
                    }
                    else {
                        result.Add(new StoreItemInfo()
                        {
                            id = request[i].id,
                            storeItemType = request[i].storeItemType,
                            status = StoreItemStatus.NotFound,
                            streamVersion = request[i].streamVersion
                        });
                    }
                }
            }
            StoreItemInfoResponse response = new StoreItemInfoResponse()
            {
                result = result,
                continuationToken = null,
                expiration = null,
                updates = new Updates()
            };
            return response;
        }
    }
}
