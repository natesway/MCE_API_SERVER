using MCE_API_SERVER.Models.Buildplate;
using MCE_API_SERVER.Models.Multiplayer;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Uuid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Utils
{
    public static class BuildplateUtils
    {
		private static Version4Generator version4Generator = new Version4Generator();

		public static BuildplateListResponse GetBuildplatesList(string playerId)
		{
            PlayerBuildplateList buildplates = ReadPlayerBuildplateList(playerId);
			BuildplateListResponse list = new BuildplateListResponse { result = new List<BuildplateData>() };
			foreach (Guid id in buildplates.UnlockedBuildplates) {
                BuildplateData bp = ReadBuildplate(id);
				bp.order = buildplates.UnlockedBuildplates.IndexOf(id);
				list.result.Add(bp.id != bp.templateId ? ReadBuildplate(id) : CloneTemplateBuildplate(playerId, bp));
			}

			buildplates.LockedBuildplates.ForEach(action =>
			{
                BuildplateData bp = ReadBuildplate(action);
				bp.order = buildplates.LockedBuildplates.IndexOf(action) + buildplates.UnlockedBuildplates.Count;
				list.result.Add(bp);
			});
			if (!buildplates.UnlockedBuildplates.Contains(new Guid("53470044-075c-4baa-a515-814fabd7c59e"))) {
				buildplates.UnlockedBuildplates.Add(new Guid("53470044-075c-4baa-a515-814fabd7c59e"));
				WritePlayerBuildplateList(playerId, buildplates);
			}

			return list;
		}

		public static PlayerBuildplateList ReadPlayerBuildplateList(string playerId)
			=> ParseJsonFile<PlayerBuildplateList>(playerId, "buildplates");

		public static void WritePlayerBuildplateList(string playerId, PlayerBuildplateList list)
			=> WriteJsonFile(playerId, list, "buildplates");

		public static void AddToPlayer(string playerId, Guid buildplateId)
		{
            PlayerBuildplateList bpList = ReadPlayerBuildplateList(playerId);

			if (!bpList.UnlockedBuildplates.Contains(buildplateId))
				bpList.UnlockedBuildplates.Add(buildplateId);

			WritePlayerBuildplateList(playerId, bpList);
		}

		public static BuildplateData CloneTemplateBuildplate(string playerId, BuildplateData templateBuildplate)
		{
            Guid clonedId = Guid.NewGuid();
			BuildplateData clonedBuildplate = templateBuildplate;
			clonedBuildplate.id = clonedId;
			clonedBuildplate.locked = false;

			WriteBuildplate(clonedBuildplate);

            PlayerBuildplateList list = ReadPlayerBuildplateList(playerId);
            int index = list.UnlockedBuildplates.IndexOf(templateBuildplate.id);
			list.UnlockedBuildplates.Remove(templateBuildplate.id);
			list.UnlockedBuildplates.Insert(index, clonedId);

			WritePlayerBuildplateList(playerId, list);

			return clonedBuildplate;
		}

		public static BuildplateShareResponse GetBuildplateById(BuildplateRequest buildplateReq)
		{
			BuildplateData buildplate = ReadBuildplate(buildplateReq.buildplateId);

			return new BuildplateShareResponse { result = new BuildplateShareResponse.BuildplateShareInfo { buildplateData = buildplate, playerId = null } };
		}

		public static void UpdateBuildplateAndList(BuildplateShareResponse data, string playerId)
		{
			data.result.buildplateData.eTag ??= "\"0xAAAAAAAAAAAAAAA\""; // TODO: If we ever use eTags for buildplates, replace this
			WriteBuildplate(data);

            PlayerBuildplateList list = ReadPlayerBuildplateList(playerId);
			PlayerBuildplateList newList = new PlayerBuildplateList();
			for (int i = list.UnlockedBuildplates.IndexOf(data.result.buildplateData.id); i > 0; i--) {
				list.UnlockedBuildplates[i] = list.UnlockedBuildplates[i - 1];
			}

			list.UnlockedBuildplates[0] = data.result.buildplateData.id;

			WritePlayerBuildplateList(playerId, list);
		}

		public static ShareBuildplateResponse ShareBuildplate(Guid buildplateId, string playerId)
		{
			string sharedId = version4Generator.NewUuid().ToString();
			BuildplateData originalBuildplate = ReadBuildplate(buildplateId);
			SharedBuildplateData sharedBuildplate = new SharedBuildplateData()
			{
				blocksPerMeter = originalBuildplate.blocksPerMeter,
				dimension = originalBuildplate.dimension,
				model = originalBuildplate.model,
				offset = originalBuildplate.offset,
				order = originalBuildplate.order,
				surfaceOrientation = originalBuildplate.surfaceOrientation,
				type = "Survival"
			};

			InventoryResponse.Result inventory = InventoryUtils.GetHotbarForSharing(playerId);

			SharedBuildplateInfo buildplateInfo = new SharedBuildplateInfo() { playerId = "Unknown user", buildplateData = sharedBuildplate, inventory = inventory, sharedOn = DateTime.UtcNow };
			SharedBuildplateResponse buildplateResponse = new SharedBuildplateResponse() { result = buildplateInfo, continuationToken = null, expiration = null, updates = new Models.Updates() };

			WriteSharedBuildplate(buildplateResponse, sharedId);

			ShareBuildplateResponse response = new ShareBuildplateResponse() { result = "minecraftearth://sharedbuildplate?id=" + sharedId, expiration = null, continuationToken = null, updates = null };

			return response;
		}

		public static SharedBuildplateResponse ReadSharedBuildplate(string buildplateId)
		{
            string filepath = StateSingleton.config.sharedBuildplateStorageFolderLocation + $"{buildplateId}.json";
			if (!ServerFileExists(filepath)) {
				Log.Error($"Error: Tried to read buildplate that does not exist! BuildplateID: {buildplateId}");
				return null;
			}

            string buildplateJson = Util.LoadSavedServerFileString(filepath);
            SharedBuildplateResponse parsedobj = JsonConvert.DeserializeObject<SharedBuildplateResponse>(buildplateJson);
			return parsedobj;
		}

		public static void WriteSharedBuildplate(SharedBuildplateResponse data, string buildplateId)
		{
            string filepath = StateSingleton.config.sharedBuildplateStorageFolderLocation + $"{buildplateId}.json";

			SaveServerFile(filepath, JsonConvert.SerializeObject(data));
		}

		public static BuildplateData ReadBuildplate(Guid buildplateId)
		{
            string filepath = StateSingleton.config.buildplateStorageFolderLocation + $"{buildplateId}.json";

			if (!ServerFileExists(filepath)) {
				Log.Error($"Error: Tried to read buildplate that does not exist! BuildplateID: {buildplateId}");
				return null;
			}

            string buildplateJson = LoadSavedServerFileString(filepath);
			BuildplateData parsedobj = Utf8Json.JsonSerializer.Deserialize<BuildplateData>(buildplateJson);//JsonConvert.DeserializeObject<BuildplateData>(buildplateJson);
			return parsedobj;
		}

		public static void WriteBuildplate(BuildplateData data)
		{
            Guid buildplateId = data.id;
            string filepath = StateSingleton.config.buildplateStorageFolderLocation + $"{buildplateId}.json";

			data.lastUpdated = DateTime.UtcNow;

			SaveServerFile(filepath, JsonConvert.SerializeObject(data));
		}

		public static void WriteBuildplate(BuildplateShareResponse shareResponse)
			=> WriteBuildplate(shareResponse.result.buildplateData);
	}
}
