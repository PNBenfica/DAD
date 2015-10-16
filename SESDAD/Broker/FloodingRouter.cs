using System;
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
        public override void addSubscrition(string name, bool isSubscriber, string topic)
        {
            TopicManager.Subscribe(name, tokenize(topic), isSubscriber);
        }

        /// <summary>
        /// param isSubscriber is always True. Doesn t update parent, only subscribers.
        /// </summary>
        public override void deleteSubscrition(string name, bool isSubscriber, string topic)
        {
            TopicManager.Unsubscribe(name, tokenize(topic), isSubscriber);
        }


        /// <summary>
        /// Returns the list of interested brokers in some event
        /// </summary>
        public override List<String> GetBrokers(Event e)
        {
            String[] brokers = Broker.Children.Keys.ToArray();
            return new List<string>(brokers);
        }
    }
}
