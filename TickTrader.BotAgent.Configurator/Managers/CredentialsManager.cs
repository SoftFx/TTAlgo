using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Configurator
{
    public class CredentialsManager
    {
        private const string SectionName = "Credentials";

        public CredentialModel Dealer { get; }

        public CredentialModel Admin { get; }

        public CredentialModel Viewer { get; }

        public CredentialsManager()
        {
            Admin = new CredentialModel(nameof(Admin));
            Dealer = new CredentialModel(nameof(Dealer));
            Viewer = new CredentialModel(nameof(Viewer));
        }

        public void UploadCredentials(List<JProperty> credentialsProp)
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
                else
                    throw new Exception($"Unknown credential {prop.Name}");

                if (prop.Name.EndsWith("Login"))
                    cred.Login = prop.Value.ToString();
                else
                if (prop.Name.EndsWith("Password"))
                    cred.Password = prop.Value.ToString();
                else
                    throw new Exception($"Unknown property {prop.Name}");
            }
        }

        public void SaveCredentialsModels(JObject obj)
        {
            SaveModels(obj, Admin);
            SaveModels(obj, Dealer);
            SaveModels(obj, Viewer);
        }

        public void SaveModels(JObject obj, CredentialModel model)
        {
            obj[SectionName][model.Name + "Login"] = model.Login;
            obj[SectionName][model.Name + "Password"] = model.Password;
        }
    }

    public class CredentialModel
    {
        public string Name { get; }

        public string Login { get; set; }

        public string Password { get; set; }


        public CredentialModel(string name)
        {
            Name = name;
        }
    }
}
