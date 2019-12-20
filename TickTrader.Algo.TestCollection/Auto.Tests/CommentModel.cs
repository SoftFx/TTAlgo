using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    internal abstract class CommentActionModel
    {
        [JsonProperty]
        protected readonly string Type;

        protected CommentActionModel(string type)
        {
            Type = type;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class RejectCommentModel : CommentActionModel
    {
        public RejectCommentModel() : base("Reject")
        {

        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class ConfirmCommentModel : CommentActionModel
    {
        public ConfirmCommentModel() : base("Confirm")
        {

        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class CancelCommentModel : CommentActionModel
    {
        public CancelCommentModel() : base("Cancel")
        {

        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class ActivateCommentModel : CommentActionModel
    {
        [JsonProperty]
        private readonly double? Price;

        [JsonProperty]
        private readonly double? Volume;

        public ActivateCommentModel(double? price, double? volume) : base("Activate")
        {
            Price = price;
            Volume = volume;
        }
    }

    internal class CommentModel
    {
        private List<CommentActionModel> actions;

        public CommentModel()
        {
            actions = new List<CommentActionModel>();
        }

        public void Add(CommentActionModel action)
        {
            actions.Add(action);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(actions);
        }
    }
}
