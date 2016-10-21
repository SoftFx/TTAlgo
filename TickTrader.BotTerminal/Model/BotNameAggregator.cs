using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class BotNameAggregator
    {
        private DynamicDictionary<string, BotMsgCounter> botStats = new DynamicDictionary<string, BotMsgCounter>();

        public IDynamicDictionarySource<string, BotMsgCounter> Items { get { return botStats; } }

        public void Register(BotMessage msg)
        {
            BotMsgCounter item;
            if (!botStats.TryGetValue(msg.Bot, out item))
            {
                item = new BotMsgCounter(msg.Bot);
                botStats.Add(msg.Bot, item);
            }
            item.Increment();
        }

        public void UnRegister(BotMessage msg)
        {
            BotMsgCounter item;
            if (botStats.TryGetValue(msg.Bot, out item))
            {
                item.Decrement();
                if (!item.HasMessages)
                {
                    botStats.Remove(msg.Bot);
                }
            }
        }
    }

    public class BotMsgCounter
    {
        public BotMsgCounter(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
        public int MessageCount { get; private set; }
        public bool HasMessages { get { return MessageCount > 0; } }
        public bool IsEmpty { get { return false; } }

        public void Increment()
        {
            MessageCount++;
        }

        public void Decrement()
        {
            MessageCount--;
        }
    }
}
