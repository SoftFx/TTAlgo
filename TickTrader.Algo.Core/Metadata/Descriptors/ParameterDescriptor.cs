using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class FileFilterEntry
    {
        public string FileTypeName { get; }

        public string FileMask { get; }


        public FileFilterEntry() { }

        public FileFilterEntry(string name, string mask)
        {
            FileTypeName = name;
            FileMask = mask;
        }
    }


    [Serializable]
    public class ParameterDescriptor : PropertyDescriptor
    {
        public override AlgoPropertyTypes PropertyType => AlgoPropertyTypes.Parameter;

        public string DataType { get; set; }

        public string DefaultValue { get; set; }

        public bool IsRequired { get; set; }

        public bool IsEnum { get; set; }

        public List<string> EnumValues { get; set; }

        public List<FileFilterEntry> FileFilters { get; set; }


        public ParameterDescriptor()
        {
            EnumValues = new List<string>();
            FileFilters = new List<FileFilterEntry>();
        }
    }
}
