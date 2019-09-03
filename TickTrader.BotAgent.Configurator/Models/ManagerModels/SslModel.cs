namespace TickTrader.BotAgent.Configurator
{
    public class SslModel : IWorkingModel
    {
        private const string DefaultFile = "certificate.ptx";

        public string File { get; set; }

        public string Password { get; set; } = "";

        public void SetDefaultValues()
        {
            File = File ?? DefaultFile;
        }

        public void UpdateCurrentFields()
        {
        }
    }
}
