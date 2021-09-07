namespace TickTrader.BotAgent.Configurator
{
    public class AlgoServerSettingsModel : IWorkingModel
    {
        public bool EnableDevMode { get; set; }

        public bool CurrentEnableDevModel { get; private set; }


        public void SetDefaultValues() { }

        public void UpdateCurrentFields()
        {
            CurrentEnableDevModel = EnableDevMode;
        }
    }
}
