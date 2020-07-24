using Machinarium.Var;
using System;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Converters;

namespace TickTrader.BotTerminal
{
    sealed class AssetViewModel
    {
        private readonly VarContext _varContext = new VarContext();

        private readonly PricePrecisionConverter<decimal> _currencyConverter;
        private readonly IAssetInfo _asset;

        public AssetViewModel(AssetInfo asset, CurrencyEntity info)
        {
            _asset = asset;

            _currencyConverter = new PricePrecisionConverter<decimal>(info?.Digits ?? 2);

            _asset.MarginUpdate += Update;

            Currency = _varContext.AddProperty(_asset.Currency);
            Amount = _varContext.AddProperty(_asset.Amount, displayConverter: _currencyConverter);
            Margin = _varContext.AddProperty(_asset.Margin, displayConverter: _currencyConverter);
            FreeAmount = _varContext.AddProperty(displayConverter: _currencyConverter);

            Update();
        }

        public Property<string> Currency { get; }
        public Property<decimal> Amount { get; }
        public Property<decimal> Margin { get; }
        public Property<decimal> FreeAmount { get; }

        private void Update()
        {
            Currency.Value = _asset.Currency;
            Amount.Value = _asset.Amount;
            Margin.Value = _asset.Margin;
            FreeAmount.Value = _asset.FreeAmount;
        }
    }
}
