﻿using System.Collections.Generic;

namespace TickTrader.Algo.Api
{
    public interface Symbol
    {
        string Name { get; }
        int Digits { get; }
        double Point { get; }
        double ContractSize { get; }
        double MaxTradeVolume { get; }
        double MinTradeVolume { get; }
        double TradeVolumeStep { get; }
        bool IsTradeAllowed { get; }
        bool IsNull { get; }
        string BaseCurrency { get; }
        Currency BaseCurrencyInfo { get; }
        string CounterCurrency { get; }
        Currency CounterCurrencyInfo { get; }
        double Bid { get; }
        double Ask { get; }
        Quote LastQuote { get; }
        double Commission { get; }
        double LimitsCommission { get; }
        CommissionChargeMethod CommissionChargeMethod { get; }
        CommissionChargeType CommissionChargeType { get; }
        CommissionType CommissionType { get; }
        double HedgingFactor { get; }
        double Slippage { get; }
        SlippageType SlippageType { get; }

        void Subscribe(int depth = 1);
        void Unsubscribe();
    }

    // Obsolete: server always uses OneWay
    public enum CommissionChargeMethod
    {
        OneWay,
        RoundTurn
    };

    // Obsolete: server always uses PerLot
    public enum CommissionChargeType
    {
        PerTrade,
        PerLot
    };

    public enum CommissionType
    {
        PerUnit,
        Percent,
        Absolute,
        PercentageWaivedCash,
        PercentageWaivedEnhanced,
        PerBond
    };

    public enum SlippageType
    {
        Percent,
        Pips,
    }

    public interface SymbolProvider
    {
        SymbolList List { get; }
        Symbol MainSymbol { get; }
    }

    public interface SymbolList : IEnumerable<Symbol>
    {
        Symbol this[string symbol] { get; }
    }
}
