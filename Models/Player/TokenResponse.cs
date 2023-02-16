using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MCE_API_SERVER.Models.Player
{
    public class TokenResponse
    {
        [JsonProperty("result")]
        public TokenResult Result { get; set; }

        public Updates updates { get; set; }

        public TokenResponse()
        {
            Result = new TokenResult { tokens = new Dictionary<Guid, Token>() };
        }
    }

    public class TokenResult
    {
        public Dictionary<Guid, Token> tokens { get; set; }
    }
}
