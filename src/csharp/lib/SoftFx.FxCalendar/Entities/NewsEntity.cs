using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoftFx.FxCalendar.Models;

namespace SoftFx.FxCalendar.Entities
{
    [Table("News")]
    public class NewsEntity : INews
    {
        [Key]
        public int Id { get; set; }

        public DateTime DateUtc { get; set; }

        public string CurrencyCode { get; set; }
    }
}