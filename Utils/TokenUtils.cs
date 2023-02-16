using MCE_API_SERVER.Models;
using MCE_API_SERVER.Models.Player;
using MCE_API_SERVER.Uuid;
using System;
using System.Collections.Generic;
using System.Linq;

using static MCE_API_SERVER.Util;

namespace MCE_API_SERVER.Utils
{
    public static class TokenUtils
    {
        private static readonly Version4Generator Version4Generator = new Version4Generator();

        public static Dictionary<Guid, Token> GetSigninTokens(string playerId)
        {
            TokenResponse origTokens = ReadTokens(playerId);
            Dictionary<Guid, Token> returnTokens = new Dictionary<Guid, Token>();
            returnTokens = origTokens.Result.tokens
                .Where(pred => pred.Value.clientProperties.Count == 0)
                .ToDictionary(pred => pred.Key, pred => pred.Value);



            return returnTokens;
        }

        public static void AddItemToken(string playerId, Guid itemId)
        {
            Token itemtoken = new Token
            {
                clientProperties = new Dictionary<string, string>(),
                clientType = "item.unlocked",
                lifetime = "Persistent",
                rewards = new Rewards()
            };

            itemtoken.clientProperties.Add("itemid", itemId.ToString());

            AddToken(playerId, itemtoken);

            Log.Information($"[{playerId}]: Added item token {itemId}!");
        }

        public static bool AddToken(string playerId, Token tokenToAdd)
        {
            TokenResponse tokens = ReadTokens(playerId);
            if (!tokens.Result.tokens.ContainsValue(tokenToAdd)) {
                tokens.Result.tokens.Add(Guid.NewGuid(), tokenToAdd);
                WriteTokens(playerId, tokens);
                Log.Information($"[{playerId}] Added token!");
                return true;
            }

            Log.Error($"[{playerId}] Tried to add token, but it already exists!");
            return false;
        }

        public static Token RedeemToken(string playerId, Guid tokenId)
        {
            TokenResponse parsedTokens = ReadTokens(playerId);
            if (parsedTokens.Result.tokens.ContainsKey(tokenId)) {
                Token tokenToRedeem = parsedTokens.Result.tokens[tokenId];
                RewardUtils.RedeemRewards(playerId, tokenToRedeem.rewards, EventLocation.Token);

                parsedTokens.Result.tokens.Remove(tokenId);

                WriteTokens(playerId, parsedTokens);

                Log.Information($"[{playerId}]: Redeemed token {tokenId}.");

                return tokenToRedeem;
            }
            else {
                Log.Information($"[{playerId}] tried to redeem token {tokenId}, but it was not in the token list!");
                return null;
            }
        }

        public static TokenResponse ReadTokens(string playerId)
            => ParseJsonFile<TokenResponse>(playerId, "tokens");
        private static void WriteTokens(string playerId, TokenResponse tokenList)
            => WriteJsonFile(playerId, tokenList, "tokens");
    }
}
