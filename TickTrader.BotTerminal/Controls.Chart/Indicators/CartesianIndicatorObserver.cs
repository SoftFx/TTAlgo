using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using static TickTrader.Algo.Domain.Metadata.Types;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public interface IIndicatorObserver : IDisposable
    {
        OutputSubWindowViewModel Overlay { get; }

        IObservableList<OutputSubWindowViewModel> SubWindows { get; }
    }


    internal sealed class StaticIndicatorObserver : PropertyChangedBase, IIndicatorObserver
    {
        private readonly VarDictionary<OutputTarget, OutputSubWindowViewModel> _subWindows = new();


        public OutputSubWindowViewModel Overlay { get; private set; }

        public IObservableList<OutputSubWindowViewModel> SubWindows { get; }


        internal StaticIndicatorObserver()
        {
            SubWindows = _subWindows.TransformToList().DisposeItems().UseSyncContext().Chain().AsObservable();
        }


        public void Dispose()
        {
            Overlay?.Dispose();
            SubWindows.Dispose();
        }


        public void LoadIndicators(OutputModel output, int digits)
        {
            _subWindows.Clear();
            Overlay?.Dispose();
            Overlay = new OutputSubWindowViewModel(output.Config.Timeframe, digits);
            NotifyOfPropertyChange(nameof(Overlay));

            foreach (var pair in output.Series)
            {
                var name = pair.Key;
                var model = pair.Value;
                var target = model.Descriptor.Target;

                if (target == OutputTarget.Overlay)
                {
                    Overlay.AddOutput(model);
                }
                else
                {
                    if (!_subWindows.TryGetValue(target, out var subWindow))
                    {
                        subWindow = new OutputSubWindowViewModel(output.Config.Timeframe, digits);
                        _subWindows.Add(target, subWindow);
                    }

                    subWindow.AddOutput(model);
                }
            }
        }
    }
}