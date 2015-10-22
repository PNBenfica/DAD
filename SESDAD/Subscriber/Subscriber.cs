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
        private bool isOrdering;
        public Dictionary<string, int> PublishersPosts { get; set; }

        /// <summary>
        /// Subscriber Construtor
        /// </summary>
        /// <param name="name">subscriber name</param>
        public Subscriber(String name, String url, String ordering)
        {
            this.name = name;
            this.url = url;
            this.isOrdering = ordering.Equals("FIFO");
            this.PublishersPosts = new Dictionary<String, int>();
        }

        public void Subscribe(string topic)
        {
            Console.WriteLine("Subscribe {0}", topic);
            broker.Subscribe(this.name, true, topic);
        }

        public void UnSubscribe(string topic)
        {
            broker.UnSubscribe(this.name, true, topic);
        }


        /// <summary>
        /// This method is called when a new event arrives from the broker
        /// </summary>
        /// <param name="e"></param>
        public void ReceiveMessage(Event e)
        {
            if (isOrdering){
                IncPublisherPost(e.PublisherId);
                PrintMessage(e);
                Console.WriteLine("Publisher Posts Recorded:" + PublishersPosts[e.PublisherId]);
                Console.WriteLine("Post ID:" + e.Id);
            }
            else
                PrintMessage(e);
        }



        private void PrintMessage(Event e)
        {
            Console.WriteLine("\r\n------------\r\nPublisher: {0}\r\nTopic: {1}\r\nContent: {2}", e.PublisherId, e.Topic, e.Content);
        }


        private void IncPublisherPost(string publisher)
        {
            if (!PublishersPosts.ContainsKey(publisher))
                PublishersPosts[publisher] = 1;
            else
                PublishersPosts[publisher]++;
        }


        /// <summary>
        /// notify site broker to add this new subscriber
        /// </summary>
        /// <param name="brokerUrl">site broker</param>
        internal void registerInBroker(string brokerUrl)
        {
            Console.WriteLine("Registing in broker at {0}", brokerUrl);
            this.broker = (IBroker)Activator.GetObject(typeof(IBroker), brokerUrl);
            this.broker.registerSubscriber(this.name, this.url);
        }

        public void Status()
        {
            //TODO
        }
    }
}
