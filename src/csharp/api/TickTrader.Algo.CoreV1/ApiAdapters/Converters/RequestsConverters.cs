using Google.Protobuf.WellKnownTypes;
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
                Expiration = apiRequest.Expiration?.ToUniversalTime().ToTimestamp(),
                OcoRelatedOrderId = apiRequest.OcoRelatedOrderId,
                OcoEqualVolume = apiRequest.OcoEqualVolume,
                OtoTrigger = apiRequest.OtoTrigger?.ToDomain(),
            };

            domainRequest.SubOpenRequests.AddRange(apiRequest.SubOpenRequests?.Select(u => u.ToDomain(isolationTag)));

            return domainRequest;
        }
    }
}
