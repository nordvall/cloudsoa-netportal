using Microsoft.IdentityModel.Clients.ActiveDirectory;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSOA.NetPortal.Tokens
{
    public static class CachingTokenClientFactory
    {
        private static Dictionary<string, ConnectionMultiplexer> _connectionPool;
        
        static CachingTokenClientFactory()
        {
            _connectionPool = new Dictionary<string, ConnectionMultiplexer>();
        }

        public static CachingTokenClient CreateFromAppSettings(NameValueCollection settings)
        {
            AuthenticationContext authContext = CreateAuthenticationContext(settings);
            ClientCredential credential = CreateClientCredential(settings);
            ConnectionMultiplexer connection = CreateConnectionMultiplexer(settings);

            var tokenClient = new CachingTokenClient(authContext, credential, connection);

            return tokenClient;
        }

        private static AuthenticationContext CreateAuthenticationContext(NameValueCollection settings)
        {
            string authority = ReadConfigSettingOrThrow(settings, "ida:Authority");

            if (string.IsNullOrWhiteSpace(authority))
            {
                throw new ConfigurationErrorsException("AppSetting missing: ida:Authority");
            }

            var authContext = new AuthenticationContext(authority);

            return authContext;
        }

        private static ClientCredential CreateClientCredential(NameValueCollection settings)
        {
            string clientId = ReadConfigSettingOrThrow(settings, "ida:ClientID");
            string clientSecret = ReadConfigSettingOrThrow(settings, "ida:ClientSecret");

            var clientCredential = new ClientCredential(clientId, clientSecret);

            return clientCredential;
        }

        private static ConnectionMultiplexer CreateConnectionMultiplexer(NameValueCollection settings)
        {
            string hostname = ReadConfigSettingOrThrow(settings, "cache:hostname");
            string password = ReadConfigSettingOrThrow(settings, "cache:password");

            if (!_connectionPool.ContainsKey(hostname))
            {
                _connectionPool[hostname] = ConnectionMultiplexer.Connect(String.Format("{0},ssl=true,password={1}", hostname, password));
            }

            ConnectionMultiplexer connection = _connectionPool[hostname];

            return connection;
        }

        private static string ReadConfigSettingOrThrow(NameValueCollection settings, string configurationKey)
        {
            string configurationValue = settings[configurationKey];

            if (string.IsNullOrWhiteSpace(configurationValue))
            {
                throw new ConfigurationErrorsException("AppSetting missing: " + configurationKey);
            }

            return configurationValue;
        }
    }
}
