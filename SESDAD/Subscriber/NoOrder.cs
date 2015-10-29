using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Subscriber
{
    class NoOrder: OrderStrategy
    {
        public NoOrder(Subscriber subscriber) 
            : base(subscriber)
        {
        }

        public override void DeliverMessage(Event e)
        {
            Subscriber.PrintMessage(e);
        }
    }
}
