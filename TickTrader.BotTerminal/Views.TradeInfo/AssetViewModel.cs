using Machinarium.Var;
using System;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Converters;

namespace TickTrader.BotTerminal
{
    sealed class AssetViewModel : IDisposable
    {
        private readonly VarContext _varContext = new VarContext();

        private readonly PricePrecisionConverter<double> _currencyConverter;
        private readonly IAssetInfo _asset;


        public Property<string> Currency { get; }
        public Property<double> Amount { get; }
        public Property<double> Margin { get; }
        public Property<double> FreeAmount { get; }


        public AssetViewModel(AssetInfo asset, CurrencyInfo info)
        {
            _asset = asset;

            _currencyConverter = new PricePrecisionConverter<double>(info?.Digits ?? 2);

            _asset.MarginUpdate += Update;

            Currency = _varContext.AddProperty(_asset.Currency);
            Amount = _varContext.AddProperty(_asset.Amount, displayConverter: _currencyConverter);
            Margin = _varContext.AddProperty(_asset.Margin, displayConverter: _currencyConverter);
            FreeAmount = _varContext.AddProperty(displayConverter: _currencyConverter);

            Update();
        }


        public void Dispose()
        {
            _asset.MarginUpdate -= Update;
        }


        private void Update()
        {
            Currency.Value = _asset.Currency;
            Amount.Value = _asset.Amount;
            Margin.Value = _asset.Margin;
            FreeAmount.Value = _asset.FreeAmount;
        }
    }
}
