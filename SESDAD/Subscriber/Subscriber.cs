using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;


namespace Subscriber
{
    class Subscriber : MarshalByRefObject, ISubscriber
    {
        private String name;
        private String url;
        private IBroker broker;
        private OrderStrategy orderStrategy;

        /// <summary>
        /// Subscriber Construtor
        /// </summary>
        /// <param name="name">subscriber name</param>
        public Subscriber(String name, String url)
        {
            this.name = name;
            this.url = url;
        }

        public void Subscribe(string topic)
        {
            bool client = true;
            broker.Subscribe(this.name, client, topic);
        }

        public void UnSubscribe(string topic)
        {
           
            bool client = true;
            broker.UnSubscribe(this.name, client, topic);
        }


        /// <summary>
        /// This method is called when a new event arrives from the broker
        /// </summary>
        /// <param name="e"></param>
        public void ReceiveMessage(Event e)
        {
            Console.WriteLine("Publisher: {0}\r\nTopic: {1}\r\nContent: {2}",e.PublisherId, e.Topic.Name, e.Content);
        }


        /// <summary>
        /// notify site broker to add this new subscriber
        /// </summary>
        /// <param name="brokerUrl">site broker</param>
        internal void registerInBroker(string brokerUrl)
        {
            Console.WriteLine("Registing in broker at {0}", brokerUrl);
            this.broker = (IBroker)Activator.GetObject(typeof(IBroker), brokerUrl);
            this.broker.registerSubscriber(this.url);
        }
    }
}
