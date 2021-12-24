using SoftFx.FxCalendar.Filters;

namespace SoftFx.FxCalendar.Calendar.FxStreet
{
    public interface IFxStreetFilter : IFilter
    {
        ImpactLevel Impact { get; set; }
    }
}