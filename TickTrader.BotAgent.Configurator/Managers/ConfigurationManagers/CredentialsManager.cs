using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TickTrader.BotAgent.Configurator
{
    public class CredentialsManager : ContentManager, IUploaderModels
    {
        public enum Properties { Login, Password }

        public CredentialModel Dealer { get; }

        public CredentialModel Admin { get; }

        public CredentialModel Viewer { get; }

        public CredentialsManager(SectionNames sectionName = SectionNames.None) : base(sectionName)
        { 
            Admin = new CredentialModel(nameof(Admin));
            Dealer = new CredentialModel(nameof(Dealer));
            Viewer = new CredentialModel(nameof(Viewer));
        }

        public void UploadModels(List<JProperty> credentialsProp)
        {
            foreach (var prop in credentialsProp)
            {
                CredentialModel cred = null;

                if (prop.Name.StartsWith(Admin.Name))
                    cred = Admin;
                else
                if (prop.Name.StartsWith(Dealer.Name))
                    cred = Dealer;
                else
                if (prop.Name.StartsWith(Viewer.Name))
                    cred = Viewer;

                if (prop.Name.EndsWith(Properties.Login.ToString()))
                    cred.Login = prop.Value.ToString();
                else
                if (prop.Name.EndsWith(Properties.Password.ToString()))
                    cred.Password = prop.Value.ToString();
            }

            SetDefaultModelValues();
        }

        public void SetDefaultModelValues()
        {
            Admin.SetDefaultValues();
            Dealer.SetDefaultValues();
            Viewer.SetDefaultValues();
        }

        public void SaveConfigurationModels(JObject obj)
        {
            SaveModels(obj, Admin);
            SaveModels(obj, Dealer);
            SaveModels(obj, Viewer);
        }

        private void SaveModels(JObject root, CredentialModel model)
        {
            SaveProperty(root, model.Name + "Login", model.Login);
            SaveProperty(root, model.Name + "Password", model.Password);
        }
    }

    public class CredentialModel
    {
        private const int PasswordLength = 10;

        public string Name { get; }

        public string Login { get; set; }

        public string Password { get; set; }


        public CredentialModel(string name)
        {
            Name = name;
        }

        public void SetDefaultValues()
        {
            if (string.IsNullOrEmpty(Login))
                Login = Name;

            if (string.IsNullOrEmpty(Password))
                Password = Name;
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
