using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Server;

using static TickTrader.Algo.Domain.Metadata.Types;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal sealed class DynamicIndicatorObserver : PropertyChangedBase, IIndicatorObserver
    {
        public static int FreeSubWindowId;

        private readonly object _syncObj = new();
        private readonly Dictionary<(string, OutputTarget), int> _subWindowIdLookup = new();
        private readonly VarDictionary<int, OutputSubWindowViewModel> _subWindows = new();
        private readonly ChartHostProxy _chartHost;
        private readonly IVarList<OutputWrapper> _outputWrappers;
        private readonly int _digits;

        private bool _disposed;


        public OutputSubWindowViewModel Overlay { get; }

        public IObservableList<OutputSubWindowViewModel> SubWindows { get; }


        public DynamicIndicatorObserver(ChartHostProxy chartHost, int digits)
        {
            _chartHost = chartHost;
            _digits = digits;

            Overlay = new OutputSubWindowViewModel(_chartHost.Info.Timeframe, digits);
            _outputWrappers = chartHost.Outputs.Select(o => new OutputWrapper(this, o)).DisposeItems();
            SubWindows = _subWindows.TransformToList().DisposeItems().UseSyncContext().Chain().AsObservable();

            _ = UpdateLoop();
        }


        public void Dispose()
        {
            _disposed = true;
            _outputWrappers.Dispose();
            Overlay.Dispose();
            SubWindows.Dispose();
        }


        public void DumpPoints(string dirPath)
        {
            Overlay.DumpPoints(dirPath);
            foreach (var wnd in SubWindows)
                wnd.DumpPoints(dirPath);
        }


        private void AddOutput(OutputSeriesProxy output)
        {
            lock (_syncObj)
            {
                if (output.Descriptor.Target == OutputTarget.Overlay)
                {
                    Overlay.AddOutput(output);
                }
                else
                {
                    var key = (output.PluginId, output.Descriptor.Target);
                    if (!_subWindowIdLookup.TryGetValue(key, out var windowId))
                    {
                        windowId = FreeSubWindowId++;
                        _subWindowIdLookup.Add(key, windowId);
                        _subWindows[windowId] = new OutputSubWindowViewModel(_chartHost.Info.Timeframe, _digits);
                    }

                    var outputWnd = _subWindows[windowId];
                    outputWnd.AddOutput(output);
                }
            }
        }

        private void RemoveOutput(OutputSeriesProxy output)
        {
            lock (_syncObj)
            {
                if (output.Descriptor.Target == OutputTarget.Overlay)
                {
                    Overlay.RemoveOutput(output);
                }
                else
                {
                    var key = (output.PluginId, output.Descriptor.Target);
                    if (!_subWindowIdLookup.TryGetValue(key, out var windowId))
                        return;

                    var outputWnd = _subWindows[windowId];
                    outputWnd.RemoveOutput(output);

                    if (outputWnd.IsEmpty)
                    {
                        _subWindowIdLookup.Remove(key);
                        _subWindows.Remove(windowId);

                        outputWnd.Dispose();
                    }
                }
            }
        }

        private async Task UpdateLoop()
        {
            while (!_disposed)
            {
                UpdateOutputs();
                await Task.Delay(250);
            }
        }

        private void UpdateOutputs()
        {
            lock (_syncObj)
            {
                Overlay.UpdateOutputs();
                foreach (var wnd in _subWindows.Values)
                {
                    wnd.UpdateOutputs();
                }
            }
        }


        private sealed class OutputWrapper : IDisposable
        {
            private readonly DynamicIndicatorObserver _parent;
            private readonly OutputSeriesProxy _output;


            public OutputWrapper(DynamicIndicatorObserver parent, OutputSeriesProxy output)
            {
                _parent = parent;
                _output = output;

                _parent.AddOutput(output);
            }


            public void Dispose()
            {
                _parent.RemoveOutput(_output);
            }
        }
    }
}
