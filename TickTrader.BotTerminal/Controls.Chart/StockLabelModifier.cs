using SciChart.Charting.ChartModifiers;
using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class StockLabelModifier : AxisModifierBase
    {
        private StockLabelProvider _labelProvider = new StockLabelProvider();

        public static readonly DependencyProperty TimeframeProperty =
            DependencyProperty.Register("Timeframe", typeof(Feed.Types.Timeframe), typeof(StockLabelModifier),
                new PropertyMetadata(Feed.Types.Timeframe.M1, OnTimeframeChanged));

        public Feed.Types.Timeframe Timeframe
        {
            get { return (Feed.Types.Timeframe)GetValue(TimeframeProperty); }
            set { SetValue(TimeframeProperty, value); }
        }

        private static void OnTimeframeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StockLabelModifier)d)._labelProvider.Timeframe = (Feed.Types.Timeframe)e.NewValue;
        }

        protected override void AttachTo(IAxis axis)
        {
            if (axis is CategoryDateTimeAxis)
                axis.LabelProvider = _labelProvider;
        }

        protected override void DeattachFrom(IAxis axis)
        {
            if (axis.LabelProvider == _labelProvider)
                axis.LabelProvider = null;
        }
    }
}
