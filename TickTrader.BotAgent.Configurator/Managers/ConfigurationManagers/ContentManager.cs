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

        protected virtual void SaveProperty(JObject root, string property, object newValue, object oldValue, NLog.Logger logger = null)
        {
            if (newValue.Equals(oldValue))
                return;

            if (!string.IsNullOrEmpty(SectionName))
            {
                InsertSection(root, new JProperty(SectionName, new JObject()));
                if (!InsertSection(root[SectionName] as JObject, new JProperty(property, newValue)))
                    root[SectionName][property] = JToken.FromObject(newValue);
            }
            else
            {
                if (!InsertSection(root, new JProperty(property, newValue)))
                    root[property] = JToken.FromObject(newValue);
            }

            if (logger != null)
                logger.Info(GetChangeMessage(property, oldValue, newValue));
        }

        private string GetChangeMessage(string property, object oldVal, object newVal)
        {
            return $"{property} was changed: {oldVal} to {newVal}";
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
