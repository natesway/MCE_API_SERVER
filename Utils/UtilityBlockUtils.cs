using MCE_API_SERVER.Models.Features;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Utils
{
    public static class UtilityBlockUtils
    {
        public static UtilityBlocksResponse ReadUtilityBlocks(string playerId)
        {
            return ParseJsonFile<UtilityBlocksResponse>(playerId, "utilityBlocks");
        }

        public static bool WriteUtilityBlocks(string playerId, UtilityBlocksResponse obj)
        {
            return WriteJsonFile(playerId, obj, "utilityBlocks");
        }

        public static bool UpdateUtilityBlocks(string playerId, int slot, CraftingSlotInfo job)
        {
            var currentUtilBlocks = ReadUtilityBlocks(playerId);
            currentUtilBlocks.result.crafting[slot] = job;
            currentUtilBlocks.result.crafting[2].streamVersion = job.streamVersion;
            currentUtilBlocks.result.crafting[3].streamVersion = job.streamVersion;

            WriteUtilityBlocks(playerId, currentUtilBlocks);

            return true;
        }

        public static bool UpdateUtilityBlocks(string playerId, int slot, SmeltingSlotInfo job)
        {
            var currentUtilBlocks = ReadUtilityBlocks(playerId);
            currentUtilBlocks.result.smelting[slot] = job;
            currentUtilBlocks.result.smelting[2].streamVersion = job.streamVersion;
            currentUtilBlocks.result.smelting[3].streamVersion = job.streamVersion;

            WriteUtilityBlocks(playerId, currentUtilBlocks);

            return true;
        }
    }
}
