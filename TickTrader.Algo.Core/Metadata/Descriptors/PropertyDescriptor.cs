using System;

namespace TickTrader.Algo.Core.Metadata
{
    public enum AlgoPropertyTypes
    {
        Unknown = 0,
        Parameter = 1,
        InputSeries = 2,
        OutputSeries = 3,
    }


    public enum AlgoPropertyErrors
    {
        None = 0,
        Unknown = 1,
        SetIsNotPublic = 2,
        GetIsNotPublic = 3,
        MultipleAttributes = 4,
        OutputIsOnlyForIndicators = 5,
        InputIsOnlyForIndicators = 6,
        InputIsNotDataSeries = 7,
        OutputIsNotDataSeries = 8,
        EmptyEnum = 9,
    }


    [Serializable]
    public class PropertyDescriptor
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public AlgoPropertyErrors Error { get; set; }

        public bool IsValid => Error == AlgoPropertyErrors.None;

        public virtual AlgoPropertyTypes PropertyType => AlgoPropertyTypes.Unknown;
    }
}
