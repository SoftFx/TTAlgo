using Caliburn.Micro;
using Google.Protobuf;
using System.Text.Json.Nodes;
using System.Text.Json;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class DrawableObjectViewModel : PropertyChangedBase
    {
        private static readonly JsonFormatter _jsonFormatter = new(new JsonFormatter.Settings(true));

        private DrawableObjectInfo _info;
        private string _jsonText;


        public string PluginId { get; }

        public string Name => _info.Name;

        public string JsonText
        {
            get
            {
                if (_jsonText is null)
                {
                    _jsonText = _jsonFormatter.Format(_info);
                    var node = JsonNode.Parse(_jsonText);
                    _jsonText = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                }

                return _jsonText;
            }
        }


        public DrawableObjectViewModel(string pluginId, DrawableObjectInfo info)
        {
            PluginId = pluginId;
            _info = info;
        }


        public void Update(DrawableObjectInfo info)
        {
            _info = info;
            _jsonText = null;
            NotifyOfPropertyChange(nameof(JsonText));
        }
    }
}
