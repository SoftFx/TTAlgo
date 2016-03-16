using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class LoginDialogViewModel : Conductor<ILoginDialogPage>.Collection.OneActive
    {
        public enum StartPageOptions { LogIn, Accounts, Servers }

        public LoginDialogViewModel(StartPageOptions startOptions = StartPageOptions.LogIn)
        {
            LoginPage = new LoginPageViewModel();
            AccountsPage = new ManageAccountsPageViewModel();
            ServerPage = new ManageServersPageViewModel();

            this.Items.Add(LoginPage);
            this.Items.Add(AccountsPage);
            this.Items.Add(ServerPage);
        }

        public LoginPageViewModel LoginPage { get; private set; }
        public ManageAccountsPageViewModel AccountsPage { get; private set; }
        public ManageServersPageViewModel ServerPage { get; private set; }
    }


    internal interface ILoginDialogPage
    {
    }
}
