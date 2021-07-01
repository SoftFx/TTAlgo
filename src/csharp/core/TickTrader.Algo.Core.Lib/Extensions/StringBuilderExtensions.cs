using System.Collections;
using System.Text;

namespace TickTrader.Algo.Core.Lib.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendList(this StringBuilder builder, IList collection, string collectionName = null)
        {
            if (!string.IsNullOrEmpty(collectionName))
                builder.AppendLine(collectionName);

            if (collection.Count == 0)
                builder.AppendLine("Empty list");
            else
                foreach (var item in collection)
                    builder.AppendLine(item.ToString());

            return builder;
        }
    }
}
