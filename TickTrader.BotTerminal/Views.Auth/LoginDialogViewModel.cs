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
        public LoginDialogViewModel()
        {
        }

        public enum StartPageOptions { LogIn }

        public LoginDialogViewModel(AuthManager magaer, ConnectionModel connection, StartPageOptions startOptions = StartPageOptions.LogIn)
        {
            LoginPage = new LoginPageViewModel(magaer, connection);
            LoginPage.Done += () => TryClose();

            this.Items.Add(LoginPage);

            switch (startOptions)
            {
                case StartPageOptions.LogIn: ActivateItem(LoginPage); break;
            }
        }

        public LoginPageViewModel LoginPage { get; private set; }
    }

    internal interface ILoginDialogPage
    {
    }
}
