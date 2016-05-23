using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoftFx.FxCalendar.Calendar.FxStreet;

namespace SoftFx.FxCalendar.Entities
{
    [Table("FxStreetNews")]
    public class FxStreetNewsEntity : IFxStreetNews
    {
        [Key]
        public int Id { get; set; }

        public DateTime DateUtc { get; set; }

        public string Category { get; set; }

        public string Event { get; set; }

        public string Link { get; set; }

        public ImpactLevel Impact { get; set; }

        public string Actual { get; set; }

        public string Consensus { get; set; }

        public string Previous { get; set; }

        public string CountryCode { get; set; }

        public string CurrencyCode { get; set; }
    }
}
