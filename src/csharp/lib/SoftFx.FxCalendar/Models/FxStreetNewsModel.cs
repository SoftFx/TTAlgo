using System;
using SoftFx.FxCalendar.Calendar.FxStreet;
using SoftFx.FxCalendar.Entities;

namespace SoftFx.FxCalendar.Models
{
    public class FxStreetNewsModel : IFxStreetNews, IModel<FxStreetNewsEntity>
    {
        public DateTime DateUtc { get; set; }
        public string CurrencyCode { get; protected set; }
        public ImpactLevel Impact { get; set; }
        public string Category { get; set; }
        public string Event { get; set; }
        public string Link { get; protected set; }
        public string Actual { get; set; }
        public string Consensus { get; set; }
        public string Previous { get; set; }
        public string CountryCode { get; protected set; }

        public FxStreetNewsModel(string currencyCode, string link, string countryCode)
        {
            CurrencyCode = currencyCode;
            Link = link;
            CountryCode = countryCode;
        }

        public FxStreetNewsEntity ConvertToEntity()
        {
            return new FxStreetNewsEntity
            {
                DateUtc = DateUtc,
                CurrencyCode = CurrencyCode,
                Impact = Impact,
                Category = Category,
                Event = Event,
                Link = Link,
                Actual = Actual,
                Consensus = Consensus,
                Previous = Previous,
                CountryCode = CountryCode
            };
        }

        public void InitFromEntity(FxStreetNewsEntity entity)
        {
            DateUtc = entity.DateUtc;
            CurrencyCode = entity.CurrencyCode;
            Impact = entity.Impact;
            Category = entity.Category;
            Event = entity.Event;
            Link = entity.Link;
            Actual = entity.Actual;
            Consensus = entity.Consensus;
            Previous = entity.Previous;
            CountryCode = entity.CountryCode;
        }
    }
}