using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace Broker
{
    public abstract class OrderStrategy
    {
        public Broker Broker { get; set; }

        public OrderStrategy(Broker broker)
        {
            this.Broker = broker;
        }

        public abstract void DeliverInOrder(Event e);

    }
}
