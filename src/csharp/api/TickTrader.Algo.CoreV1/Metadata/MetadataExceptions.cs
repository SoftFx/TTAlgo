﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1.Metadata
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
        public Domain.Metadata.Types.MetadataErrorCode ErrorCode { get; private set; }

        public PropertyMetadataBase[] InvalidProperties { get; private set; }


        public AlgoMetadataException(Domain.Metadata.Types.MetadataErrorCode errorCode, IEnumerable<PropertyMetadataBase> invalidProperties = null)
            : base(CreateMessageDescription(errorCode, invalidProperties))
        {
            this.ErrorCode = errorCode;

            if (invalidProperties != null)
                this.InvalidProperties = invalidProperties.ToArray();
            else
                this.InvalidProperties = new PropertyMetadataBase[0];
        }


        private static string CreateMessageDescription(Domain.Metadata.Types.MetadataErrorCode errorCode, IEnumerable<PropertyMetadataBase> invalidProperties)
        {
            switch (errorCode)
            {
                case Domain.Metadata.Types.MetadataErrorCode.HasInvalidProperties:
                    StringBuilder builder = new StringBuilder();
                    builder.Append("Plugin error: invalid properties.");
                    if (invalidProperties != null)
                    {
                        foreach (var property in invalidProperties)
                            builder.Append("\tproperty ").Append(property.Id).Append(" - ").Append(property.Error);
                    }
                    return builder.ToString();
                case Domain.Metadata.Types.MetadataErrorCode.UnknownBaseType:
                    return "Plugin error: Invalid base class. Your plugin must be derived from Indicator or TradeBot API classes.";
                case Domain.Metadata.Types.MetadataErrorCode.IncompatibleApiVersion:
                    return "Plugin error: Incompatible api version. This client can't support newer api versions. Please update your client.";
                default:
                    return "Plugin error: unknown error!";
            }
        }
    }
}