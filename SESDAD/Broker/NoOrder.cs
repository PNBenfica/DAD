using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Broker
{
    class NoOrder: OrderStrategy
    {
        public NoOrder(Broker broker) 
            : base(broker)
        {
        }

        public override void DeliverInOrder(Event e)
        {
            Broker.Log(e);
            Broker.Router.route(e);
        }
    }
}
