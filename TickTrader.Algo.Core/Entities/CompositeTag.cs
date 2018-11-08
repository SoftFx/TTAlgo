using Newtonsoft.Json;
using System;

namespace TickTrader.Algo.Core
{
    public class CompositeTag
    {
        [JsonConstructor]
        private CompositeTag()
        {
        }

        private CompositeTag(string isolationTag, string userTag)
        {
            if (isolationTag.Length > 30)
                throw new ArgumentException("Key too large. Must be no more than 30 characters.", nameof(isolationTag));

            Key = isolationTag;
            Tag = userTag;
        }
        [JsonProperty]
        public string Key { get; private set; }
        [JsonProperty]
        public string Tag { get; private set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static string NewTag(string isolationTag, string userTag)
        {
            return new CompositeTag(isolationTag, userTag).ToString();
        }

        public static string ExtarctUserTarg(string encodedTag)
        {
            CompositeTag tag;
            if (!TryParse(encodedTag, out tag))
                return "";
            return tag.Tag;
        }

        public static bool TryParse(string tag, out CompositeTag compositeTag)
        {
            try
            {
                compositeTag = null;
                if (tag?.StartsWith("{") ?? false) // hack to reduce number of exceptions
                {
                    compositeTag = JsonConvert.DeserializeObject<CompositeTag>(tag);
                }
                return compositeTag != null;
            }
            catch
            {
                compositeTag = null;
                return false;
            }
        }
    }
}
