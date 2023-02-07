using Caliburn.Micro;
using Google.Protobuf;
using Machinarium.Qnil;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.IndicatorHost;

namespace TickTrader.BotTerminal
{
    internal class ObjectListViewModel : Screen
    {
        private readonly ChartHostProxy _chartHost;
        private readonly VarList<DrawableObjectViewModel> _drawables;

        private DrawableObjectViewModel _selectedObject;


        public IObservableList<DrawableObjectViewModel> DrawableObjects { get; }

        public DrawableObjectViewModel SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject == value)
                    return;

                _selectedObject = value;
                NotifyOfPropertyChange(nameof(SelectedObject));
            }
        }


        public ObjectListViewModel(ChartHostProxy chartHost)
        {
            _chartHost = chartHost;

            DisplayName = $"{chartHost.Info.Symbol}, {chartHost.Info.Timeframe} Object List";

            _drawables = new VarList<DrawableObjectViewModel>();
            DrawableObjects = _drawables.AsObservable();

            _drawables.Add(new DrawableObjectViewModel("<AlgoTerminal>", new DrawableObjectInfo("vLine", Drawable.Types.ObjectType.VerticalLine)));
            _drawables.Add(new DrawableObjectViewModel("<AlgoTerminal>", new DrawableObjectInfo("hLine", Drawable.Types.ObjectType.HorizontalLine)));
            _drawables.Add(new DrawableObjectViewModel("<AlgoTerminal>", new DrawableObjectInfo("triangle", Drawable.Types.ObjectType.Triangle)));
        }


        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            return base.TryCloseAsync(dialogResult);
        }
    }


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
    }
}
