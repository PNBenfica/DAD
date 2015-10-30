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
        public Broker Broker { get; set; }
        public Topic TopicManager { get; set; }

        public Router(Broker broker)
        {
            this.Broker = broker;
            this.TopicManager = new Topic("/");
        }


        /// <summary>
        /// Get the proper interested in the event and sends them the event
        /// </summary>
        public void route(Event e)
        {
             
            foreach (String s in GetSubscribers(e))
            {
                Broker.Subscribers[s].ReceiveMessage(e);
            }

            foreach (String broker in GetBrokers(e))
            {
  
                Broker.Children[broker].DiffuseMessage(e);
            }
        }

        public List<String> GetSubscribers(Event e)
        {
            return TopicManager.GetSubscribers(tokenize(e.Topic));
        }

        public abstract List<String> GetBrokers(Event e);
        public abstract DateTime addSubscrition(String name, bool isSubscriber, String topic);
        public abstract void deleteSubscrition(String name, bool isSubscriber, String topic);



        /// <summary>
        /// Divides the string topic into String[]
        /// </summary>
        public String[] tokenize(String topic)
        {
            char[] delimiterChars = { '/' };
            string[] topicSplit = topic.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            return topicSplit;
        }
    }
}
