﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Broker
{
    class FloodingRouter : Router
    {

        public FloodingRouter(Broker broker) : base(broker)
        {
        }


        /// <summary>
        /// param isSubscriber is always True. Doesn t update parent broker, only subscribers.
        /// </summary>
        public override void addSubscrition(string name, bool isSubscriber, string topic, bool isClimbing)
        {
            SubscribersSubscriptions.Subscribe(name, tokenize(topic));
        }

        /// <summary>
        /// param isSubscriber is always True. Doesn t update parent, only subscribers.
        /// </summary>
        public override void deleteSubscrition(string name, bool isSubscriber, string topic)
        {
            SubscribersSubscriptions.UnSubscribe(name, tokenize(topic));
        }


        /// <summary>
        /// Returns the list of interested brokers in some event
        /// </summary>
        public override List<String> GetBrokersSites(Event e)
        {
            String[] brokers = Broker.ChildrenSites.Keys.ToArray();
            return new List<string>(brokers);
        }


        public override void BrokersSubscriptionsStatus()
        {
            Console.WriteLine("Flooding to all Brokers");
        }

        /// <summary>
        /// Always true. The current broker is expecting every events
        /// </summary>
        public override bool HasSubscrition(String topic)
        {
            return true;
        }

        public override void notifyChildrenOfSubscription(string name, string topic, bool isClimbing = false)
        {
            return;
        }

        public override bool IsParentInterested(string topic)
        {
            return true;
        }
    }
}
