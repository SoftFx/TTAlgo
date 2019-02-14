using Caliburn.Micro;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    internal static class VmActions 
    {
        public static Caliburn.Micro.IResult<bool?> ShowWin32Dialog(CommonDialog dlg)
        {
            return new LambdaResult<bool?>(c => dlg.ShowDialog(GetWnd(c)));
        }

        public static Caliburn.Micro.IResult ShowError(string message, string caption = null)
        {
            return ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static Caliburn.Micro.IResult<MessageBoxResult> ShowMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return new LambdaResult<MessageBoxResult>(c => MessageBox.Show(GetWnd(c), message, caption, buttons, icon));
        }

        private static Window GetWnd(CoroutineExecutionContext context)
        {
            if (context.View is Window)
                return (Window)context.View;
            else if (context.View is DependencyObject)
                return Window.GetWindow((DependencyObject)context.View);

            throw new Exception("Cannot locate window!");
        }

        private class LambdaResult : Caliburn.Micro.IResult
        {
            private Action<CoroutineExecutionContext> _action;

            public LambdaResult(Action<CoroutineExecutionContext> action)
            {
                _action = action ?? throw new ArgumentNullException();
            }

            public event EventHandler<ResultCompletionEventArgs> Completed;

            public void Execute(CoroutineExecutionContext context)
            {
                try
                {
                    _action(context);
                    Completed?.Invoke(this, new ResultCompletionEventArgs());
                }
                catch (Exception ex)
                {
                    Completed?.Invoke(this, new ResultCompletionEventArgs() { Error = ex });
                }
            }
        }

        private class LambdaResult<T> : IResult<T>
        {
            private Func<CoroutineExecutionContext, T> _action;

            public LambdaResult(Func<CoroutineExecutionContext, T> action)
            {
                _action = action ?? throw new ArgumentNullException();
            }

            public T Result { get; private set; }

            public event EventHandler<ResultCompletionEventArgs> Completed;

            public void Execute(CoroutineExecutionContext context)
            {
                try
                {
                    Result = _action(context);
                    Completed?.Invoke(this, new ResultCompletionEventArgs());
                }
                catch (Exception ex)
                {
                    Completed?.Invoke(this, new ResultCompletionEventArgs() { Error = ex });
                }
            }
        }
    }
}
