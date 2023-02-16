using System.Collections.Generic;
using static MCE_API_SERVER.Models.Player.LocationResponse;

namespace MCE_API_SERVER.Models.Player
{
    public class ScrollsResponse
    {
        public List<ActiveLocation> result { get; set; }
        public object expiration { get; set; }
        public object continuationToken { get; set; }
        public Updates updates { get; set; }
    }
}
