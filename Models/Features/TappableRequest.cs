using MCE_API_SERVER.Models.Login;
using System;

namespace MCE_API_SERVER.Models.Features
{
    public class TappableRequest
    {
        public Guid id { get; set; }
        public Coordinate playerCoordinate { get; set; }
    }
}
