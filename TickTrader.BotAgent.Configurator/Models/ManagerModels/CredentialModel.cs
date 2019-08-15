namespace TickTrader.BotAgent.Configurator
{
    public class CredentialModel : IWorkingModel
    {
        private const int PasswordLength = 10;

        public string Name { get; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string CurrentLogin { get; private set; }

        public string CurrentPassword { get; private set; }

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
        }

        public void GeneratePassword()
        {
            Password = CryptoManager.GetNewPassword(8);
        }

        public void GenerateNewLogin()
        {
            Login = $"{Name}_{CryptoManager.GetNewPassword(3)}";
        }
    }
}
