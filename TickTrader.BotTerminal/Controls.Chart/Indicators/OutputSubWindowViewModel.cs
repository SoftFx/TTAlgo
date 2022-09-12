using LiveChartsCore;
using Machinarium.Qnil;
using System;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public sealed class OutputSubWindowViewModel : IDisposable
    {
        private readonly VarDictionary<string, IOutputSeriesViewModel> _outputs = new();
        private readonly IVarList<IOutputSeriesViewModel> _outputList;
        private readonly Feed.Types.Timeframe _timeframe;
        private readonly int _digits;


        public IObservableList<ISeries> Series { get; }

        public IVarList<ISeries> SeriesList { get; }

        public bool IsEmpty => _outputs.Count == 0;

        public ChartSettings Settings { get; } = new();


        public OutputSubWindowViewModel(Feed.Types.Timeframe timeframe, int digits)
        {
            _timeframe = timeframe;
            _digits = digits;

            _outputList = _outputs.TransformToList().DisposeItems();
            Series = _outputList.Select(o => o.Series).UseSyncContext().Chain().AsObservable();
            SeriesList = _outputList.Select(o => o.Series);
        }


        public void Dispose()
        {
            _outputList.Dispose();
            Series.Dispose();
            SeriesList.Dispose();
        }


        public void AddOutput(OutputSeriesProxy output)
        {
            var settings = GetSettings(output.Descriptor);

            _outputs.Add(output.SeriesId, new DynamicOutputSeriesViewModel(output, settings));
        }

        public void RemoveOutput(OutputSeriesProxy output)
        {
            if (_outputs.TryGetValue(output.SeriesId, out var outputVM))
            {
                _outputs.Remove(output.SeriesId);
            }
        }

        public void UpdateOutputs()
        {
            foreach (var output in _outputs.Values)
            {
                output.UpdatePoints();
            }
        }


        internal void AddOutput(OutputSeriesModel output)
        {
            var settings = GetSettings(output.Descriptor);

            _outputs.Add(output.Descriptor.Id, new StaticOutputSeriesViewModel(output, settings));
        }


        private IndicatorChartSettings GetSettings(OutputDescriptor descriptor)
        {
            var precision = descriptor.Precision;

            var settings = new IndicatorChartSettings
            {
                Name = descriptor.DisplayName,
                Precision = precision == -1 ? _digits : precision,
                Period = _timeframe,
            };

            Settings.Precision = Math.Max(Settings.Precision, settings.Precision);

            return settings;
        }
    }
}
