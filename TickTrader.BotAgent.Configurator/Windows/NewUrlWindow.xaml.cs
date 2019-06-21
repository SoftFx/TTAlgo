using System.Windows;

namespace TickTrader.BotAgent.Configurator
{
    public partial class NewUrlWindow : Window
    {
        public NewUrlWindow(ServerViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }
    }
}
