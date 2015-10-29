using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Reflection;


namespace Subscriber
{
    public class Subscriber : MarshalByRefObject, ISubscriber
    {
        private String name;
        private String url;
        private IBroker broker;
        public OrderStrategy OrderStrategy { get; set; } // Guarantees that the message is delivered in the correct order
        public Topic Subscriptions { get; set; }


        /// <summary>
        /// Subscriber Construtor
        /// </summary>
        /// <param name="name">subscriber name</param>
        public Subscriber(String name, String url, String order)
        {
            this.name = name;
            this.url = url;
            this.OrderStrategy = GetOrderByRefletion(order);
            this.Subscriptions = new Topic("/");
        }


        private OrderStrategy GetOrderByRefletion(string order)
        {
            order = "Subscriber." + Char.ToUpper(order[0]) + order.Substring(1).ToLower() + "Order";
            Assembly assembly = Assembly.Load("Subscriber");
            Type t = assembly.GetType(order);
            return (OrderStrategy)Activator.CreateInstance(t, new Object[] { this });
        }


        public void Subscribe(string topic)
        {
            Console.WriteLine("New Subscrition on Topic: {0}", topic);
            Subscriptions.Subscribe(name, tokenize(topic), true);
            broker.Subscribe(this.name, true, topic);
        }

        public void UnSubscribe(string topic)
        {
            Console.WriteLine("Unsubscrition on Topic: {0}", topic);
            Subscriptions.Unsubscribe(name, tokenize(topic), true);
            broker.UnSubscribe(this.name, true, topic);
        }


        /// <summary>
        /// This method is called when a new event arrives from the broker
        /// </summary>
        /// <param name="e"></param>
        public void ReceiveMessage(Event e)
        {
            lock (this)
            {
                OrderStrategy.DeliverMessage(e);
            }
        }


        public void PrintMessage(Event e)
        {
            Console.WriteLine("");
            Console.WriteLine("------- New Message -------");
            Console.WriteLine("Post ID:" + e.Id);
            Console.WriteLine("Publisher: {0}\r\nTopic: {1}\r\nContent: {2}", e.PublisherId, e.Topic, e.Content);
        }


        /// <summary>
        /// returns true if the subscriber has a subscrition in the topic
        /// </summary>
        public bool HasSubscrition(string topic)
        {
            return Subscriptions.HasSubscrition(name, tokenize(topic));
        }


        /// <summary>
        /// notify site broker to add this new subscriber
        /// </summary>
        internal void registerInBroker(string brokerUrl)
        {
            Console.WriteLine("Registing in broker at {0}", brokerUrl);
            this.broker = (IBroker)Activator.GetObject(typeof(IBroker), brokerUrl);
            this.broker.registerSubscriber(this.name, this.url);
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

        
        public void Status()
        {
            Console.WriteLine("\r\n<------Status------>");
            Subscriptions.Status();
            Console.WriteLine("");

        }
    }
}
