using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Broker
{
    class FilteredRouter : Router
    {

        public Topic<String> BrokersSubscriptions { get; set; }

        public FilteredRouter(Broker broker) : base(broker)
        {
            this.BrokersSubscriptions = new Topic<String>("/");
        }

        /// <summary>
        /// Updates Topic manager and propagtes subscrition up the tree
        /// </summary>
        public override DateTime addSubscrition(string name, bool isSubscriber, string topic)
        {
            if (isSubscriber)
                SubscribersSubscriptions.Subscribe(name, tokenize(topic));
            else
                BrokersSubscriptions.Subscribe(name, tokenize(topic));

            if (Broker.IsRoot())
            {
                return DateTime.Now;
            }
            else
            {
                return Broker.Parent.Subscribe(Broker.Name, false, topic);
            }
        }


        /// <summary>
        /// Updates Topic manager and propagtes subscrition up the tree if needed
        /// </summary>
        public override void deleteSubscrition(string name, bool isSubscriber, string topic)
        {
            if (isSubscriber)
                SubscribersSubscriptions.UnSubscribe(name, tokenize(topic));
            else
                BrokersSubscriptions.UnSubscribe(name, tokenize(topic));

            bool parentNeedUpdate = !Broker.IsRoot() && !SubscribersSubscriptions.HaveSubscribers(tokenize(topic)) && !BrokersSubscriptions.HaveSubscribers(tokenize(topic));
            if (parentNeedUpdate)
            {
                Broker.Parent.UnSubscribe(Broker.Name, false, topic);
            }
        }


        /// <summary>
        /// Returns the list of interested brokers in some event
        /// </summary>
        public override List<String> GetBrokers(Event e)
        {
            lock (SubscribersSubscriptions)
            {
                return BrokersSubscriptions.GetSubscribers(tokenize(e.Topic));
            }
        }

        public override void BrokersSubscriptionsStatus()
        {
            BrokersSubscriptions.Status();
        }
    }
}
