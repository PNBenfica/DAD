using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace Subscriber
{
    public abstract class OrderStrategy
    {
        public Subscriber Subscriber { get; set; }

        public OrderStrategy(Subscriber subscriber)
        {
            this.Subscriber = subscriber;
        }

        public abstract void DeliverMessage(Event e);

    }
}
