using MCE_API_SERVER.Models.Login;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Models.Features
{
	public class TappableRequest
	{
		public Guid id { get; set; }
		public Coordinate playerCoordinate { get; set; }
	}
}
