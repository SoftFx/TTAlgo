using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server.Common
{
    public class MessageFormatter
    {
        private static readonly string[] ExcludedFields = new[] { "password", "binary" };
        private static readonly string[] EscapedMessages = new[] { "chunk" };


        private JsonFormatter _formatter;


        public bool LogMessages { get; set; }


        public MessageFormatter(FileDescriptor serviceDescriptor)
        {
            _formatter = new JsonFormatter(new JsonFormatter.Settings(true, TypeRegistry.FromFiles(serviceDescriptor))); //передача
        }


        public string ToJson(IMessage message)
        {
            return $"{message.GetType().Name}: {Format(message)}";
        }

        public void LogClientRequest(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"client > {request.GetType().Name}: {Format(request)}");
            }
        }

        public void LogClientResponse(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"client < {request.GetType().Name}: {Format(request)}");
            }
        }

        public void LogClientUpdate(ILogger logger, IUpdateInfo updateInfo)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"client < {updateInfo.ValueMsg.Descriptor.Name}: {{ Type = {updateInfo.Type}, Value = {Format(updateInfo.ValueMsg)} }}");
            }
        }

        public void LogServerRequest(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"server < {request.GetType().Name}: {Format(request)}");
            }
        }

        public void LogServerResponse(ILogger logger, IMessage request)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"server > {request.GetType().Name}: {Format(request)}");
            }
        }

        public void LogServerUpdate(ILogger logger, IUpdateInfo updateInfo)
        {
            if (LogMessages && logger != null)
            {
                logger.Info($"server > {updateInfo.ValueMsg.Descriptor.Name}: {{ Type = {updateInfo.Type}, Value = {Format(updateInfo.ValueMsg)} }}");
            }
        }


        private string Format(IMessage msg)
        {
            var escapedFields = new Dictionary<IFieldAccessor, object>();

            EscapeMessage(msg, escapedFields);

            var res = _formatter.Format(msg);

            RestoreMessage(msg, escapedFields);

            return res;
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
