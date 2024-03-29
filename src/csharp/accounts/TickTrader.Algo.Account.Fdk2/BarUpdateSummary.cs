﻿using System;
using System.Linq;
using System.Text;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account.Fdk2
{
    internal class BarUpdateSummary
    {
        public string Symbol { get; set; }

        public bool IsReset { get; set; }

        public bool CloseOnly { get; set; }

        public double? AskClose { get; set; }

        public double? BidClose { get; set; }

        public double? AskVolumeDelta { get; set; }

        public double? BidVolumeDelta { get; set; }

        public BarUpdateDetails[] Details { get; set; }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Symbol}, reset = {IsReset}, closeOnly = {CloseOnly}, ask = {AskClose}, bid = {BidClose}, askVolDelta = {AskVolumeDelta}, bidVolDelta = {BidVolumeDelta}");
            if (Details?.Length > 0)
            {
                foreach (var d in Details)
                {
                    sb.AppendLine($"{d.Timeframe}, {d.MarketSide}: from={d.From}, open={d.Open}, high={d.High}, low={d.Low}, volume={d.Volume}");
                }
            }
            return sb.ToString();
        }


        public static BarUpdateSummary[] FromRemovedSymbols(string[] symbols)
        {
            return symbols.Select(smb => new BarUpdateSummary { Symbol = smb, IsReset = true }).ToArray();
        }
    }


    internal class BarUpdateDetails
    {
        public Feed.Types.Timeframe Timeframe { get; set; }

        public Feed.Types.MarketSide MarketSide { get; set; }

        public DateTime? From { get; set; }

        public double? Open { get; set; }

        public double? High { get; set; }

        public double? Low { get; set; }

        public double? Volume { get; set; }

        public bool HasAllProperties => From.HasValue && Open.HasValue && High.HasValue && Low.HasValue;


        public BarData CreateBarData(double close)
        {
            if (HasAllProperties && BarSampler.TryGet(Timeframe, out var sampler))
            {
                var boundaries = sampler.GetBar(new UtcTicks(From.Value));
                var data = new BarData(boundaries.Open, boundaries.Close)
                {
                    Close = close,
                    Open = Open.Value,
                    High = High.Value,
                    Low = Low.Value
                };

                if (Volume.HasValue) // Volume available only in TTS 1.57
                    data.RealVolume = Volume.Value;

                return data;
            }

            return null;
        }

        public void UpdateBarData(BarData data)
        {
            if (HasAllProperties && BarSampler.TryGet(Timeframe, out var sampler))
            {
                var boundaries = sampler.GetBar(new UtcTicks(From.Value));
                data.OpenTime = boundaries.Open;
                data.CloseTime = boundaries.Close;
                data.Open = Open.Value;
                data.High = High.Value;
                data.Low = Low.Value;

                if (Volume.HasValue) // Volume available only in TTS 1.57
                    data.RealVolume = Volume.Value;
            }
            else
            {
                if (High.HasValue)
                    data.High = High.Value;
                if (Low.HasValue)
                    data.Low = Low.Value;
            }
        }
    }
}
