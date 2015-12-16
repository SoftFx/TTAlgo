using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class LoginDialogViewModel
    {
        public LoginDialogViewModel()
        {
            LoginPage = new LoginPageViewModel();
            ManageAccountsPage = new ManageAccountsPageViewModel();
        }

        public LoginPageViewModel LoginPage { get; private set; }
        public ManageAccountsPageViewModel ManageAccountsPage { get; private set; }
    }
}
