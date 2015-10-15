using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace Broker
{
    public class SubscriptionManager
    {
        List<ISubscriber> subscribers;

        public SubscriptionManager()
        {

        }

        public void Subscribe(String subscriberId, Topic topic)
        {

        }

        public void SubscribeClient(String subscriberId, Topic topic)
        {

        }

        internal void SubscribeRouting(string Id, Topic topic)
        {

        }

        public void UnSubscribe(String subscriberId, Topic topic)
        {

        }

        public List<ISubscriber> getSubscriptors(Topic topic)
        {
            //still TODO
            return subscribers;
        }


      
    }
}
