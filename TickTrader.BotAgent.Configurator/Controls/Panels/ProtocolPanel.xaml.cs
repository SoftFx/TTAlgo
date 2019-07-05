using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotAgent.Configurator.Controls
{
    public partial class ProtocolPanel : UserControl
    {
        public ProtocolPanel()
        {
            InitializeComponent();

            ////ValidationError validationError = new ValidationError(new ExceptionValidationRule(), listeningPortBox.GetBindingExpression(TextBox.TextProperty), "Error", new System.Exception()); 

            ////validationError.ErrorContent = "Error";

            ////Validation.MarkInvalid(listeningPortBox.GetBindingExpression(TextBox.TextProperty), validationError);

            ////var x = Validation.GetHasError(listeningPortBox);

            //listeningPortBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            //listeningPortBox.GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
            ////Validation.ClearInvalid(listeningPortBox.GetBindingExpression(TextBox.TextProperty));
            ////x = Validation.GetHasError(listeningPortBox);
        }

        private void ListeningPortBox_Error(object sender, ValidationErrorEventArgs e)
        {
            //if (e.Action == ValidationErrorEventAction.Added)
            //{
            //    MessageBox.Show("Error");
            //}
        }
    }
}
