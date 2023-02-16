using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Features;
using MCE_API_SERVER.Models.Player;
using System;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Utils
{
    public static class JournalUtils
    {
        public static bool UpdateEntry(string playerId, InventoryResponse.BaseItem item)
        {
            JournalResponse baseJournal = ReadJournalForPlayer(playerId);
            bool createEntry = !baseJournal.result.inventoryJournal.ContainsKey(item.id);

            if (createEntry) {
                JournalEntry entry = new JournalEntry() { firstSeen = item.unlocked.on, lastSeen = item.seen.on };

                entry.amountCollected = item is InventoryResponse.StackableItem stackableItem
                    ? (uint)stackableItem.owned
                    : (uint)((InventoryResponse.NonStackableItem)item).instances.Count;

                baseJournal.result.inventoryJournal.Add(item.id, entry);

                TokenUtils.AddItemToken(playerId, item.id);
            }
            else {
                JournalEntry entry = baseJournal.result.inventoryJournal[item.id];
                uint itemAmount = item is InventoryResponse.StackableItem stackableItem
                    ? (uint)stackableItem.owned
                    : (uint)((InventoryResponse.NonStackableItem)item).instances.Count;

                if (entry.amountCollected > itemAmount) entry.amountCollected = itemAmount;

                entry.lastSeen = item.seen.on;

                baseJournal.result.inventoryJournal[item.id] = entry;
            }

            WriteJournalForPlayer(playerId, baseJournal);

            return true;
        }

        public static void AddActivityLogEntry(string playerId, BaseEvent ev)
        {
            JournalResponse journal = ReadJournalForPlayer(playerId);

            Activity activityLogEntry = new Activity
            {
                eventTime = DateTime.UtcNow,
                properties = new ActivityProperties
                {
                    duration = ChallengeDuration.Career,
                    order = (uint)journal.result.activityLog.Count,
                    referenceId = Guid.NewGuid()
                },
                rewards = null,
                scenario = Scenario.CraftingJobCompleted
            };

            switch (ev) {
                case ItemEvent evt:
                    activityLogEntry.rewards = new Rewards { Inventory = new RewardComponent[0] };
                    activityLogEntry.rewards.Inventory[0].Amount = (int)evt.amount;
                    activityLogEntry.rewards.Inventory[0].Id = evt.eventId;
                    switch (evt.action) {
                        case ItemEventAction.ItemCrafted:
                            activityLogEntry.scenario = Scenario.CraftingJobCompleted;
                            break;

                        case ItemEventAction.ItemSmelted:
                            activityLogEntry.scenario = Scenario.SmeltingJobCompleted;
                            break;

                    }

                    break;

                case ChallengeEvent evt:
                    activityLogEntry.rewards = ChallengeUtils.GetRewardsForChallenge(evt.eventId);
                    activityLogEntry.scenario = Scenario.ChallengeCompleted;
                    break;

                case TappableEvent evt:
                    activityLogEntry.rewards = StateSingleton.activeTappables[evt.eventId].rewards;
                    activityLogEntry.scenario = Scenario.TappableCollected;
                    break;

                case JournalEvent evt:
                    activityLogEntry.rewards = new Rewards();
                    activityLogEntry.scenario = Scenario.JournalContentCollected;
                    break;


            }

            journal.result.activityLog.Add(activityLogEntry);
            WriteJournalForPlayer(playerId, journal);
        }

        public static JournalResponse ReadJournalForPlayer(string playerId)
            => ParseJsonFile<JournalResponse>(playerId, "journal");

        public static void WriteJournalForPlayer(string playerId, JournalResponse data)
            => WriteJsonFile(playerId, data, "journal");
    }
}
