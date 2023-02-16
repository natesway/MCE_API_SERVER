using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Buildplate;
using MCE_API_SERVER.Models.Login;
using MCE_API_SERVER.Models.Multiplayer;
using MCE_API_SERVER.Models.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Utils
{
    public static class MultiplayerUtils
    {
        private static readonly Dictionary<Guid, BuildplateServerResponse> InstanceList = new Dictionary<Guid, BuildplateServerResponse>();
        private static readonly Dictionary<Guid, Guid> ApiKeyList = new Dictionary<Guid, Guid>();
        private static readonly Dictionary<Guid, ServerInformation> ServerInfoList = new Dictionary<Guid, ServerInformation>();
        private static readonly Dictionary<Guid, WebSocket> ServerSocketList = new Dictionary<Guid, WebSocket>();
        private static readonly Dictionary<Guid, bool> InstanceReadyList = new Dictionary<Guid, bool>();

        public static async Task<BuildplateServerResponse> CreateBuildplateInstance(string playerId,
            string buildplateId,
            Coordinate playerCoords)
        {

            Log.Information($"[{playerId}]: Creating new buildplate instance: Buildplate {buildplateId}");

            Random rdm = new Random();
            byte[] serverRoleInstanceBytes = new byte[6];
            rdm.NextBytes(serverRoleInstanceBytes);

            //var serverRoleInstance = BitConverter.ToString(serverRoleInstanceBytes);
            string serverRoleInstance = "776932eeeb69";
            string serverPlayerJoinCode = Convert.ToBase64String(serverRoleInstanceBytes);

            KeyValuePair<Guid, ServerInformation> server = ServerInfoList.First();
            string serverIp = server.Value.ip;
            int serverPort = server.Value.port;
            Guid serverInstanceId = await NotifyServerInstance(server.Key, buildplateId, playerId);

            BuildplateData buildplate = BuildplateUtils.ReadBuildplate(Guid.Parse(buildplateId));
            double blocksPerMeter = buildplate.blocksPerMeter;
            Offset buildplateOffset = buildplate.offset;
            BuildplateServerResponse.InstanceMetadata instanceMetadata = new BuildplateServerResponse.InstanceMetadata { buildplateid = buildplateId };

            Dimension dimensions = buildplate.dimension;
            Guid templateId = buildplate.templateId; // Not used AFAIK
            SurfaceOrientation surfaceOrientation = buildplate.surfaceOrientation; // Can also be vertical

            BuildplateServerResponse.GameplayMetadata buildplateData = new BuildplateServerResponse.GameplayMetadata
            {
                augmentedImageSetId = null,
                blocksPerMeter = blocksPerMeter,
                breakableItemToItemLootMap = new BuildplateServerResponse.BreakableItemToItemLootMap(),
                dimension = dimensions,
                gameplayMode = GameplayMode.Buildplate,
                isFullSize =
                    false, // TODO: Defines if the buildplate should be rendered, we just disable it (Actual Check: (dimensions.x >= 32 && dimensions.z >= 32))
                offset = buildplateOffset, // Same for all buildplates
                playerJoinCode = serverPlayerJoinCode, // 24 letters/Numbers, probably randomly generated
                rarity = null, // Why even is this here?
                shutdownBehavior = new List<string>()
                {
                    "ServerShutdownWhenAllPlayersQuit", "ServerShutdownWhenHostPlayerQuits"
                }, // Own instance server needs to respect this
                snapshotOptions = new BuildplateServerResponse.SnapshotOptions
                {
                    saveState = new BuildplateServerResponse.SaveState // Should be the same for all buildplates
                    {
                        inventory = true,
                        model = true,
                        world = true
                    },
                    snapshotTriggerConditions = "None",
                    snapshotWorldStorage = "Buildplate",
                    triggerConditions = new List<string>() { "Interval", "PlayerExits" },
                    triggerInterval = new TimeSpan(00, 00, 30)
                },
                spawningClientBuildNumber =
                    "2020.1217.02", // How should we figure this out? Should probably just be the latest every time
                spawningPlayerId = playerId,
                surfaceOrientation = surfaceOrientation,
                templateId = templateId,
                worldId = buildplateId
            };

            BuildplateServerResponse result = new BuildplateServerResponse
            {
                result = new BuildplateServerResponse.Result
                {
                    applicationStatus = "Unknown",
                    //fqdn = "dns2527870c-89c6-420e-8378-996a2c40304a-azurebatch-cloudservice.westeurope.cloudapp.azure.com", // figure out why this breaks everything
                    fqdn = "d.projectearth.dev",
                    gameplayMetadata = buildplateData,
                    hostCoordinate = playerCoords,
                    instanceId = serverInstanceId.ToString(),
                    ipV4Address = serverIp,
                    metadata = JsonConvert.SerializeObject(instanceMetadata),
                    partitionId = playerId,
                    port = serverPort,
                    roleInstance = serverRoleInstance,
                    serverReady = false,
                    serverStatus = "Running"
                },
                updates = new Updates()
            };

            if (InstanceReadyList[serverInstanceId]) {
                result.result.applicationStatus = "Ready";
                result.result.serverReady = true;
            }

            InstanceList.Add(serverInstanceId, result);

            return result;
        }

        public static async Task<BuildplateServerResponse> CreateBuildplatePlayInstance(string playerId,
            string buildplateId,
            Coordinate playerCoords)
        {

            Log.Information($"[{playerId}]: Creating new buildplate play instance: Buildplate {buildplateId}");

            Random rdm = new Random();
            byte[] serverRoleInstanceBytes = new byte[6];
            rdm.NextBytes(serverRoleInstanceBytes);

            //var serverRoleInstance = BitConverter.ToString(serverRoleInstanceBytes);
            string serverRoleInstance = "776932eeeb69";
            string serverPlayerJoinCode = Convert.ToBase64String(serverRoleInstanceBytes);

            KeyValuePair<Guid, ServerInformation> server = ServerInfoList.First();
            string serverIp = server.Value.ip;
            int serverPort = server.Value.port;
            Guid serverInstanceId = await NotifyServerInstance(server.Key, buildplateId, playerId);

            BuildplateData buildplate = BuildplateUtils.ReadBuildplate(Guid.Parse(buildplateId));
            double blocksPerMeter = buildplate.blocksPerMeter;
            Offset buildplateOffset = buildplate.offset;
            BuildplateServerResponse.InstanceMetadata instanceMetadata = new BuildplateServerResponse.InstanceMetadata { buildplateid = buildplateId };

            Dimension dimensions = buildplate.dimension;
            Guid templateId = buildplate.templateId; // Not used AFAIK
            SurfaceOrientation surfaceOrientation = buildplate.surfaceOrientation; // Can also be vertical

            BuildplateServerResponse.GameplayMetadata buildplateData = new BuildplateServerResponse.GameplayMetadata
            {
                augmentedImageSetId = null,
                blocksPerMeter = 2d,
                breakableItemToItemLootMap = new BuildplateServerResponse.BreakableItemToItemLootMap(),
                dimension = dimensions,
                gameplayMode = GameplayMode.Buildplate,
                isFullSize =
                    false, // TODO: Defines if the buildplate should be rendered, we just disable it (Actual Check: (dimensions.x >= 32 && dimensions.z >= 32))
                offset = buildplateOffset, // Same for all buildplates
                playerJoinCode = serverPlayerJoinCode, // 24 letters/Numbers, probably randomly generated
                rarity = null, // Why even is this here?
                shutdownBehavior = new List<string>()
                {
                    "ServerShutdownWhenAllPlayersQuit", "ServerShutdownWhenHostPlayerQuits"
                }, // Own instance server needs to respect this
                snapshotOptions = new BuildplateServerResponse.SnapshotOptions
                {
                    saveState = new BuildplateServerResponse.SaveState // Should be the same for all buildplates
                    {
                        inventory = true,
                        model = true,
                        world = true
                    },
                    snapshotTriggerConditions = "None",
                    snapshotWorldStorage = "Buildplate",
                    triggerConditions = new List<string>() { "Interval", "PlayerExits" },
                    triggerInterval = new TimeSpan(00, 00, 30)
                },
                spawningClientBuildNumber =
                    "2020.1217.02", // How should we figure this out? Should probably just be the latest every time
                spawningPlayerId = playerId,
                surfaceOrientation = surfaceOrientation,
                templateId = templateId,
                worldId = buildplateId
            };

            BuildplateServerResponse result = new BuildplateServerResponse
            {
                result = new BuildplateServerResponse.Result
                {
                    applicationStatus = "Unknown",
                    //fqdn = "dns2527870c-89c6-420e-8378-996a2c40304a-azurebatch-cloudservice.westeurope.cloudapp.azure.com", // figure out why this breaks everything
                    fqdn = "d.projectearth.dev",
                    gameplayMetadata = buildplateData,
                    hostCoordinate = playerCoords,
                    instanceId = serverInstanceId.ToString(),
                    ipV4Address = serverIp,
                    metadata = JsonConvert.SerializeObject(instanceMetadata),
                    partitionId = playerId,
                    port = serverPort,
                    roleInstance = serverRoleInstance,
                    serverReady = false,
                    serverStatus = "Running"
                },
                updates = new Updates()
            };

            if (InstanceReadyList[serverInstanceId]) {
                result.result.applicationStatus = "Ready";
                result.result.serverReady = true;
            }

            InstanceList.Add(serverInstanceId, result);

            return result;
        }

        public static async Task<BuildplateServerResponse> CreateSharedBuildplatePlayInstance(string playerId,
            string buildplateId,
            Coordinate playerCoords)
        {

            Log.Information($"[{playerId}]: Creating new shared buildplate play instance: Buildplate {buildplateId}");

            Random rdm = new Random();
            byte[] serverRoleInstanceBytes = new byte[6];
            rdm.NextBytes(serverRoleInstanceBytes);

            //var serverRoleInstance = BitConverter.ToString(serverRoleInstanceBytes);
            string serverRoleInstance = "776932eeeb69";
            string serverPlayerJoinCode = Convert.ToBase64String(serverRoleInstanceBytes);

            KeyValuePair<Guid, ServerInformation> server = ServerInfoList.First();
            string serverIp = server.Value.ip;
            int serverPort = server.Value.port;
            Guid serverInstanceId = await NotifyServerInstance(server.Key, buildplateId, playerId);

            SharedBuildplateResponse buildplate = BuildplateUtils.ReadSharedBuildplate(buildplateId);
            double blocksPerMeter = buildplate.result.buildplateData.blocksPerMeter;
            Offset buildplateOffset = buildplate.result.buildplateData.offset;
            BuildplateServerResponse.InstanceMetadata instanceMetadata = new BuildplateServerResponse.InstanceMetadata { buildplateid = buildplateId };

            Dimension dimensions = buildplate.result.buildplateData.dimension;
            Guid templateId = Guid.Empty; // Not used AFAIK
            SurfaceOrientation surfaceOrientation = buildplate.result.buildplateData.surfaceOrientation; // Can also be vertical

            BuildplateServerResponse.GameplayMetadata buildplateData = new BuildplateServerResponse.GameplayMetadata
            {
                augmentedImageSetId = null,
                blocksPerMeter = blocksPerMeter,
                breakableItemToItemLootMap = new BuildplateServerResponse.BreakableItemToItemLootMap(),
                dimension = dimensions,
                gameplayMode = GameplayMode.Buildplate,
                isFullSize =
                    false, // TODO: Defines if the buildplate should be rendered, we just disable it (Actual Check: (dimensions.x >= 32 && dimensions.z >= 32))
                offset = buildplateOffset, // Same for all buildplates
                playerJoinCode = serverPlayerJoinCode, // 24 letters/Numbers, probably randomly generated
                rarity = null, // Why even is this here?
                shutdownBehavior = new List<string>()
                {
                    "ServerShutdownWhenAllPlayersQuit", "ServerShutdownWhenHostPlayerQuits"
                }, // Own instance server needs to respect this
                snapshotOptions = new BuildplateServerResponse.SnapshotOptions
                {
                    saveState = new BuildplateServerResponse.SaveState // Should be the same for all buildplates
                    {
                        inventory = true,
                        model = true,
                        world = true
                    },
                    snapshotTriggerConditions = "None",
                    snapshotWorldStorage = "Buildplate",
                    triggerConditions = new List<string>() { "Interval", "PlayerExits" },
                    triggerInterval = new TimeSpan(00, 00, 30)
                },
                spawningClientBuildNumber =
                    "2020.1217.02", // How should we figure this out? Should probably just be the latest every time
                spawningPlayerId = playerId,
                surfaceOrientation = surfaceOrientation,
                templateId = templateId,
                worldId = buildplateId
            };

            BuildplateServerResponse result = new BuildplateServerResponse
            {
                result = new BuildplateServerResponse.Result
                {
                    applicationStatus = "Unknown",
                    //fqdn = "dns2527870c-89c6-420e-8378-996a2c40304a-azurebatch-cloudservice.westeurope.cloudapp.azure.com", // figure out why this breaks everything
                    fqdn = "d.projectearth.dev",
                    gameplayMetadata = buildplateData,
                    hostCoordinate = playerCoords,
                    instanceId = serverInstanceId.ToString(),
                    ipV4Address = serverIp,
                    metadata = JsonConvert.SerializeObject(instanceMetadata),
                    partitionId = playerId,
                    port = serverPort,
                    roleInstance = serverRoleInstance,
                    serverReady = false,
                    serverStatus = "Running"
                },
                updates = new Updates()
            };

            if (InstanceReadyList[serverInstanceId]) {
                result.result.applicationStatus = "Ready";
                result.result.serverReady = true;
            }

            InstanceList.Add(serverInstanceId, result);

            return result;
        }

        public static async Task<BuildplateServerResponse> CreateAdventureInstance(string playerId,
            string buildplateId,
            Coordinate playerCoords)
        {

            Log.Information($"[{playerId}]: Creating new adventure instance: Adventure {buildplateId}");

            Random rdm = new Random();
            byte[] serverRoleInstanceBytes = new byte[6];
            rdm.NextBytes(serverRoleInstanceBytes);

            //var serverRoleInstance = BitConverter.ToString(serverRoleInstanceBytes);
            string serverRoleInstance = "776932eeeb69";
            string serverPlayerJoinCode = Convert.ToBase64String(serverRoleInstanceBytes);

            KeyValuePair<Guid, ServerInformation> server = ServerInfoList.First();
            string serverIp = server.Value.ip;
            int serverPort = server.Value.port;
            Guid serverInstanceId = await NotifyServerInstance(server.Key, buildplateId, playerId);

            BuildplateData buildplate = BuildplateUtils.ReadBuildplate(Guid.Parse(buildplateId));
            double blocksPerMeter = buildplate.blocksPerMeter;
            Offset buildplateOffset = buildplate.offset;
            BuildplateServerResponse.InstanceMetadata instanceMetadata = new BuildplateServerResponse.InstanceMetadata { buildplateid = buildplateId };

            Dimension dimensions = buildplate.dimension;
            Guid templateId = buildplate.templateId; // Not used AFAIK
            SurfaceOrientation surfaceOrientation = buildplate.surfaceOrientation; // Can also be vertical

            BuildplateServerResponse.GameplayMetadata buildplateData = new BuildplateServerResponse.GameplayMetadata
            {
                augmentedImageSetId = null,
                blocksPerMeter = blocksPerMeter,
                breakableItemToItemLootMap = new BuildplateServerResponse.BreakableItemToItemLootMap(),
                dimension = dimensions,
                gameplayMode = GameplayMode.Encounter,
                isFullSize =
                    false, // TODO: Defines if the buildplate should be rendered, we just disable it (Actual Check: (dimensions.x >= 32 && dimensions.z >= 32))
                offset = buildplateOffset, // Same for all buildplates
                playerJoinCode = serverPlayerJoinCode, // 24 letters/Numbers, probably randomly generated
                rarity = null, // Why even is this here?
                shutdownBehavior = new List<string>()
                {
                    "ServerShutdownWhenAllPlayersQuit", "ServerShutdownWhenHostPlayerQuits"
                }, // Own instance server needs to respect this
                snapshotOptions = new BuildplateServerResponse.SnapshotOptions
                {
                    saveState = new BuildplateServerResponse.SaveState // Should be the same for all buildplates
                    {
                        inventory = true,
                        model = true,
                        world = true
                    },
                    snapshotTriggerConditions = "None",
                    snapshotWorldStorage = "Encounter",
                    triggerConditions = new List<string>() { "Interval", "PlayerExits" },
                    triggerInterval = new TimeSpan(00, 00, 30)
                },
                spawningClientBuildNumber =
                    "2020.1217.02", // How should we figure this out? Should probably just be the latest every time
                spawningPlayerId = playerId,
                surfaceOrientation = surfaceOrientation,
                templateId = templateId,
                worldId = buildplateId
            };

            BuildplateServerResponse result = new BuildplateServerResponse
            {
                result = new BuildplateServerResponse.Result
                {
                    applicationStatus = "Unknown",
                    //fqdn = "dns2527870c-89c6-420e-8378-996a2c40304a-azurebatch-cloudservice.westeurope.cloudapp.azure.com", // figure out why this breaks everything
                    fqdn = "d.projectearth.dev",
                    gameplayMetadata = buildplateData,
                    hostCoordinate = playerCoords,
                    instanceId = serverInstanceId.ToString(),
                    ipV4Address = serverIp,
                    metadata = JsonConvert.SerializeObject(instanceMetadata),
                    partitionId = playerId,
                    port = serverPort,
                    roleInstance = serverRoleInstance,
                    serverReady = false,
                    serverStatus = "Running"
                },
                updates = new Updates()
            };

            if (InstanceReadyList[serverInstanceId]) {
                result.result.applicationStatus = "Ready";
                result.result.serverReady = true;
            }

            InstanceList.Add(serverInstanceId, result);

            return result;
        }

        public static async Task<Guid> NotifyServerInstance(Guid serverId, string buildplateId, string playerId)
        {
            Guid instanceId = Guid.NewGuid();

            InstanceReadyList.Add(instanceId, false);

            ServerInstanceRequestInfo instanceInfo = new ServerInstanceRequestInfo { buildplateId = Guid.Parse(buildplateId), instanceId = instanceId, playerId = playerId };

            string requestString = JsonConvert.SerializeObject(instanceInfo);
            byte[] requestArr = Encoding.UTF8.GetBytes(requestString);

            await ServerSocketList[serverId].SendAsync(new ArraySegment<byte>(requestArr, 0, requestArr.Length),
                WebSocketMessageType.Text, true, CancellationToken.None);

            return instanceId;
        }

        public static BuildplateServerResponse CheckInstanceStatus(string playerId, Guid instanceId)
        {
            if (InstanceReadyList[instanceId]) {
                InstanceList[instanceId].result.applicationStatus = "Ready";
                InstanceList[instanceId].result.serverReady = true;
                return InstanceList[instanceId];
            }
            else return InstanceList[instanceId];
        }

        public static BuildplateServerResponse GetServerInstance(string joinCode)
        {
            BuildplateServerResponse instance = InstanceList.FirstOrDefault(match => match.Value.result.gameplayMetadata.playerJoinCode == joinCode).Value;
            return instance;
        }

        private static HotbarTranslation[] EditHotbarForPlayer(string playerId, MultiplayerItem[] multiplayerHotbar)
        {
            if (multiplayerHotbar == null) {
                return null;
            }

            if (multiplayerHotbar.Length != 7) {
                MultiplayerItem[] tempArr = new MultiplayerItem[7];
                multiplayerHotbar.CopyTo(tempArr, 0);
                for (int i = 0; i < tempArr.Length; i++) {
                    tempArr[i] ??= new MultiplayerItem
                    {
                        category = new MultiplayerItemCategory
                        {
                            loc = ItemCategory.Invalid,
                            value = (int)ItemCategory.Invalid
                        },
                        count = 0,
                        guid = Guid.Empty,
                        owned = true,
                        rarity = new MultiplayerItemRarity
                        {
                            loc = ItemRarity.Invalid,
                            value = (int)ItemRarity.Invalid
                        }
                    };
                }

                multiplayerHotbar = tempArr;
            }

            InventoryResponse inv = InventoryUtils.ReadInventory(playerId);
            InventoryResponse.Hotbar[] hotbar = new InventoryResponse.Hotbar[multiplayerHotbar.Length];
            HotbarTranslation[] response = new HotbarTranslation[multiplayerHotbar.Length];

            for (int i = 0; i < multiplayerHotbar.Length; i++) {
                MultiplayerItem item = multiplayerHotbar[i];
                if (item.guid != Guid.Empty) {
                    Models.Features.Item catalogItem = StateSingleton.catalog.result.items.Find(match => match.id == item.guid);
                    if (item.instance_data != null) {
                        hotbar[i] = new InventoryResponse.Hotbar
                        {
                            count = 1,
                            id = item.guid,
                            instanceId = item.instance_data.id,
                            health = item.instance_data.health
                        };
                    }
                    else {
                        hotbar[i] = new InventoryResponse.Hotbar
                        {
                            count = item.count,
                            id = item.guid,
                            instanceId = null,
                            health = null
                        };
                    }

                    response[i] = new HotbarTranslation
                    {
                        count = item.count,
                        identifier = catalogItem.item.name,
                        meta = catalogItem.item.aux,
                        slotId = i
                    };
                }
                else {
                    hotbar[i] = null;
                    response[i] = new HotbarTranslation
                    {
                        count = 0,
                        identifier = "air",
                        meta = 0,
                        slotId = i
                    };
                }
            }

            InventoryUtils.EditHotbar(playerId, hotbar);

            return response;
        }

        private static HotbarTranslation[] GetHotbarForPlayer(string playerId)
        {
            InventoryResponse inv = InventoryUtils.ReadInventory(playerId);
            InventoryResponse.Hotbar[] hotbar = inv.result.hotbar;
            HotbarTranslation[] response = new HotbarTranslation[hotbar.Length];

            for (int i = 0; i < hotbar.Length; i++) {
                InventoryResponse.Hotbar item = hotbar[i];

                if (item != null) {
                    var catalogItem = StateSingleton.catalog.result.items.Find(match => match.id == item.id);

                    response[i] = new HotbarTranslation { count = item.count, identifier = catalogItem.item.name, meta = catalogItem.item.aux, slotId = i };
                }
                else {
                    response[i] = new HotbarTranslation { count = 0, identifier = "air", meta = 0, slotId = i };
                }
            }

            return response;
        }

        public static void EditInventoryForPlayer(string playerId, EditInventoryRequest request)
        {
            int damage = request.meta == -1 ? 0 : request.meta;
            Models.Features.Item catalogItem =
                StateSingleton.catalog.result.items.Find(match =>
                    match.item.name == request.identifier && match.item.aux == damage);
            bool isNonStackableItem = catalogItem.item.type == "Tool";
            bool isHotbar = request.slotIndex <= 6;

            if (isHotbar) {
                InventoryResponse.Hotbar[] hotbar = InventoryUtils.GetHotbar(playerId).Item2;
                InventoryResponse.Hotbar slot = hotbar[request.slotIndex] ?? new InventoryResponse.Hotbar();

                if (request.removeItem) slot = null;
                else {
                    slot.count = request.count + 1;
                    slot.id = catalogItem.id;

                    if (isNonStackableItem) slot.health = request.health;
                }

                hotbar[request.slotIndex] = slot;
                InventoryUtils.EditHotbar(playerId, hotbar, false);
            }
            else {
                // Removing items from the normal inventory should never be possible, except from the hotbar
                //if (request.removeItem) InventoryUtils.RemoveItemFromInv(playerId, catalogItem.id, request.count, request.health);
                //else 
                InventoryUtils.AddItemToInv(playerId, catalogItem.id, request.count);
            }
        }
        public static string ExecuteServerCommand(ServerCommandRequest request)
        {
            ServerCommandType command = request.command;
            Log.Information($"Received {command} from Server {request.serverId}!");
            if (ApiKeyList.ContainsValue(request.apiKey)) {
                string playerId = request.playerId;
                switch (command) {
                    case ServerCommandType.GetBuildplate:
                        BuildplateRequest buildplate = JsonConvert.DeserializeObject<BuildplateRequest>(request.requestData);
                        return JsonConvert.SerializeObject(BuildplateUtils.GetBuildplateById(buildplate));

                    case ServerCommandType.GetInventoryForClient:
                        MultiplayerInventoryResponse inv = InventoryUtils.ReadInventoryForMultiplayer(playerId);
                        return JsonConvert.SerializeObject(inv);

                    case ServerCommandType.GetInventory:
                        HotbarTranslation[] hotbarForServer = GetHotbarForPlayer(playerId);
                        return JsonConvert.SerializeObject(hotbarForServer);

                    case ServerCommandType.EditInventory:
                        EditInventoryRequest invEdits = JsonConvert.DeserializeObject<EditInventoryRequest>(request.requestData);
                        EditInventoryForPlayer(playerId, invEdits);
                        return "ok";

                    case ServerCommandType.EditHotbar:
                        MultiplayerInventoryResponse invData = JsonConvert.DeserializeObject<MultiplayerInventoryResponse>(request.requestData);
                        HotbarTranslation[] newHotbarInfo = EditHotbarForPlayer(playerId, invData.hotbar);
                        return JsonConvert.SerializeObject(newHotbarInfo);

                    case ServerCommandType.EditBuildplate:
                        BuildplateShareResponse newBuildplateData =
                            JsonConvert.DeserializeObject<BuildplateShareResponse>(request.requestData);
                        //BuildplateUtils.UpdateBuildplateAndList(newBuildplateData, playerId);
                        return "ok";

                    case ServerCommandType.MarkServerAsReady:
                        ServerInstanceInfo instanceInfo = JsonConvert.DeserializeObject<ServerInstanceInfo>(request.requestData);
                        MarkServerAsReady(instanceInfo);
                        return "ok";

                    default:
                        return null;
                }
            }
            else return null;
        }

        public static void MarkServerAsReady(ServerInstanceInfo info)
        {
            InstanceReadyList[info.instanceId] = true;
        }

        public static async Task AuthenticateServer(WebSocket webSocketRequest)
        {
            byte[] messageBuffer = new byte[4096];
            WebSocketReceiveResult result =
                await webSocketRequest.ReceiveAsync(new ArraySegment<byte>(messageBuffer), CancellationToken.None);
            ServerAuthInformation authStatus = ServerAuthInformation.NotAuthed;
            ServerInformation info = null;
            string challenge = null;

            while (!result.CloseStatus.HasValue) {
                info ??= JsonConvert.DeserializeObject<ServerInformation>(Encoding.UTF8.GetString(messageBuffer));

                switch (authStatus) {
                    case ServerAuthInformation.NotAuthed: // Send Challenge

                        challenge = Guid.NewGuid().ToString();
                        byte[] challengeBytes = Encoding.UTF8.GetBytes(challenge);

                        await webSocketRequest.SendAsync(
                            new ArraySegment<byte>(challengeBytes, 0, challengeBytes.Length), result.MessageType,
                            result.EndOfMessage, CancellationToken.None);

                        authStatus = ServerAuthInformation.AuthStage1;

                        Array.Clear(messageBuffer, 0, messageBuffer.Length);
                        result = await webSocketRequest.ReceiveAsync(new ArraySegment<byte>(messageBuffer),
                            CancellationToken.None);

                        break;

                    case ServerAuthInformation.AuthStage1: // Verify challenge response
                        string challengeResponse = Encoding.UTF8.GetString(messageBuffer).TrimEnd('\0');
                        bool success = VerifyChallenge(challenge, challengeResponse, info);

                        byte[] challengeResponseStatus = Encoding.UTF8.GetBytes(success.ToString().ToLower());

                        await webSocketRequest.SendAsync(
                            new ArraySegment<byte>(challengeResponseStatus, 0, challengeResponseStatus.Length),
                            result.MessageType, result.EndOfMessage, CancellationToken.None);

                        if (success) authStatus = ServerAuthInformation.AuthStage2;
                        else {
                            authStatus = ServerAuthInformation.FailedAuth;

                            await webSocketRequest.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);

                            return;
                        }

                        break;

                    case ServerAuthInformation.AuthStage2:

                        if (!ApiKeyList.ContainsKey(info.serverId)) {
                            Log.Information($"Server {info.serverId} registered itself to the api.");

                            Guid apiKey = Guid.NewGuid();
                            ApiKeyList.Add(info.serverId, apiKey);
                            ServerInfoList.Add(info.serverId, info);
                            ServerSocketList.Add(info.serverId, webSocketRequest);

                            byte[] apiKeyBytes = Encoding.UTF8.GetBytes(apiKey.ToString().ToLower());

                            await webSocketRequest.SendAsync(
                                new ArraySegment<byte>(apiKeyBytes, 0, apiKeyBytes.Length), result.MessageType,
                                result.EndOfMessage, CancellationToken.None);

                            authStatus = ServerAuthInformation.Authed;
                        }

                        break;

                    case ServerAuthInformation.Authed:
                        while (true) { }

                        break;
                }
            }
        }

        private static bool VerifyChallenge(string challenge, string challengeResponse, ServerInformation info)
        {
            HMACSHA256 crypto = new HMACSHA256 { Key = Convert.FromBase64String(StateSingleton.config.multiplayerAuthKeys[info.ip]) };

            string expectedResult = /*Convert.ToHexString*/ByteArrayToHexString(crypto.ComputeHash(Encoding.UTF8.GetBytes(challenge)));
            return expectedResult == challengeResponse;
        }
    }
}
