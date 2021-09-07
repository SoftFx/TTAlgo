using Newtonsoft.Json.Linq;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public abstract class ContentManager
    {
        public virtual string SectionName { get; }

        public bool EnableManager { get; set; }


        public ContentManager(SectionNames sectionName)
        {
            SectionName = sectionName == SectionNames.None ? string.Empty : sectionName.ToString();
        }

        protected virtual void SaveProperty(JObject root, string property, object newValue, object oldValue, NLog.Logger logger, bool secret = false)
        {
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

            if (!newValue.Equals(oldValue))
            {
                if (!secret)
                    logger.Info(GetChangeMessage(property, oldValue, newValue));
                else
                    logger.Info(GetChangeMessage(property));
            }
        }

        private string GetChangeMessage(string property, object oldVal, object newVal)
        {
            return $"Property {property} was changed: {oldVal} to {newVal}";
        }

        private string GetChangeMessage(string property)
        {
            return $"Property {property} was changed";
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
