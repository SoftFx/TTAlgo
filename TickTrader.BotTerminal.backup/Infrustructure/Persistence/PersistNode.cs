using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Infrustructure.Persistence
{
    public class PersistNode : IPersistNode, SettingsReader, SettingsWriter, IDisposable
    {
        private Action<SettingsReader> _loadAction;
        private Action<SettingsWriter> _saveAction;
        private bool _initialized;
        private Dictionary<string, SettingProxy> _proxies;

        public PersistNode(string id, PersistModes mode)
        {
            Id = id;
            Mode = mode;
        }

        public string Id { get; }
        public PersistModes Mode { get; }
        public SettingsPage Page { get; private set; }
        public bool IsLoaded => Page != null;
        public List<PersistNode> NestedNodes { get; } = new List<PersistNode>();

        public event Action<PersistNode> Disposed;

        public IPersistNode GetNode(string nodeId, PersistModes mode)
        {
            if (nodeId == null)
                throw new ArgumentNullException("nodeId");

            var node = NestedNodes.FirstOrDefault(n => n.Id == nodeId);

            if (node == null)
            {
                node = new PersistNode(nodeId, mode);
                NestedNodes.Add(node);

                node.Disposed += Node_Disposed;

                if (IsLoaded)
                    node.FireLoaded(GetNestedPage(nodeId));
            }

            return node;
        }

        private void Node_Disposed(PersistNode nestedNode)
        {
            RemoveNode(nestedNode, nestedNode.Mode == PersistModes.Dynamic);
        }

        private void RemoveNode(PersistNode nestedNode, bool removePage)
        {
            nestedNode.Disposed -= Node_Disposed;

            NestedNodes.Remove(nestedNode);

            if (removePage && IsLoaded && Page.NestedPages != null)
                Page.NestedPages.RemoveAll(p => p.Id == nestedNode.Id);
        }

        public void Init(Action<SettingsReader> loadAction, Action<SettingsWriter> saveAction)
        {
            InternalInit(loadAction ?? throw new ArgumentNullException("loadAction"),
                saveAction ?? throw new ArgumentNullException("saveAction"));
        }

        public void Init()
        {
            InternalInit(null, null);
        }

        private void InternalInit(Action<SettingsReader> loadAction, Action<SettingsWriter> saveAction)
        {
            if (_initialized)
                throw new InvalidOperationException("Node with id=" + Id + " is already in use!");

            _loadAction = loadAction;
            _saveAction = saveAction;

            _initialized = true;

            if (IsLoaded)
                OnLoaded();
        }

        public void FireLoaded(SettingsPage page)
        {
            Page = page;

            Page.OnDeserializing();

            foreach (var nested in NestedNodes)
                nested.FireLoaded(GetNestedPage(nested.Id));

            if (_initialized)
                OnLoaded();
        }

        public void FireSaving()
        {
            SerializeProxies();

            _saveAction?.Invoke(this);

            Page.OnSerializing();

            foreach (var nestedNode in NestedNodes)
                nestedNode.FireSaving();
        }

        public void Dispose()
        {
            _initialized = false;
            _loadAction = null;
            _saveAction = null;
            Disposed?.Invoke(this);
        }

        public FlatSettingProxy this[string name] => GetOrAddFlatProxy(name);

        #region SettingsReader

        public IEnumerable<string> NestedNodeIds => Page?.NestedPages?.Select(p => p.Id) ?? Enumerable.Empty<string>();

        public string Get(string settingId, string defValue)
        {
            if (GetFlatValue(settingId, out string val))
                return val;
            return defValue;
        }

        public int Get(string settingId, int defValue)
        {
            if (GetFlatValue(settingId, out string strVal))
            {
                if (SettingConverter.ToInt32(strVal, out var intVal))
                    return intVal;
            }

            return defValue;
        }

        public double Get(string settingId, double defValue)
        {
            if (GetFlatValue(settingId, out string strVal))
            {
                if (SettingConverter.ToDouble(strVal, out var doubleVal))
                    return doubleVal;
            }

            return defValue;
        }

        public bool Get(string settingId, bool defValue)
        {
            if (GetFlatValue(settingId, out string strVal))
            {
                if (SettingConverter.ToBool(strVal, out var doubleVal))
                    return doubleVal;
            }

            return defValue;
        }

        public T Get<T>(string settingId)
        {
            var entry = GetEntry(settingId) as ComplexSettingEntry;
            if (entry != null && entry.Val is T)
                return (T)entry.Val;
            return default(T);

        }

        public bool TryGet<T>(string settingId, out T complexValue)
        {
            var entry = GetEntry(settingId) as ComplexSettingEntry;
            if (entry != null && entry.Val is T)
            {
                complexValue = (T)entry.Val;
                return true;
            }
            complexValue = default(T);
            return false;
        }

        #endregion

        #region SettingsWriter

        public void Set(string settingId, string settingVal)
        {
            SetFlatValue(settingId, settingVal);
        }

        public void Set(string settingId, int settingVal)
        {
            SetFlatValue(settingId, SettingConverter.ToString(settingVal));
        }

        public void Set(string settingId, double settingVal)
        {
            SetFlatValue(settingId, SettingConverter.ToString(settingVal));
        }

        public void Set(string settingId, bool settingVal)
        {
            SetFlatValue(settingId, SettingConverter.ToString(settingVal));
        }

        public void Set(string settingId, object complexVal)
        {
            Page.Set(new ComplexSettingEntry(settingId, complexVal));
        }

        #endregion

        private SettingsPage GetNestedPage(string nestedPageId)
        {
            if (Page.NestedPages == null)
                Page.NestedPages = new List<SettingsPage>();

            var nestedPage = Page.NestedPages.FirstOrDefault(p => p.Id == nestedPageId);

            if (nestedPage == null)
            {
                nestedPage = new SettingsPage();
                nestedPage.Id = nestedPageId;
                Page.NestedPages.Add(nestedPage);
            }

            return nestedPage;
        }

        private void OnLoaded()
        {
            DeserializeProxies();

            _loadAction?.Invoke(this);
        }

        private FlatSettingProxy GetOrAddFlatProxy(string settingName)
        {
            if (_proxies == null)
                _proxies = new Dictionary<string, SettingProxy>();

            var hasProxy = _proxies.TryGetValue(settingName, out var proxy);
            var flatProxy = proxy as FlatSettingProxy;

            if (hasProxy && flatProxy != null)
                return flatProxy;

            flatProxy = new FlatSettingProxy(settingName);
            flatProxy.Deserialize(GetEntry(settingName));
            _proxies.Add(settingName, flatProxy);
            return flatProxy;
        }

        private SettingEntry GetEntry(string settingName)
        {
            if (Page != null && Page.TryGetEntry(settingName, out var entry))
                return entry;
            return null;
        }

        private void SerializeProxies()
        {
            if (_proxies != null)
            {
                foreach (var proxy in _proxies.Values)
                    Page.Set(proxy.Serialize());
            }
        }

        private void DeserializeProxies()
        {
            if (_proxies != null)
            {
                foreach (var proxy in _proxies.Values)
                {
                    Page.TryGetEntry(proxy.Name, out var entry);
                    proxy.Deserialize(entry);
                }
            }
        }

        private bool GetFlatValue(string settingName, out string val)
        {
            Page.TryGetEntry(settingName, out var entry);
            var flatEntry = entry as FlatSettingEntry;
            if (flatEntry == null || flatEntry.Val == null)
            {
                val = null;
                return false;
            }

            val = flatEntry.Val;
            return true;
        }

        private void SetFlatValue(string settingName, string value)
        {
            Page.Set(new FlatSettingEntry(settingName, value));
        }
    }
}
