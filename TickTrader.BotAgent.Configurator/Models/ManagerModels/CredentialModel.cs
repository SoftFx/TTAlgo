namespace TickTrader.BotAgent.Configurator
{
    public class CredentialModel
    {
        public string Name { get; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string OtpSecret { get; set; }

        public string CurrentLogin { get; private set; }

        public string CurrentPassword { get; private set; }

        public string CurrentOtpSecret { get; private set; }

        public bool OtpEnabled => !string.IsNullOrEmpty(OtpSecret);

        public CredentialModel(string name)
        {
            Name = name;
        }

        public void SetDefaultValues()
        {
            Login = Login ?? Name;
            Password = Password ?? Name;
        }

        public void UpdateCurrentFields()
        {
            CurrentLogin = Login;
            CurrentPassword = Password;
            CurrentOtpSecret = OtpSecret;
        }

        public void ResetValues()
        {
            Login = null;
            Password = null;
            OtpSecret = null;
        }

        public void GeneratePassword()
        {
            Password = CryptoManager.GetNewPassword(8);
        }

        public void GenerateNewLogin()
        {
            Login = $"{Name}_{CryptoManager.GetNewPassword(3)}";
        }

        public void GenerateNewOtpSecret()
        {
            OtpSecret = CryptoManager.GenerateOtpSecret(40);
        }

        public void RemoveOtpSecret()
        {
            OtpSecret = null;
        }
    }
}
