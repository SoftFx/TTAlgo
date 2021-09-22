using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TickTrader.BotAgent.Configurator
{
    public class CredentialsManager : ContentManager, IWorkingManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public enum Properties { Login, Password, OtpSecret }

        public CredentialModel Dealer { get; }

        public CredentialModel Admin { get; }

        public CredentialModel Viewer { get; }

        private readonly List<IWorkingModel> _models;

        public CredentialsManager(SectionNames sectionName = SectionNames.None) : base(sectionName)
        {
            Admin = new CredentialModel(nameof(Admin));
            Dealer = new CredentialModel(nameof(Dealer));
            Viewer = new CredentialModel(nameof(Viewer));

            _models = new List<IWorkingModel>() { Admin, Dealer, Viewer };
        }

        public void UploadModels(List<JProperty> credentialsProp)
        {
            foreach (var prop in credentialsProp)
            {
                CredentialModel cred = null;

                if (prop.Name.StartsWith(Admin.Name))
                    cred = Admin;
                else if (prop.Name.StartsWith(Dealer.Name))
                    cred = Dealer;
                else if (prop.Name.StartsWith(Viewer.Name))
                    cred = Viewer;

                if (prop.Name.EndsWith(Properties.Login.ToString()))
                    cred.Login = prop.Value.ToString();
                else if (prop.Name.EndsWith(Properties.Password.ToString()))
                    cred.Password = prop.Value.ToString();
                else if (prop.Name.EndsWith(Properties.OtpSecret.ToString()))
                    cred.OtpSecret = prop.Value.ToString();
            }

            SetDefaultModelValues();
            UpdateCurrentModelValues();
        }

        public void SetDefaultModelValues()
        {
            foreach (var model in _models)
                model.SetDefaultValues();
        }

        public void UpdateCurrentModelValues()
        {
            foreach (var model in _models)
                model.UpdateCurrentFields();
        }

        public void SaveConfigurationModels(JObject obj)
        {
            foreach (var model in _models)
                SaveModels(obj, (CredentialModel)model);
        }

        private void SaveModels(JObject root, CredentialModel model)
        {
            SaveProperty(root, $"{model.Name}Login", model.Login, model.CurrentLogin, _logger);
            SaveProperty(root, $"{model.Name}Password", model.Password, model.CurrentPassword, _logger, true);
            if (!string.IsNullOrEmpty(model.OtpSecret))
                SaveProperty(root, $"{model.Name}OtpSecret", model.OtpSecret, model.CurrentOtpSecret, _logger, true);
        }
    }
}
