using Google.Protobuf;
using Google.Protobuf.Reflection;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Protocol.Grpc
{
    internal class MessageFormatter
    {
        private static readonly string[] ExcludedFields = new[] { "password", "binary" };


        private JsonFormatter _formatter;


        public bool LogMessages { get; set; }


        public MessageFormatter()
        {
            _formatter = new JsonFormatter(new JsonFormatter.Settings(true));
        }


        public string ToJson(IMessage message)
        {
            return $"{message.GetType().Name}: {Format(message)}";
        }

        public void LogClientRequest(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger?.Info($"client > {request.GetType().Name}: {Format(request)}");
            }
        }

        public void LogClientResponse(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger?.Info($"client < {request.GetType().Name}: {Format(request)}");
            }
        }

        public void LogServerRequest(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger?.Info($"server < {request.GetType().Name}: {Format(request)}");
            }
        }

        public void LogServerResponse(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger?.Info($"server > {request.GetType().Name}: {Format(request)}");
            }
        }


        private string Format(IMessage msg)
        {
            var escapedFields = new List<Tuple<IFieldAccessor, object>>();

            foreach (var field in msg.Descriptor.Fields.InFieldNumberOrder())
            {
                if (ExcludedFields.Any(f => field.Name.ToLower().Contains(f)))
                {
                    escapedFields.Add(new Tuple<IFieldAccessor, object>(field.Accessor, field.Accessor.GetValue(msg)));
                    field.Accessor.SetValue(msg, GetDefaultValue(field.FieldType));
                }
            }

            var res = _formatter.Format(msg);

            foreach(var field in escapedFields)
            {
                field.Item1.SetValue(msg, field.Item2);
            }

            return res;
        }

        private object GetDefaultValue(FieldType type)
        {
            switch (type)
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
