using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;

namespace TickTrader.Algo.Async.Tests
{
    [TestClass]
    public class ActorAskTests
    {
        private ConsumerActor[] _consumers;
        private ProducerActor[] _producers;


        public static IEnumerable<object[]> TestCases => ActorProducerConsumerParams.TestCases.Select(p => new object[] { p });

        [TestMethod]
        [DynamicData(nameof(TestCases))]
        public void ProducerConsumerCounter(ActorProducerConsumerTestParams p)
        {
            ActorErrorException lastError = null;
            ActorFailedException lastFailure = null;
            var sub1 = ActorSystem.ActorErrors.Subscribe(ex => lastError = ex);
            var sub2 = ActorSystem.ActorFailed.Subscribe(ex => lastFailure = ex);

            SpawnSystem(p);

            if (lastError != null)
                throw lastError;
            if (lastFailure != null)
                throw lastFailure;

            Assert.AreEqual(SendMessages(p).Result, p.ExpectedResult);

            sub1.Dispose();
            sub2.Dispose();

            if (lastError != null)
                throw lastError;
            if (lastFailure != null)
                throw lastFailure;
        }


        private void SpawnSystem(ActorProducerConsumerTestParams p)
        {
            _consumers = new ConsumerActor[p.ConsumerActorCnt];
            for (var i = 0; i < p.ConsumerActorCnt; i++)
            {
                _consumers[i] = new ConsumerActor();
            }

            _producers = new ProducerActor[p.ProducerActorCnt];
            for (var i = 0; i < p.ProducerActorCnt; i++)
            {
                _producers[i] = new ProducerActor(i + 1, _consumers);
            }
        }

        private async Task<long> SendMessages(ActorProducerConsumerTestParams p)
        {
            var msgCnt = p.MessageCnt;
            await Task.WhenAll(_producers.Select(p => p.SendMessages(msgCnt)));
            var results = _consumers.Select(c => c.GetResult());
            await Task.WhenAll(results);
            return results.Sum(r => r.Result);
        }


        public class ProducerActor
        {
            private readonly IActorRef _impl;

            public ProducerActor(int value, ConsumerActor[] consumers)
            {
                _impl = ActorSystem.SpawnLocal<Impl>(null, new Impl.InitParams(value, consumers));
            }

            public Task SendMessages(int msgCnt) => _impl.Ask(new Impl.SendMsgCmd(msgCnt));

            private class Impl : Actor
            {
                public record InitParams(int Value, ConsumerActor[] Consumers);

                public record SendMsgCmd(int MsgCnt);


                private int _value;
                private ConsumerActor[] _consumers;


                public Impl()
                {
                    Receive<SendMsgCmd>(SendMessages);
                }


                protected override void ActorInit(object initMsg)
                {
                    var p = (InitParams)initMsg;
                    _value = p.Value;
                    _consumers = p.Consumers;
                }


                private async Task SendMessages(SendMsgCmd cmd)
                {
                    var msgCnt = cmd.MsgCnt;
                    var val = new ConsumerActor.AddValueCmd(_value);
                    var consumerCnt = _consumers.Length;
                    if (consumerCnt == 1)
                    {
                        var consumer = _consumers[0];
                        for (var i = 0; i < msgCnt; i++)
                        {
                            await consumer.AddValue(val);
                        }
                    }
                    else
                    {
                        var consumers = _consumers;
                        var consumerTasks = new Task[consumerCnt];
                        for (var i = 0; i < msgCnt; i++)
                        {
                            for (var j = 0; j < consumerCnt; j++)
                            {
                                consumerTasks[j] = consumers[j].AddValue(val);
                            }
                            await Task.WhenAll(consumerTasks);
                        }
                    }
                }
            }
        }

        public class ConsumerActor
        {
            public record AddValueCmd(int Value);

            private readonly IActorRef _impl = ActorSystem.SpawnLocal<Impl>();


            public Task AddValue(AddValueCmd cmd) => _impl.Ask(cmd);

            public Task<long> GetResult() => _impl.Ask<long>(new Impl.GetResultCmd());

            public Task ResetResult() => _impl.Ask(new Impl.ResetResultCmd());


            private class Impl : Actor
            {
                public record GetResultCmd();

                public record ResetResultCmd();


                private long _res;


                public Impl()
                {
                    Receive<AddValueCmd>(AddValue);
                    Receive<GetResultCmd, long>(GetResult);
                    Receive<ResetResultCmd>(ResetResult);
                }


                private void AddValue(AddValueCmd cmd)
                {
                    _res += cmd.Value;
                }

                private long GetResult(GetResultCmd cmd)
                {
                    return _res;
                }

                private void ResetResult(ResetResultCmd cmd)
                {
                    _res = 0;
                }

            }
        }
    }
}
