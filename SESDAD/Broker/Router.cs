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
        private Dictionary<string, Object> brokerLocks = new Dictionary<string, Object>();
        private Dictionary<string, Object> subscribersLocks = new Dictionary<string, Object>();

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
        /// Every event is sent in parallel so that in case of crash of a child the other childs dont stop receiving events
        /// After sendind to all the childs broker must notify replicas
        /// </summary>
        public void route(Event e)
        {
            lock (this)
            {
                Thread thread1 = new Thread(() => {
                    List<Thread> threads = new List<Thread>();
                    foreach (String s in GetSubscribers(e))
                    {
                        Thread thread = new Thread(() => { SendToSubscriber(e, s); });
                        thread.Start();
                        threads.Add(thread);
                    }

                    foreach (String site in GetBrokersSites(e))
                    {
                        Thread thread = new Thread(() => { SendToBroker(e, site); });
                        thread.Start();
                        threads.Add(thread);
                    }
                    foreach (Thread thread in threads)
                    {
                        thread.Join();
                    }
                    Broker.SendToReplicas("SentEventNotification", e);
                });
                thread1.Start();
            }
        }


        /// <summary>
        /// send a event to a subscriber, and then records the info and sends it to the replicas
        /// </summary>
        private void SendToSubscriber(Event e, String subscriber)
        {
            if (!subscribersLocks.ContainsKey(subscriber))
                subscribersLocks[subscriber] = new Object();
            lock (subscribersLocks[subscriber])
            {
                try
                {
                    Broker.Subscribers[subscriber].ReceiveMessage(e);
                    RecordSentInfo(e, subscriber, true);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    Broker.Subscribers.Remove(subscriber);
                    // TODO remove all subscriptions of this subscriber
                }
            }
        }


        /// <summary>
        /// send a event to a broker, and then records the info and sends it to the replicas
        /// </summary>
        private void SendToBroker(Event e, String site)
        {
            if (!brokerLocks.ContainsKey(site))
                brokerLocks[site] = new Object();
            lock (brokerLocks[site])
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
        }


        /// <summary>
        /// method called when a event e was sucessful send to receiverName. records that info
        /// </summary>
        public void RecordSentInfo(Event e, string receiverName, bool isSubscriber)
        {
            if (Broker.isPrimaryBroker)
                Broker.SendToReplicas("UpdateSentEvents", e, receiverName, isSubscriber);
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


        /// <summary>
        /// return true if has send this event to the site
        /// </summary>
        public bool HasSentEvent(Event e, string siteName)
        {
            if (brokersSentEvents.ContainsKey(siteName))
                return brokersSentEvents[siteName].Contains(e);
            return false;
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
        public abstract void addSubscrition(String name, bool isSubscriber, String topic, bool isClimbing);
        public abstract void deleteSubscrition(String name, bool isSubscriber, String topic);
        public abstract bool HasSubscrition(String topic);
        public abstract void BrokersSubscriptionsStatus();
        public abstract void notifyChildrenOfSubscription(string name, string topic, bool isClimbing = false);
        public abstract bool IsParentInterested(string topic);
        #endregion
    }
}
