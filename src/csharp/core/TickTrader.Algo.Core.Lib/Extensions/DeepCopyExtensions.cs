using Newtonsoft.Json;

namespace TickTrader.Algo.Core.Lib
{
    public static class DeepCopyExtensions
    {
        public static T DeepCopy<T>(this T self)
        {
            if (self == null)
                return default;

            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
