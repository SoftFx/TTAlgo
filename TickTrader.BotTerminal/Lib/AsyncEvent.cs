using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Lib
{
    public static class AsyncEvent
    {
        public static Task InvokeAsync<T>(this AsyncEventHandler<T> handler, object sender, T args, CancellationToken cancelToken)
        {
            var list = handler.GetInvocationList();
            Task[] handlerTasks = new Task[list.Length];

            for (int i = 0; i < list.Length; i++)
                handlerTasks[i] = ((AsyncEventHandler<T>)list[i])(sender, args, cancelToken);

            return Task.WhenAll(handlerTasks);
        }

        public static Task InvokeAsync(this AsyncEventHandler handler, object sender)
        {
            return InvokeAsync(handler, sender, CancellationToken.None);
        }

        public static Task InvokeAsync(this AsyncEventHandler handler, object sender, CancellationToken cancelToken)
        {
            if (handler != null)
            {
                var list = handler.GetInvocationList();
                Task[] handlerTasks = new Task[list.Length];

                for (int i = 0; i < list.Length; i++)
                    handlerTasks[i] = ((AsyncEventHandler)list[i])(sender, cancelToken);

                return Task.WhenAll(handlerTasks);
            }
            return Task.FromResult<object>(null);
        }
    }

    public delegate Task AsyncEventHandler<T>(object sender, T e, CancellationToken cancelToken);
    public delegate Task AsyncEventHandler(object sender, CancellationToken cancelToken);
}
