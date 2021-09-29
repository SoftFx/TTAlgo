using Caliburn.Micro;
using Machinarium.Var;

namespace TickTrader.BotTerminal
{
    internal sealed class TwoFactorCodeDialogViewModel : Screen, IWindowModel
    {
        private readonly VarContext _context = new VarContext();


        public string QueryText { get; }

        public Validable<string> Code { get; set; }

        public BoolVar CanOk { get; }


        public TwoFactorCodeDialogViewModel(string serverName, string login)
        {
            DisplayName = "2FA Auth";

            QueryText = $"Please enter 2FA code for AlgoServer '{serverName}/{login}'";

            Code = _context.AddValidable<string>().MustBeNotEmpty();
            CanOk = !_context.HasError;
        }


        public void Ok()
        {
            TryCloseAsync(true);
        }

        public void Cancel()
        {
            TryCloseAsync(false);
        }
    }
}
