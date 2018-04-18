using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal class BacktesterSymbolSetupViewModel : PropertyChangedBase
    {
        public BacktesterSymbolSetupViewModel(bool primary, IObservableList<SymbolData> symbols)
        {
            IsPrimary = primary;

            AvailableSymbols = symbols;

            SelectedTimeframe = new Property<TimeFrames>();
            SelectedPriceType = new Property<BarPriceType>();
            SelectedSymbol = new Property<SymbolData>();

            SelectedTimeframe.Value = TimeFrames.M1;
            SelectedPriceType.Value = BarPriceType.Bid;
        }
        
        public bool IsPrimary { get; private set; }
        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public IEnumerable<BarPriceType> AvailablePriceTypes => EnumHelper.AllValues<BarPriceType>();
        public IObservableList<SymbolData> AvailableSymbols { get; }
        public Property<SymbolData> SelectedSymbol { get; }
        public Property<TimeFrames> SelectedTimeframe { get; }
        public Property<BarPriceType> SelectedPriceType { get; }

        public void Add() => OnAdd?.Invoke();
        public void Remove() => Removed?.Invoke(this);

        public event System.Action<BacktesterSymbolSetupViewModel> Removed;
        public event System.Action OnAdd;
    }
}
