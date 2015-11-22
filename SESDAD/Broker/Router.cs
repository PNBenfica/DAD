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

        #endregion

        #region classUtils
        public Router(Broker broker)
        {
            this.Broker = broker;
            this.SubscribersSubscriptions = new Topic<String>("/");
        }

        /// <summary>
        /// Get the proper interested in the event and sends them the event
        /// </summary>
        public void route(Event e)
        {

            foreach (String s in GetSubscribers(e))
            {
                Console.WriteLine(s);
                Broker.Subscribers[s].ReceiveMessage(e);
            }

            foreach (String site in GetBrokersSites(e))
            {
                SendToBroker(e, site);    
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
                    sent = true;
                }
                catch (System.Net.Sockets.SocketException) // primary broker is down. lets ask to see if there is a new one
                {
                    Broker.ChildrenSites[site].ConnectPrimaryBroker();
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
