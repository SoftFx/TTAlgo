using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TickTrader.Algo.Domain
{
    public class InvalidPluginType : AlgoException
    {
        public InvalidPluginType(string msg)
            : base(msg)
        {
        }
    }

    public class AlgoMetadataException : AlgoException
    {
        public Metadata.Types.MetadataErrorCode ErrorCode { get; private set; }

        public IPropertyDescriptor[] InvalidProperties { get; private set; }


        public AlgoMetadataException(Metadata.Types.MetadataErrorCode errorCode, IEnumerable<IPropertyDescriptor> invalidProperties = null)
            : base(CreateMessageDescription(errorCode, invalidProperties))
        {
            this.ErrorCode = errorCode;

            if (invalidProperties != null)
                this.InvalidProperties = invalidProperties.ToArray();
            else
                this.InvalidProperties = new IPropertyDescriptor[0];
        }


        public static string CreateMessageDescription(Metadata.Types.MetadataErrorCode errorCode, IEnumerable<IPropertyDescriptor> invalidProperties)
        {
            switch (errorCode)
            {
                case Metadata.Types.MetadataErrorCode.HasInvalidProperties:
                    StringBuilder builder = new StringBuilder();
                    builder.Append("Invalid properties.");
                    if (invalidProperties != null)
                    {
                        foreach (var property in invalidProperties)
                            builder.Append("\tproperty ").Append(property.Id).Append(" - ").Append(property.ErrorCode);
                    }
                    return builder.ToString();
                case Metadata.Types.MetadataErrorCode.UnknownBaseType:
                    return "Invalid base class. Your plugin must be derived from Indicator or TradeBot API classes.";
                case Metadata.Types.MetadataErrorCode.IncompatibleApiNewerVersion:
                    return "Incompatible api version. This client can't support newer api versions. Please update your client.";
                case Metadata.Types.MetadataErrorCode.IncompatibleApiOlderVersion:
                    return "Incompatible api version. This client can't support older api versions. Please update your plugin.";
                default:
                    return "Unknown metadata error!";
            }
        }
    }
}
