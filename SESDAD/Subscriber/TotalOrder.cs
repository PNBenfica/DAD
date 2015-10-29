using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Subscriber
{
    class TotalOrder : OrderStrategy
    {
        public TotalOrder(Subscriber subscriber) 
            : base(subscriber) 
        { 
        }

        public override void DeliverMessage(Event e)
        {
            throw new NotImplementedException();
        }
    }
}
