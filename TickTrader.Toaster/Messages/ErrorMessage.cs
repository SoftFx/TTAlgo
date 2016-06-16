using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Toaster.Messages
{
    public class ErrorMessage: BaseMessage
    {
        public ErrorMessage()
        {

        }

        public ErrorMessage(string title, string body):base(title, body)
        {

        }
    }
}
