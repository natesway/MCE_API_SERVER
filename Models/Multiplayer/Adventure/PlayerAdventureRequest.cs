using MCE_API_SERVER.Models.Login;
using MCE_API_SERVER.Models.Player;
using System;

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
