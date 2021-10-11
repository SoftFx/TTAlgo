using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal class CommentActionModel
    {
        [JsonProperty]
        protected readonly string Type;

        public CommentActionModel(ADCases type)
        {
            Type = type.ToString();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class OrderCommentActionModel : CommentActionModel
    {
        [JsonProperty]
        private readonly double? Price;

        [JsonProperty]
        private readonly double? Volume;

        public OrderCommentActionModel(ADCases type, double? volume = null, double? price = null) : base(type)
        {
            Price = price;
            Volume = volume;
        }
    }

    internal class CommentModelManager : IEnumerable<CommentActionModel>
    {
        private List<CommentActionModel> _actions;

        public CommentModelManager()
        {
            _actions = new List<CommentActionModel>();
        }

        public void Add(CommentActionModel action)
        {
            _actions.Add(action);
        }

        public string GetComment() => $"json:{JsonConvert.SerializeObject(_actions)}";

        public IEnumerator<CommentActionModel> GetEnumerator() => _actions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _actions.GetEnumerator();
    }

    public enum ADCases { Reject, Confirm, Cancel, Activate }
}
