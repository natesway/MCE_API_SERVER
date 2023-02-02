using MCE_API_SERVER.Models.Login;
using MCE_API_SERVER.Models.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Models.Multiplayer.Adventure
{
	public class PlayerAdventureRequest
	{
		public Coordinate coordinate { get; set; }
		public InventoryResponse.Hotbar[] hotbar { get; set; }
		public Guid? instanceId { get; set; }
		public Guid[] scrollsToDeactivate { get; set; }
	}
}
