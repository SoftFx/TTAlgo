using SoftFx.FxCalendar.Models;

namespace SoftFx.FxCalendar.Calendar.FxStreet
{
    public interface IFxStreetNews : INews
    {
        ImpactLevel Impact { get; set; }
        string Category { get; set; }
        string Event { get; set; }
        string Link { get; }
        string Actual { get; set; }
        string Consensus { get; set; }
        string Previous { get; set; }
        string CountryCode { get; }
    }
}