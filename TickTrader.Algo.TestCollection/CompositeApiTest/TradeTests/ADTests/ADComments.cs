using Newtonsoft.Json;
using System.Collections.Generic;

namespace TickTrader.Algo.TestCollection.CompositeApiTest.ADComments
{
    public enum ADTokens { Reject, Confirm, Cancel, Activate }

    internal class ActionADComment
    {
        public string Type { get; }


        public ActionADComment(ADTokens type)
        {
            Type = type.ToString();
        }
    }


    internal sealed class OrderADComment : ActionADComment
    {
        public double? Price { get; }

        public double? Volume { get; }


        public OrderADComment(ADTokens type, double? volume = null, double? price = null) : base(type)
        {
            Price = price;
            Volume = volume;
        }
    }


    internal sealed class ADCommentsList : List<ActionADComment>
    {
        private static readonly OrderADComment _remaningActivationToken;


        public static string WithReject { get; }

        public static string WithConfirm { get; }


        static ADCommentsList()
        {
            _remaningActivationToken = new OrderADComment(ADTokens.Activate, null);

            WithReject = new ADCommentsList
            {
                new ActionADComment(ADTokens.Reject)
            }.ToString();

            WithConfirm = new ADCommentsList
            {
                new ActionADComment(ADTokens.Confirm)
            }.ToString();
        }

        private ADCommentsList() { }


        public static string WithActivate(double partialVolume) =>
            new ADCommentsList
            {
                new OrderADComment(ADTokens.Activate, partialVolume),
                _remaningActivationToken,
            }.ToString();


        public override string ToString()
        {
            return $"json:{JsonConvert.SerializeObject(this)}";
        }
    }
}
