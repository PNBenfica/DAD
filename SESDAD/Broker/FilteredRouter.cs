using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Threading;

namespace Broker
{
    public class FilteredRouter : Router
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

            if (Broker.isPrimaryBroker)
            {
                notifyChildrenOfSubscription(name, topic, true);

                if (Broker.IsRoot())
                {
                    return DateTime.Now;
                }
                else
                {
                    return SendToParent(topic, true);
                }
            }
            return DateTime.Now;
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

            if (Broker.isPrimaryBroker)
            {
                bool parentNeedUpdate = !Broker.IsRoot() && !SubscribersSubscriptions.HaveSubscribers(tokenize(topic)) && !BrokersSubscriptions.HaveSubscribers(tokenize(topic));
                if (parentNeedUpdate)
                {
                    SendToParent(topic, false);
                }
            }
        }


        private DateTime SendToParent(string topic, bool isSubscription)
        {
            bool sent = false;
            DateTime timeStamp = DateTime.Now;
            while (!sent)
            {
                try
                {
                    if (isSubscription)
                        timeStamp = Broker.ParentPrimaryBroker().Subscribe(Broker.SiteName, false, topic, true);
                    else
                        Broker.ParentPrimaryBroker().UnSubscribe(Broker.SiteName, false, topic);
                    sent = true;
                }
                catch (System.Net.Sockets.SocketException) // primary broker is down. lets ask to see if there is a new one
                {
                    Broker.ParentBrokers.ConnectPrimaryBroker();
                }
            }
            return timeStamp;
        }


        /// <summary>
        /// Returns the list of interested brokers in some event
        /// </summary>
        public override List<String> GetBrokersSites(Event e)
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
                this.ParentSubscriptions.Subscribe(Broker.SiteName, tokenize(topic));
            }
            foreach (KeyValuePair<string, SiteBrokers> site in Broker.ChildrenSites)
            {
                if (!site.Key.Equals(name))
                {
                    site.Value.PrimaryBroker.notifyChildrenOfSubscription(site.Key, topic);
                }
            }
        }

        public override bool IsParentInterested(String topic)
        {
            return this.ParentSubscriptions.HasSubscrition(Broker.SiteName, tokenize(topic));
        }
    }
}
