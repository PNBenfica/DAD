using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.Threading;

namespace Broker
{
    public abstract class Router
    {

        #region variables
        public Broker Broker { get; set; }
        public Topic<String> SubscribersSubscriptions { get; set; }

        private Dictionary<string, List<Event>> subscribersSentEvents;
        private Dictionary<string, List<Event>> brokersSentEvents;

        #endregion

        #region classUtils
        public Router(Broker broker)
        {
            this.Broker = broker;
            this.SubscribersSubscriptions = new Topic<String>("/");
            this.subscribersSentEvents = new Dictionary<string, List<Event>>();
            this.brokersSentEvents = new Dictionary<string, List<Event>>();
        }

        /// <summary>
        /// Get the proper interested in the event and sends them the event
        /// </summary>
        public void route(Event e)
        {
            foreach (String s in GetSubscribers(e))
            {
                SendToSubscriber(e, s);
            }

            foreach (String site in GetBrokersSites(e))
            {
                SendToBroker(e, site);
            }
            Broker.SendReplicasEventSent(e);
        }


        private void SendToSubscriber(Event e, String subscriber)
        {
            try
            {
                Broker.Subscribers[subscriber].ReceiveMessage(e);
                RecordSentInfo(e, subscriber, true);
            }
            catch (System.Net.Sockets.SocketException)
            {
                Broker.Subscribers.Remove(subscriber);
                // TODO - remove all subscriptions of this subscriber
            }
        }

        private void SendToBroker(Event e, String site)
        {
            bool sent = false;
            while (!sent)
            {
                try
                {
                    Broker.ChildrenSites[site].PrimaryBroker.DiffuseMessage(e);
                    RecordSentInfo(e, site, false);
                    sent = true;
                }
                catch (System.Net.Sockets.SocketException) // primary broker is down. lets ask to see if there is a new one
                {
                    Broker.ChildrenSites[site].ConnectPrimaryBroker();
                }
            }
        }

        public void RecordSentInfo(Event e, string receiverName, bool isSubscriber)
        {
            if (isSubscriber)
            {
                if (!subscribersSentEvents.ContainsKey(receiverName))
                    subscribersSentEvents[receiverName] = new List<Event>();
                subscribersSentEvents[receiverName].Add(e);
            }
            else
            {
                if (!brokersSentEvents.ContainsKey(receiverName))
                    brokersSentEvents[receiverName] = new List<Event>();
                brokersSentEvents[receiverName].Add(e);
            }
        }


        public void ResendEvents(Event e, string siteName)
        {
            lock (this)
            {
                Console.WriteLine(brokersSentEvents[siteName].Count);
                foreach (Event ev in brokersSentEvents[siteName])
                {
                    SendToBroker(ev, siteName);
                }
            }
        }


        /// <summary>
        /// Divides the string topic into String[]
        /// </summary>
        public String[] tokenize(String topic)
        {
            char[] delimiterChars = { '/' };
            string[] topicSplit = topic.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            return topicSplit;
        }



        public List<String> GetSubscribers(Event e)
        {
            lock (SubscribersSubscriptions)
            {
                return SubscribersSubscriptions.GetSubscribers(tokenize(e.Topic));
            }
        }

        public void Status()
        {
            Console.WriteLine("--Subscribers Subscriptions--");
            SubscribersSubscriptions.Status();
            Console.WriteLine("--Brokers Subscriptions--");
            BrokersSubscriptionsStatus();
        }

        #endregion

        #region abstractMethods

        public abstract List<String> GetBrokersSites(Event e);
        public abstract DateTime addSubscrition(String name, bool isSubscriber, String topic, bool isClimbing);
        public abstract void deleteSubscrition(String name, bool isSubscriber, String topic);
        public abstract bool HasSubscrition(String topic);
        public abstract void BrokersSubscriptionsStatus();
        public abstract void notifyChildrenOfSubscription(string name, string topic, bool isClimbing = false);
        public abstract bool IsParentInterested(string topic);
        #endregion
    }
}
