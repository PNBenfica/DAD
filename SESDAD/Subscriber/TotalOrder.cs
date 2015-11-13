using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Broker
{
    class TotalOrder : OrderStrategy
    {
        public TotalOrder(Broker broker) 
            : base(broker) 
        { 
        }

        public override void DeliverInOrder(Event e)
        {
            throw new NotImplementedException();
        }
    }
}
