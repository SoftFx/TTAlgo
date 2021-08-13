using System.Collections.Generic;

namespace TickTrader.Algo.Server.PublicAPI
{
    public partial class AccountCreds
    {
        private const string SimpleAuthSchemeId = "simple";
        private const string PasswordKey = "password";


        public AccountCreds(string scheme, IDictionary<string, string> secret)
        {
            AuthScheme = scheme;
            Secret.Add(secret);
        }

        public AccountCreds(string password)
        {
            AuthScheme = SimpleAuthSchemeId;
            SetPassword(password);
        }


        public void SetPassword(string password)
        {
            UpdateSecret(PasswordKey, password);
        }

        private bool UpdateSecret(string key, string value)
        {
            var secret = Secret;

            if (value == null && !secret.ContainsKey(key))
                return false;

            if (value == null)
                secret.Remove(key);
            else
                secret[key] = value;

            return true;
        }
    }


    public partial class ConnectionErrorInfo
    {
        public bool IsSuccessful => Code == Types.ErrorCode.NoConnectionError;
    }
}
