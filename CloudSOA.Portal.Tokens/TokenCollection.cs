using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSOA.Portal.Tokens
{
    [Serializable]
    public class TokenCollection
    {
        public TokenCollection()
        {
            AccessTokens = new Dictionary<string, AccessToken>();
        }

        public string RefreshToken { get; set; }

        public Dictionary<string, AccessToken> AccessTokens { get; private set; }

        public AccessToken GetNonExpiredAccessToken(string resourceId)
        {
            if (AccessTokens.ContainsKey(resourceId))
            {
                AccessToken accessToken = AccessTokens[resourceId];

                if (accessToken != null && !accessToken.Expired)
                {
                    return accessToken;
                }
            }

            return null;
        }
    }
}
