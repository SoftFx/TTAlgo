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
        InputIsNotDataSeries = 5,
        OutputIsNotDataSeries = 6,
        EmptyEnum = 7,
    }


    [Serializable]
    public class PropertyDescriptor
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public virtual AlgoPropertyTypes PropertyType => AlgoPropertyTypes.Unknown;

        public AlgoPropertyErrors Error { get; set; }

        public bool IsValid => Error == AlgoPropertyErrors.None;
    }
}
