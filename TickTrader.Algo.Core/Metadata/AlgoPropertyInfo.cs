using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Metadata
{
    /// <summary>
    /// This is serializable light copy of AlgoPropertyDescriptor.
    /// It is used to bypass descriptor to another application domain.
    /// </summary>
    [Serializable]
    public class AlgoPropertyInfo
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public virtual AlgoPropertyTypes PropertyType { get; set; }
        public AlgoPropertyErrors? Error { get; set; }
        public bool IsValid { get { return Error == null; } }
    }

    [Serializable]
    public class ParameterInfo : AlgoPropertyInfo
    {
    }

    [Serializable]
    public class InputInfo : AlgoPropertyInfo
    {
    }

    [Serializable]
    public class OutputInfo : AlgoPropertyInfo
    {
    }

}
