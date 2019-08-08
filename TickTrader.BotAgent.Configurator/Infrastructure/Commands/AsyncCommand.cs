using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TickTrader.BotAgent.Configurator
{
    public class AsyncCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Func<Task> ExecutedHandler { get; private set; }

        public Func<bool> CanExecuteHandler { get; private set; }

        public AsyncCommand(Func<Task> executedHandler, Func<bool> canExecuteHandler = null)
        {
            this.ExecutedHandler = executedHandler ?? throw new ArgumentNullException("executedHandler");
            this.CanExecuteHandler = canExecuteHandler;
        }

        public Task Execute()
        {
            return this.ExecutedHandler();
        }

        public bool CanExecute()
        {
            return this.CanExecuteHandler == null || this.CanExecuteHandler();
        }

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute();
        }

        async void ICommand.Execute(object parameter)
        {
            await this.Execute();
        }
    }
}
