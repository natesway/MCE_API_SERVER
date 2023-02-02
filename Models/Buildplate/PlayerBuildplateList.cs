using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Models.Buildplate
{
	public class PlayerBuildplateList
	{
		public List<Guid> UnlockedBuildplates { get; set; }
		public List<Guid> LockedBuildplates { get; set; }

		public PlayerBuildplateList()
		{
			UnlockedBuildplates = new List<Guid>();
			LockedBuildplates = new List<Guid>();
		}
	}
}
