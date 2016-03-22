using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TickTrader.BotTerminal
{
    /// <summary>
    /// Interaction logic for LoginPageView.xaml
    /// </summary>
    public partial class LoginPageView : UserControl
    {
        public LoginPageView()
        {
            InitializeComponent();
            Loaded += LoginPageView_Loaded;
        }

        private void LoginPageView_Loaded(object sender, RoutedEventArgs e)
        {
            var pwdContainer = DataContext as IPasswordContainer;

            if (pwdContainer != null)
            {
                this.PasswordInput.Password = pwdContainer.Password;
                this.PasswordInput.PasswordChanged += (s, a) => pwdContainer.Password = PasswordInput.Password;
                pwdContainer.PropertyChanged += (s, a) =>
                {
                    if (a.PropertyName == nameof(IPasswordContainer.Password) && pwdContainer.Password != PasswordInput.Password)
                        PasswordInput.Password = pwdContainer.Password;
                };
            }
        }
    }
}
