namespace TickTrader.Algo.Server.PublicAPI
{
    public partial class AccountCreds
    {
        private const string PasswordKey = "password";


        public AccountCreds(string scheme, string password)
        {
            AuthScheme = scheme;
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


    public partial class AccountMetadataRequest
    {
        public AccountMetadataRequest(string accountId)
        {
            AccountId = accountId;
        }
    }
}
