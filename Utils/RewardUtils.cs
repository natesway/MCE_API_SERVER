using MCE_API_SERVER.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Utils
{
    public static class RewardUtils
    {
		public static Updates RedeemRewards(string playerId, Rewards rewards, EventLocation location)
		{
			Updates updates = new Updates();
			uint nextStreamId = Util.GetNextStreamVersion();
			foreach (RewardComponent buildplate in rewards.Buildplates) {
				BuildplateUtils.AddToPlayer(playerId, buildplate.Id);
				updates.buildplates = nextStreamId;
			}

			foreach (RewardComponent challenge in rewards.Challenges) {
				ChallengeUtils.AddChallengeToPlayer(playerId, challenge.Id);
				EventUtils.HandleEvents(playerId, new ChallengeEvent { action = ChallengeEventAction.ChallengeUnlocked, eventId = challenge.Id });

				updates.challenges = nextStreamId;
			}

			foreach (RewardComponent item in rewards.Inventory) {
				InventoryUtils.AddItemToInv(playerId, item.Id, item.Amount);
				EventUtils.HandleEvents(playerId, new ItemEvent { action = ItemEventAction.ItemAwarded, amount = (uint)item.Amount, eventId = item.Id, location = location });

				updates.inventory = nextStreamId;
				updates.playerJournal = nextStreamId;
			}

			foreach (RewardComponent utilityBlock in rewards.UtilityBlocks) {
				// TODO: This is most likely unused in the actual game, since crafting tables/furnaces dont have ids
			}

			foreach (Guid personaItem in rewards.PersonaItems) {
				// PersonaUtils.AddToPlayer(playerId, personaItem) If we can ever implement CC, this is already in place
			}

			if (rewards.ExperiencePoints != null) {
				ProfileUtils.AddExperienceToPlayer(playerId, rewards.ExperiencePoints.Value);
				updates.characterProfile = nextStreamId;
			}

			if (rewards.Rubies != null) {
				RubyUtils.AddRubiesToPlayer(playerId, rewards.Rubies.Value);
				updates.characterProfile = nextStreamId;
			}

			return updates;
		}
	}
}
