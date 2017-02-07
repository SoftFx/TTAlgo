using System;
using SoftFx.FxCalendar.Entities;

namespace SoftFx.FxCalendar.Models
{
    public class NewsModel : INews, IModel<NewsEntity>
    {
        public DateTime DateUtc { get; set; }
        public string CurrencyCode { get; protected set; }

        public NewsModel(string currencyCode)
        {
            CurrencyCode = currencyCode;
        }

        public NewsEntity ConvertToEntity()
        {
            return new NewsEntity
            {
                CurrencyCode = CurrencyCode,
                DateUtc = DateUtc
            };
        }

        public void InitFromEntity(NewsEntity entity)
        {
            DateUtc = entity.DateUtc;
            CurrencyCode = entity.CurrencyCode;
        }
    }
}