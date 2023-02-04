using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Models.Multiplayer.Adventure
{
	public class EncounterStateResponse
	{
		public Dictionary<Guid, ActiveEncounterStateMetadata> result { get; set; }
		public object expiration { get; set; }
		public object continuationToken { get; set; }
		public Updates updates { get; set; }
	}

	public class ActiveEncounterStateMetadata
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public ActiveEncounterState ActiveEncounterState { get; set; }
	}

	public enum ActiveEncounterState
	{
		Pristine,
		Dirty
	}
}
