using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Features;
using MCE_API_SERVER.Models.Login;
using MCE_API_SERVER.Models.Multiplayer.Adventure;
using MCE_API_SERVER.Models.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MCE_API_SERVER.Utils
{
	public static class AdventureUtils
	{
		public static List<LocationResponse.ActiveLocation> Encounters = new List<LocationResponse.ActiveLocation>();

		public static string[] AdventureIcons = new[]
		{
			"genoa:adventure_generic_map", "genoa:adventure_generic_map_b", "genoa:adventure_generic_map_c"
		};

		private static Random random = new Random();

		public static Dictionary<Guid, Item.Rarity> crystalRarityList = StateSingleton.catalog.result.items
			.FindAll(select => select.item.type == "AdventureScroll")
			.ToDictionary(pred => pred.id, pred => pred.rarity);

		public static AdventureRequestResult RedeemCrystal(string playerId, PlayerAdventureRequest adventureRequest, Guid crystalId)
		{
			InventoryUtils.RemoveItemFromInv(playerId, crystalId);

            string selectedAdventureIcon = AdventureIcons[random.Next(0, AdventureIcons.Length)];
            Guid selectedAdventureId = Guid.Parse("b7335819-c123-49b9-83fb-8a0ec5032779");

            LocationResponse.ActiveLocation adventureLocation = new LocationResponse.ActiveLocation
			{
				coordinate = adventureRequest.coordinate,
				encounterMetadata = new EncounterMetadata
				{
					anchorId = "",
					anchorState = "Off",
					augmentedImageSetId = "",
					encounterType = EncounterType.None,
					locationId = Guid.Empty,
					worldId = Guid.Parse("4f16a053-4929-263a-c91a-29663e29df76") // TODO: Replace this with actual adventure id
				},
				expirationTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(10.00)),
				spawnTime = DateTime.UtcNow,
				icon = selectedAdventureIcon,
				id = selectedAdventureId,
				metadata = new LocationResponse.Metadata
				{
					rarity = Item.Rarity.Common,
					rewardId = "genoa:adventure_rewards"
				},
				tileId = Tile.getTileForCoordinates(adventureRequest.coordinate.latitude, adventureRequest.coordinate.longitude),
				type = "PlayerAdventure"
			};

			return new AdventureRequestResult { result = adventureLocation, updates = new Updates() };
		}

		public static List<Coordinate> readEncounterLocations()
		{
            string filepath = Util.SavePath_Server + StateSingleton.config.EncounterLocationsFileLocation;
            string encouterLocationsJson = File.ReadAllText(filepath);
			return JsonConvert.DeserializeObject<List<Coordinate>>(encouterLocationsJson);
		}

		public static List<LocationResponse.ActiveLocation> GetEncountersForLocation(double lat, double lon)
		{
            List<Coordinate> encouterLocations = readEncounterLocations();

			Encounters.RemoveAll(match => match.expirationTime < DateTime.UtcNow);

			foreach (Coordinate coordinate in encouterLocations) {
				if (Encounters.FirstOrDefault(match => match.coordinate.latitude == coordinate.latitude && match.coordinate.longitude == coordinate.longitude) == null) {
                    string selectedAdventureIcon = AdventureIcons[random.Next(0, AdventureIcons.Length)];
                    Guid selectedAdventureId = Guid.Parse("b7335819-c123-49b9-83fb-8a0ec5032779");
                    DateTime currentTime = DateTime.UtcNow;
					Encounters.Add(new LocationResponse.ActiveLocation
					{
						coordinate = coordinate,
						encounterMetadata = new Models.Multiplayer.Adventure.EncounterMetadata
						{
							anchorId = "",
							anchorState = "Off",
							augmentedImageSetId = "",
							encounterType = EncounterType.Short16X16Hostile,
							locationId = selectedAdventureId,
							worldId = selectedAdventureId // TODO: Replace this with actual adventure id
						},
						expirationTime = currentTime.Add(TimeSpan.FromMinutes(10.00)),
						spawnTime = currentTime,
						icon = selectedAdventureIcon,
						id = selectedAdventureId,
						metadata = new LocationResponse.Metadata
						{
							rarity = Item.Rarity.Common,
							rewardId = "genoa:adventure_rewards"//version4Generator.NewUuid().ToString() // Seems to always be uuidv4 from official responses so generate one
						},
						tileId = Tile.getTileForCoordinates(coordinate.latitude, coordinate.longitude),
						type = "Encounter"
					});
				}
			}
			return Encounters;
		}
	}
}
