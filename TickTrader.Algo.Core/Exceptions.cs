using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class MisconfigException : AlgoException
    {
        public MisconfigException(string message) : base(message)
        {
        }

        protected MisconfigException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }
    }

    [Serializable]
    public class StopOutException : AlgoException
    {
        public StopOutException(string message) : base(message)
        {
        }

        protected StopOutException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }
    }
}

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class AlgoException : Exception
    {
        public AlgoException(string msg)
            : base(msg)
        {
        }
    }

    [Serializable]
    public class InvalidPluginType : AlgoException
    {
        public InvalidPluginType(string msg)
            : base(msg)
        {
        }
    }

    [Serializable]
    public class AlgoMetadataException : AlgoException
    {
        public AlgoMetadataException(AlgoMetadataErrors errorCode, IEnumerable<PropertyMetadataBase> invalidProperties = null)
            : base(CreateMessageDescription(errorCode, invalidProperties))
        {
            this.ErrorCode = errorCode;

            if (invalidProperties != null)
                this.InvalidProperties = invalidProperties.ToArray();
            else
                this.InvalidProperties = new PropertyMetadataBase[0];
        }

        private static string CreateMessageDescription(AlgoMetadataErrors errorCode, IEnumerable<PropertyMetadataBase> invalidProperties)
        {
            switch (errorCode)
            {
                case AlgoMetadataErrors.HasInvalidProperties:
                    StringBuilder builder = new StringBuilder();
                    builder.Append("Plugin error: invalid properties.");
                    if (invalidProperties != null)
                    {
                        foreach (var property in invalidProperties)
                            builder.Append("\tproperty ").Append(property.Id).Append(" - ").Append(property.Error);
                    }
                    return builder.ToString();
                case AlgoMetadataErrors.UnknownBaseType:
                    return "Plugin error: Invalid base class. Your plugin must be derived from Indicator or TradeBot API classes.";
                case AlgoMetadataErrors.IncompatibleApiVersion:
                    return "Plugin error: Incompatible api version. This client can't support newer api versions. Please update your client.";
                default:
                    return "Plugin error: unknown error!";
            }
        }

        public AlgoMetadataErrors ErrorCode { get; private set; }
        public PropertyMetadataBase[] InvalidProperties { get; private set; }
    }
}
