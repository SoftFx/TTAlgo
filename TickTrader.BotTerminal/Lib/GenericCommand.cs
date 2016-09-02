using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TickTrader.BotTerminal.Lib
{
    public class GenericCommand : ICommand
    {
        private bool isEnabled = true;
        private event EventHandler canExecuteChanged;
        private Action<object> execAction;

        public GenericCommand(Action<object> execAction)
        {
            this.execAction = execAction;
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    if (canExecuteChanged != null)
                        canExecuteChanged(this, EventArgs.Empty);
                }
            }
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { canExecuteChanged += value; }
            remove { canExecuteChanged -= value; }
        }


        bool ICommand.CanExecute(object parameter)
        {
            return isEnabled;
        }

        void ICommand.Execute(object parameter)
        {
            execAction(parameter);
        }
    }
}
