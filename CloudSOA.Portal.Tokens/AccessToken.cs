using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSOA.Portal.Tokens
{
    [Serializable]
    public class AccessToken
    {
        private DateTimeOffset _expirationTime;

        public AccessToken(string token, DateTimeOffset expirationTime)
        {
            Token = token;
            _expirationTime = expirationTime;
        }

        public string Token { get; private set; }

        public bool Expired
        {
            get { return _expirationTime < DateTimeOffset.Now; }
        }
    }
}
