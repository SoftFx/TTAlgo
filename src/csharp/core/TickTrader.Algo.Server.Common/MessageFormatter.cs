using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Server.Common
{
    public class MessageFormatter
    {
        private static readonly string[] ExcludedFields = new[] { "password", "binary" };
        private static readonly string[] EscapedMessages = new[] { "data" };


        private JsonFormatter _formatter;


        public bool LogMessages { get; set; }


        public MessageFormatter(FileDescriptor serviceDescriptor)
        {
            _formatter = new JsonFormatter(new JsonFormatter.Settings(true, TypeRegistry.FromFiles(serviceDescriptor)));
        }


        public string ToJson(IMessage message) => message == null ? "{null}" : $"{message.Descriptor.Name}: {Format(message)}";

        public void LogMsgFromClient(ILogger logger, IMessage msg) => LogMessageInternal("client >", logger, msg);

        public void LogMsgToClient(ILogger logger, IMessage msg) => LogMessageInternal("client <", logger, msg);

        public void LogMsgToServer(ILogger logger, IMessage msg) => LogMessageInternal("server <", logger, msg);

        public void LogMsgFromServer(ILogger logger, IMessage msg) => LogMessageInternal("server >", logger, msg);

        public string FormatUpdateToClient(IMessage update, int packedSize, bool compressed)
        {
            if (!LogMessages)
                return null;

            return $"client < {update.Descriptor.Name}({packedSize} bytes{(compressed ? ", compressed" : "")}): {Format(update)}";
        }


        protected string Format(IMessage msg)
        {
            var escapedFields = new Dictionary<IFieldAccessor, object>();

            EscapeMessage(msg, escapedFields);

            var res = _formatter.Format(msg);

            RestoreMessage(msg, escapedFields);

            return res;
        }

        private void LogMessageInternal(string prefix, ILogger logger, IMessage msg)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"{prefix} {msg.Descriptor.Name}: {Format(msg)}");
            }
        }

        private void EscapeMessage(IMessage msg, Dictionary<IFieldAccessor, object> escapedFields)
        {
            foreach (var field in msg.Descriptor.Fields.InFieldNumberOrder())
            {
                if (field.FieldType == FieldType.Message && EscapedMessages.Any(f => field.Name.ToLower().Contains(f)))
                {
                    var innerMsg = (IMessage)field.Accessor.GetValue(msg);
                    if (innerMsg != null)
                    {
                        var innerEscapedFields = new Dictionary<IFieldAccessor, object>();
                        escapedFields.Add(field.Accessor, innerEscapedFields);
                        EscapeMessage(innerMsg, innerEscapedFields);
                    }
                }
                else if (ExcludedFields.Any(f => field.Name.ToLower().Contains(f)))
                {
                    escapedFields.Add(field.Accessor, field.Accessor.GetValue(msg));
                    field.Accessor.SetValue(msg, GetDefaultValue(field));
                }
            }
        }

        private void RestoreMessage(IMessage msg, Dictionary<IFieldAccessor, object> escapedFields)
        {
            foreach (var field in escapedFields)
            {
                if (field.Value is Dictionary<IFieldAccessor, object>)
                {
                    var innerMsg = (IMessage)field.Key.GetValue(msg);
                    if (innerMsg != null)
                        RestoreMessage(innerMsg, (Dictionary<IFieldAccessor, object>)field.Value);
                }
                else
                {
                    field.Key.SetValue(msg, field.Value);
                }
            }
        }

        private object GetDefaultValue(FieldDescriptor field)
        {
            if (field.FieldType == FieldType.Message && field.MessageType.ClrType == typeof(StringValue))
                return "***";

            switch (field.FieldType)
            {
                case FieldType.String:
                    return "***";
                case FieldType.Bytes:
                    return ByteString.FromBase64("somebytesA==");
                default:
                    throw new ArgumentException("Unsupported field type");
            }
        }
    }
}
