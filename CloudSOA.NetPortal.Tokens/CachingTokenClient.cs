using Microsoft.IdentityModel.Clients.ActiveDirectory;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudSOA.NetPortal.Tokens
{
    public class CachingTokenClient
    {
        private AuthenticationContext _authContext;
        private ConnectionMultiplexer _connection;
        private ClientCredential _clientCredential;

        public CachingTokenClient(AuthenticationContext authContext, ClientCredential clientCredential, ConnectionMultiplexer connection)
        {
            _authContext = authContext;
            _connection = connection;
            _clientCredential = clientCredential;
        }

        public string GetAccessTokenFromAuthorizationCode(string userId, string authorizationCode, string redirectUri)
        {
            AuthenticationResult result = _authContext.AcquireTokenByAuthorizationCode(authorizationCode, new Uri(redirectUri), _clientCredential);

            TokenCollection userTokens = GetOrCreateCacheObject(userId);

            userTokens.RefreshToken = result.RefreshToken;

            SaveCacheObject(userId, userTokens);

            return result.AccessToken;
        }

        private TokenCollection GetOrCreateCacheObject(string userId)
        {
            IDatabase cache = _connection.GetDatabase();
            TokenCollection userTokens = null;

            try
            {
               userTokens  = cache.GetAs<TokenCollection>(userId);
            }
            catch (SerializationException ex)
            {
                // Happens when the .NET class is updated and old instances are kept in cache
                cache.KeyDelete(userId);
            }

            if (userTokens == null)
            {
                userTokens = new TokenCollection();
            }

            return userTokens;
        }

        private void SaveCacheObject(string userId, TokenCollection userTokens)
        {
            IDatabase cache = _connection.GetDatabase();
            cache.Set(userId, userTokens);
        }

        public string GetAccessTokenFromRefreshToken(string userId, string resourceId)
        {
            TokenCollection userTokens = GetOrCreateCacheObject(userId);

            AccessToken accessToken = userTokens.GetNonExpiredAccessToken(resourceId);

            if (accessToken == null)
            {
                if (string.IsNullOrEmpty(userTokens.RefreshToken))
                {
                    throw new Exception("No access token or refresh token available");
                }

                AuthenticationResult result = _authContext.AcquireTokenByRefreshToken(userTokens.RefreshToken, _clientCredential, resourceId);

                accessToken = new AccessToken(result.AccessToken, result.ExpiresOn);

                userTokens.AccessTokens[resourceId] = accessToken;
                userTokens.RefreshToken = result.RefreshToken;

                SaveCacheObject(userId, userTokens);
            }

            return accessToken.Token;
        }
    }
}
