namespace TickTrader.Algo.Domain
{
    public partial class AccountCreds
    {
        public const string SimpleAuthSchemeId = "simple";
        public const string PasswordKey = "password";


        public AccountCreds(string password)
        {
            AuthScheme = SimpleAuthSchemeId;
            SetPassword(password);
        }


        public string GetPassword()
        {
            return GetSecret(PasswordKey);
        }

        public void SetPassword(string password)
        {
            UpdateSecret(PasswordKey, password);
        }


        private string GetSecret(string key)
        {
            if (!Secret.TryGetValue(key, out var value))
                value = null;

            return value;
        }

        private bool UpdateSecret(string key, string value)
        {
            var secret = Secret;

            if (value == null && !secret.ContainsKey(key))
                return false;

            if (value == null)
                secret.Remove(key);
            else secret[key] = value;

            return true;
        }
    }
}
