﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface IPluginContext
    {
        string InstanceId { get; }
        FeedProvider Feed { get; }
        AccountDataProvider AccountData { get; }
        SymbolProvider Symbols { get; }
        CurrencyList Currencies { get; }
        IPluginMonitor Logger { get; }
        TradeCommands TradeApi { get; }
        StatusApi StatusApi { get; }
        EnvironmentInfo Environment { get; }
        IHelperApi Helper { get; }
        ITimerApi TimerApi { get; }
        DiagnosticInfo Diagnostics { get; }
        bool IsStopped { get; }
        TimeFrames TimeFrame { get; }
        IndicatorProvider Indicators { get; }
        IDrawableApi DrawableApi { get; }
        void OnExit();
        void OnPluginThread(Action action);
        void BeginOnPluginThread(Action action);
        Task OnPluginThreadAsync(Action action);
        void SetFeedBufferSize(int newSize);
        double DefaultOptimizationMetric { get; }
    }

    internal interface IHelperApi
    {
        string FormatPrice(double price, int digits);
        string FormatPrice(double price, Symbol symbolInfo);

        double RoundVolumeDown(double volume, Symbol symbolInfo);
        double RoundVolumeUp(double volume, Symbol symbolInfo);
    }

    internal interface IPluginMonitor
    {
        IAlertAPI Alert { get; }

        void Print(string entry);
        void Print(string entry, object[] parameters);
        void PrintError(string entry);
        void PrintError(string entry, object[] parameters);
    }

    internal interface IPluginActivator
    {
        IPluginContext Activate(AlgoPlugin instance);
    }

    internal interface ITimerApi
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
        Timer CreateTimer(TimeSpan period, Action<Timer> callback);
        Task Delay(TimeSpan period);
    }

    public interface IAlertAPI
    {
        void Print(string message);
    }

    internal interface IDrawableApi
    {
        IDrawableCollection LocalCollection { get; }
    }
}
