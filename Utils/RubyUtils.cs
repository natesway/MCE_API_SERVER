using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Utils
{
    public static class RubyUtils
    {
		public static SplitRubyResponse ReadRubies(string playerId)
		{
			return Util.ParseJsonFile<SplitRubyResponse>(playerId, "rubies");
		}

		public static bool WriteRubies(string playerId, SplitRubyResponse ruby)
		{
			return Util.WriteJsonFile(playerId, ruby, "rubies");
		}

		public static bool AddRubiesToPlayer(string playerId, int count)
		{
            SplitRubyResponse origRubies = ReadRubies(playerId);
			origRubies.result.earned += count;

            int newRubyNum = origRubies.result.earned;

			WriteRubies(playerId, origRubies);

			return true;
		}

		public static bool RemoveRubiesFromPlayer(string playerId, int count)
		{
            SplitRubyResponse origRubies = ReadRubies(playerId);
			origRubies.result.earned -= count;

            int newRubyNum = origRubies.result.earned;

			WriteRubies(playerId, origRubies);

			return true;
		}

		public static RubyResponse GetNormalRubyResponse(string playerid)
		{
            SplitRubyResponse splitrubies = ReadRubies(playerid);
            RubyResponse response = new RubyResponse()
			{
				result = splitrubies.result.earned + splitrubies.result.purchased,
				expiration = null,
				continuationToken = null,
				updates = new Updates()
			};

			return response;
		}
	}
}
