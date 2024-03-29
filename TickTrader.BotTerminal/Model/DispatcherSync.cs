﻿using System;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    public class DispatcherSync : ISyncContext
    {
        public void Invoke(Action syncAction)
        {
            if (CheckAccess())
                syncAction();
            else App.Current.Dispatcher.Invoke(syncAction);
        }

        public void Invoke<T>(Action<T> syncAction, T args)
        {
            if (CheckAccess())
                syncAction(args);
            else App.Current.Dispatcher.Invoke(() => syncAction(args));
        }

        public T Invoke<T>(Func<T> syncFunc)
        {
            if (CheckAccess())
                return syncFunc();
            return App.Current.Dispatcher.Invoke<T>(syncFunc);
        }

        public TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args)
        {
            if (CheckAccess())
                return syncFunc(args);
            return App.Current.Dispatcher.Invoke<TOut>(() => syncFunc(args));
        }

        public void Send(Action asyncAction)
        {
            App.Current.Dispatcher.BeginInvoke(asyncAction);
        }

        private bool CheckAccess()
        {
            return App.Current.Dispatcher.CheckAccess();
        }
    }
}
