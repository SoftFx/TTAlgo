using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal enum EventType
    {
        OpenOrder,
        FillOrder,
        CloseOrder,
    }


    internal abstract record TradeEventSettings
    {
        internal ObservableCollection<EventPoint> Events { get; } = new();


        internal string Name { get; }

        internal SKColor Color { get; }


        internal Func<BaseTransactionModel, EventPoint> GetPoint { get; init; }


        internal TradeEventSettings(EventType type, SKColor color)
        {
            Name = $"{type}";
            Color = color;
        }


        internal abstract ISeries GetMarkerSeries();
    }


    internal sealed record TradeEventSettings<T> : TradeEventSettings where T : class, ISizedVisualChartPoint<SkiaSharpDrawingContext>, new()
    {
        internal TradeEventSettings(EventType type, SKColor color) : base(type, color)
        { }

        internal override ISeries GetMarkerSeries() => Customizer.GetScatterSeries<T>(this);
    }
}