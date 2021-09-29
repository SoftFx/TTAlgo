using Newtonsoft.Json.Linq;

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
            var target = GetTargetSection(root);

            var newValueToken = JToken.FromObject(newValue);
            if (!TryAddProperty(target, property, newValueToken))
                target[property] = newValueToken;

            if (!newValue.Equals(oldValue))
                logger.Info(secret ? GetChangeMessage(property) : GetChangeMessage(property, oldValue, newValue));
        }

        protected virtual void RemoveProperty(JObject root, string property, NLog.Logger logger)
        {
            var target = GetTargetSection(root);

            if (target.Remove(property))
                logger.Info(GetRemoveMessage(property));
        }


        private JObject GetTargetSection(JObject root)
        {
            if (string.IsNullOrEmpty(SectionName))
                return root;

            TryAddProperty(root, SectionName, new JObject());
            return root[SectionName] as JObject;
        }

        private bool TryAddProperty(JObject root, string propName, JToken value)
        {
            var prop = root.Property(propName);
            if (prop != null)
                return false;

            root.Add(new JProperty(propName, value));

            return true;
        }

        private string GetChangeMessage(string property, object oldVal, object newVal) => $"Property {property} was changed: {oldVal} to {newVal}";

        private string GetChangeMessage(string property) => $"Property {property} was changed";

        private string GetRemoveMessage(string property) => $"Property {property} was removed";
    }
}
