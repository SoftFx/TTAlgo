using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Toaster.Messages
{
    public class InfoMessage: BaseMessage
    {
        public InfoMessage()
        {

        }

        public InfoMessage(string title, string body):base(title, body)
        {

        }
    }
}
