using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TickTrader.BotTerminal
{
    public class InputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
    {
        public static readonly DependencyProperty InputBindingProperty = DependencyProperty.Register("InputBinding", typeof(InputBinding), typeof(InputBindingTrigger),
            new UIPropertyMetadata(null));

        public InputBindingTrigger()
        {

        }

        public InputBinding InputBinding
        {
            get => (InputBinding)GetValue(InputBindingProperty);
            set => SetValue(InputBindingProperty, value);
        }


        protected override void OnAttached()
        {
            if (InputBinding != null)
            {
                InputBinding.Command = this;
                AssociatedObject.InputBindings.Add(InputBinding);
            }
            base.OnAttached();
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            // action is anyway blocked by Caliburn at the invoke level
            return true;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            InvokeActions(parameter);
        }

        #endregion
    }
}
