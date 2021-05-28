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
        public enum StartPageOptions { LogIn }

        public LoginDialogViewModel(ConnectionManager cManager, AccountAuthEntry creds = null, StartPageOptions startOptions = StartPageOptions.LogIn)
        {
            DisplayName = "Log In";

            LoginPage = new LoginPageViewModel(cManager, creds);
            LoginPage.Done += () => TryCloseAsync();

            this.Items.Add(LoginPage);

            switch (startOptions)
            {
                case StartPageOptions.LogIn: ActivateItemAsync(LoginPage); break;
            }
        }

        public LoginPageViewModel LoginPage { get; private set; }

        public void Connect() => LoginPage.Connect();

        public void Stop() => TryCloseAsync();
    }

    internal interface ILoginDialogPage
    {
    }
}
