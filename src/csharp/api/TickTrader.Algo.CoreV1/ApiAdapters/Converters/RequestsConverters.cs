using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public static class RequestsConverters
    {
        public static Domain.OpenOrderRequest ToDomain(this Api.OpenOrderRequest apiRequest, string isolationTag)
        {
            var domainRequest = new Domain.OpenOrderRequest
            {
                Symbol = apiRequest.Symbol,
                Type = apiRequest.Type.ToDomainEnum(),
                Side = apiRequest.Side.ToDomainEnum(),
                Amount = apiRequest.Volume,
                MaxVisibleAmount = apiRequest.MaxVisibleVolume,
                Price = apiRequest.Price,
                StopPrice = apiRequest.StopPrice,
                StopLoss = apiRequest.StopLoss,
                TakeProfit = apiRequest.TakeProfit,
                Comment = apiRequest.Comment,
                Slippage = apiRequest.Slippage,
                ExecOptions = apiRequest.Options.ToDomainEnum(),
                Tag = CompositeTag.NewTag(isolationTag, apiRequest.Tag),
                Expiration = apiRequest.Expiration.ToUtcTicks(),
                OcoRelatedOrderId = apiRequest.OcoRelatedOrderId,
                OcoEqualVolume = apiRequest.OcoEqualVolume,
                OtoTrigger = apiRequest.OtoTrigger?.ToDomain(),
            };

            domainRequest.SubOpenRequests.AddRange(apiRequest.SubOpenRequests?.Select(u => u.ToDomain(isolationTag)));

            return domainRequest;
        }


        public static Domain.ModifyOrderRequest ToDomain(this Api.ModifyOrderRequest apiRequest, string isolationTag)
        {
            return new Domain.ModifyOrderRequest
            {
                OrderId = apiRequest.OrderId,
                CurrentAmount = double.NaN,
                NewAmount = apiRequest.Volume,
                MaxVisibleAmount = apiRequest.MaxVisibleVolume,
                AmountChange = double.NaN,
                Price = apiRequest.Price,
                StopPrice = apiRequest.StopPrice,
                StopLoss = apiRequest.StopLoss,
                TakeProfit = apiRequest.TakeProfit,
                Slippage = apiRequest.Slippage,
                Comment = apiRequest.Comment,
                Tag = apiRequest.Tag != null ? CompositeTag.NewTag(isolationTag, apiRequest.Tag) : null,
                Expiration = apiRequest.Expiration.ToUtcTicks(),
                ExecOptions = apiRequest.Options?.ToDomainEnum(),
                OcoRelatedOrderId = apiRequest.OcoRelatedOrderId,
                OcoEqualVolume = apiRequest.OcoEqualVolume,
                OtoTrigger = apiRequest.OtoTrigger?.ToDomain(),
            };
        }
    }
}
