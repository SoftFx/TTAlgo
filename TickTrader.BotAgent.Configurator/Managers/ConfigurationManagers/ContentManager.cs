using Newtonsoft.Json.Linq;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public abstract class ContentManager
    {
        public virtual string SectionName { get; }

        public ContentManager(SectionNames sectionName)
        {
            SectionName = sectionName == SectionNames.None ? string.Empty : sectionName.ToString();
        }

        protected virtual void SaveProperty(JObject root, string property, string value)
        {
            if (!string.IsNullOrEmpty(SectionName))
            {
                InsertSection(root, new JProperty(SectionName, new JObject()));
                if (!InsertSection(root[SectionName] as JObject, new JProperty(property, value)))
                    root[SectionName][property] = value;
            }
            else
            {
                if (!InsertSection(root, new JProperty(property, value)))
                    root[property] = value;
            }
        }

        private bool InsertSection(JObject root, JProperty prop)
        {
            if (root.Children<JProperty>().Where(p => p.Name == prop.Name).Count() > 0)
                return false;

            root.Add(prop);
            return true;
        }
    }
}
