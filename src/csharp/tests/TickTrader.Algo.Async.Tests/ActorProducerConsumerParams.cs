using System;

namespace TickTrader.Algo.Async.Tests
{
    public record ActorProducerConsumerTestParams(int ProducerActorCnt, int ConsumerActorCnt, int MessageCnt)
    {
        public long ExpectedResult => 1L * MessageCnt * ConsumerActorCnt * (ProducerActorCnt + 1) * ProducerActorCnt / 2;

        public override string ToString()
        {
            return $"({ProducerActorCnt}, {ConsumerActorCnt}, {MessageCnt})";
        }
    }

    class ActorProducerConsumerParams
    {
        public static ActorProducerConsumerTestParams[] TestCases { get; } = new[]
            {
                new ActorProducerConsumerTestParams(1, 1, (int)1e5),
                new ActorProducerConsumerTestParams(1, (int)1e4, 1),
                new ActorProducerConsumerTestParams((int)1e4, 1, 1),
                new ActorProducerConsumerTestParams(100, (int)1e4, 1),
                new ActorProducerConsumerTestParams((int)1e4, 100, 1),

                new ActorProducerConsumerTestParams(Environment.ProcessorCount / 2, Environment.ProcessorCount / 2, (int)1e4),
                new ActorProducerConsumerTestParams(Environment.ProcessorCount / 4, Environment.ProcessorCount / 4 * 3, (int)1e4),
                new ActorProducerConsumerTestParams(Environment.ProcessorCount / 4 * 3, Environment.ProcessorCount / 4, (int)1e4),
                new ActorProducerConsumerTestParams(Environment.ProcessorCount, Environment.ProcessorCount / 2, (int)1e4),
                new ActorProducerConsumerTestParams(Environment.ProcessorCount / 2, Environment.ProcessorCount, (int)1e4),
            };
    }
}
