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
        public Topic<String> ParentSubscriptions { get; set; }

        public FilteredRouter(Broker broker) : base(broker)
        {
            this.BrokersSubscriptions = new Topic<String>("/");
            this.ParentSubscriptions = new Topic<String>("/");
        }

        /// <summary>
        /// Updates Topic manager and propagtes subscrition up the tree
        /// </summary>
        public override DateTime addSubscrition(string name, bool isSubscriber, string topic, bool isClimbing)
        {
            if (isSubscriber)
                SubscribersSubscriptions.Subscribe(name, tokenize(topic));
            else
                BrokersSubscriptions.Subscribe(name, tokenize(topic));

            //notify every child
            notifyChildrenOfSubscription(name, topic, true);

            if (Broker.IsRoot())
            {
                return DateTime.Now;
            }
            else
            {
                return Broker.Parent.Subscribe(Broker.Name, false, topic, true);
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

        public override bool HasSubscrition(string topic)
        {
            String[] topicTokens = tokenize(topic);
            return SubscribersSubscriptions.HaveSubscribers(topicTokens) || BrokersSubscriptions.HaveSubscribers(topicTokens);
        }

        public override void notifyChildrenOfSubscription(string name, string topic, bool isClimbing)
        {
            //only add if was the parent calling
            if (!isClimbing)
            {
                this.ParentSubscriptions.Subscribe(Broker.Name, tokenize(topic));
            }
            foreach (KeyValuePair<string, IBroker> brokerChild in Broker.Children)
            {
                if (!brokerChild.Key.Equals(name))
                {
                    brokerChild.Value.notifyChildrenOfSubscription(brokerChild.Key, topic);
                }
            }
        }

        public override bool IsParentInterested(String topic)
        {
            return this.ParentSubscriptions.HasSubscrition(this.Broker.Name, tokenize(topic));
        }
    }
}
