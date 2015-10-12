using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace Broker
{
    public class SubscriptionManager
    {
        List<ISubscriberInterface> subscribers;

        public SubscriptionManager()
        {

        }

        public void Subscribe(String subscriberId, Topic topic)
        {

        }

        public void UnSubscribe(String subscriberId, Topic topic)
        {

        }

        public List<ISubscriberInterface> getSubscriptors(Topic topic)
        {
            //still TODO
            return subscribers;
        }

    }
}
