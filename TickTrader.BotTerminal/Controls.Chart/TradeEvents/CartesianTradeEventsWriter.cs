using LiveChartsCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.BotTerminal.Controls.Chart.Markers;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public interface ITradeEventsWriter
    {
        List<ISeries> Markers { get; }

        event Action InitNewDataEvent;
    }


    internal sealed class TradeEventsWriter : ITradeEventsWriter
    {
        private readonly Dictionary<EventType, TradeEventSettings> _settings = new()
        {
            [EventType.OpenOrder] = new TradeEventSettings<ArrowUpMarker>(EventType.OpenOrder, SKColors.Blue)
            {
                GetPoint = TradePointsFactory.GetOpenMarker
            },

            [EventType.FillOrder] = new TradeEventSettings<ArrowDownMarker>(EventType.FillOrder, SKColors.Blue)
            {
                GetPoint = TradePointsFactory.GetFillMarker
            },

            [EventType.CloseOrder] = new TradeEventSettings<ArrowDownMarker>(EventType.CloseOrder, SKColors.DarkRed)
            {
                GetPoint = TradePointsFactory.GetCloseMarker
            },
        };


        public List<ISeries> Markers { get; } = new();

        public event Action InitNewDataEvent;


        public TradeEventsWriter()
        {
            foreach (var setting in _settings.Values)
                Markers.Add(setting.GetMarkerSeries());
        }


        public void LoadTradeEvents(IEnumerable<BaseTransactionModel> eventSource)
        {
            void AddEvent(EventType type, BaseTransactionModel report)
            {
                _settings[type].Events.Add(_settings[type].GetPoint(report));
            }

            _settings.ForEach(p => p.Value.Events.Clear());

            foreach (var report in eventSource)
            {
                switch (report.ActionType)
                {
                    case Algo.Domain.TradeReportInfo.Types.ReportType.OrderFilled:
                        AddEvent(EventType.FillOrder, report);
                        break;
                    case Algo.Domain.TradeReportInfo.Types.ReportType.PositionClosed:
                        AddEvent(EventType.OpenOrder, report);
                        AddEvent(EventType.CloseOrder, report);
                        break;
                    default:
                        break;
                }
            }

            InitNewDataEvent?.Invoke();
        }
    }
}