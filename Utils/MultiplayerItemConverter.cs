using MCE_API_SERVER.Models.Multiplayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MCE_API_SERVER.Utils
{
	public class MultiplayerItemRarityConverter : JsonConverter<MultiplayerItemRarity>
	{
		public override MultiplayerItemRarity ReadJson(JsonReader reader, Type objectType, [AllowNull] MultiplayerItemRarity existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
            JToken obj = JObject.ReadFrom(reader);
			string rarityString = (string)obj["loc"];
			string rarity = rarityString == "" ? "Invalid" : rarityString.Split(".")[2];

			return new MultiplayerItemRarity { loc = Enum.Parse<ItemRarity>(rarity, true), value = (int)Enum.Parse<ItemRarity>(rarity, true) };
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] MultiplayerItemRarity value, JsonSerializer serializer)
		{
			JObject obj = new JObject { { "loc", "inventory.rarity." + Enum.GetName(typeof(ItemCategory), value.loc).ToLower() }, { "value", value.value } };

			obj.WriteTo(writer);
		}
	}

	public class MultiplayerItemCategoryConverter : JsonConverter<MultiplayerItemCategory>
	{
		public override MultiplayerItemCategory ReadJson(JsonReader reader, Type objectType, [AllowNull] MultiplayerItemCategory existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
            JToken obj = JObject.ReadFrom(reader);
			string categoryString = (string)obj["loc"];
			string category = categoryString == "" ? "Invalid" : categoryString.Split(".")[2];

			return new MultiplayerItemCategory { loc = Enum.Parse<ItemCategory>(category, true), value = (int)Enum.Parse<ItemCategory>(category, true) };
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] MultiplayerItemCategory value, JsonSerializer serializer)
		{
			JObject obj = new JObject { { "loc", "inventory.category." + Enum.GetName(typeof(ItemCategory), value.loc).ToLower() }, { "value", value.value } };

			obj.WriteTo(writer);
		}
	}
}
