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
        public FilteredRouter(Broker broker) : base(broker)
        {
        }

        /// <summary>
        /// Updates Topic manager and propagtes subscrition up the tree
        /// </summary>
        public override DateTime addSubscrition(string name, bool isSubscriber, string topic)
        {
            TopicManager.Subscribe(name, tokenize(topic), isSubscriber);

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
            TopicManager.UnSubscribe(name, tokenize(topic), isSubscriber);
            
            bool parentNeedUpdate = !Broker.IsRoot() && !TopicManager.HaveSubscribers(tokenize(topic));
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
            lock (TopicManager)
            {
                return TopicManager.GetBrokers(tokenize(e.Topic));
            }
        }
    }
}
