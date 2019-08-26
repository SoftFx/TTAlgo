namespace TickTrader.BotAgent.Configurator
{
    public class FdkModel : IWorkingModel
    {
        public bool EnableLogs { get; set; }

        public bool CurrentEnableLogs { get; set; }

        public void SetDefaultValues() { }

        public void UpdateCurrentFields()
        {
            CurrentEnableLogs = EnableLogs;
        }
    }
}
