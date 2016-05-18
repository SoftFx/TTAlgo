using SoftFx.FxCalendar.Common;

namespace SoftFx.FxCalendar.Sources.FxStreet
{
    public interface IFxStreetNews : INews
    {
        string Category { get; set; }
        string Event { get; set; }
        string Link { get; }
        string Actual { get; set; }
        string Consensus { get; set; }
        string Previous { get; set; }
        string CountryCode { get; }
    }
}