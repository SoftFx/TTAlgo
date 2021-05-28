using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TickTrader.BotTerminal.Infrustructure.Persistence
{
    public class PersistManager
    {
        private PersistNode _rootNode;
        private List<Type> _additionalTypes = new List<Type>();

        public PersistManager()
        {
            _rootNode = new PersistNode("", PersistModes.Static);
        }

        public IPersistNode RootNode => _rootNode;
        public bool IsLoaded => _rootNode.Page != null;

        public void AddSettingType<T>()
        {
            _additionalTypes.Add(typeof(T));
        }

        public void SaveToFile(string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Create))
                SaveToStream(file);
        }

        public void SaveToStream(Stream srcStream)
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Settings must be loaded before saving. At least LoadEmpty() must be called!");

            _rootNode.FireSaving();

            var xmlSerializer = new XmlSerializer(typeof(SettingsPage), _additionalTypes.ToArray());
            xmlSerializer.Serialize(srcStream, _rootNode.Page);
        }

        public void LoadEmpty()
        {
            _rootNode.FireLoaded(new SettingsPage());
        }

        public void LoadFormFile(string filePath)
        {
            try
            {
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    LoadFromStream(file);
            }
            catch (System.IO.FileNotFoundException)
            {
                LoadEmpty();
            }
        }

        public void LoadFromStream(Stream srcStream)
        {
            var xmlSerializer = new XmlSerializer(typeof(SettingsPage), _additionalTypes.ToArray());
            //Root = (PersistNode)xmlSerializer.Deserialize(srcStream);
            var page = (SettingsPage)xmlSerializer.Deserialize(srcStream);
            _rootNode.FireLoaded(page);
        }
    }
}
