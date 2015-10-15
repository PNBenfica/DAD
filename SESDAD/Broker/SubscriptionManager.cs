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

        public void Subscribe(String subscriberId, String topic)
        {

        }

        public void SubscribeClient(String subscriberId, String topic)
        {

        }

        internal void SubscribeRouting(string Id, String topic)
        {

        }

        public void UnSubscribe(String subscriberId, String topic)
        {

        }

        public List<ISubscriber> getSubscriptors(String topic)
        {
            //still TODO
            return subscribers;
        }




        internal void UnSubscribeClient(string Id, String topic)
        {
        }

        internal void UnSubscribeRouter(string Id, String topic)
        {
        }

        internal List<Router> getRouters(String topic)
        {
            //still TODO

            return null;
        }
    }
}
