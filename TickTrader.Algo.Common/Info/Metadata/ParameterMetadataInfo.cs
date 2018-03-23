using System.Collections.Generic;

namespace TickTrader.Algo.Common.Info
{
    public class ParameterMetadataInfo : PropertyMetadataInfo
    {
        public bool IsRequired { get; set; }

        public string DataType { get; set; }

        public object DefaultValue { get; set; }


        public bool IsEnum { get; set; }

        public List<string> EnumValues { get; private set; }


        public List<FileFilterInfo> FileFilters { get; private set; }
    }


    public class FileFilterInfo
    {
        public string FileTypeName { get; set; }

        public string FileMask { get; set; }
    }
}
