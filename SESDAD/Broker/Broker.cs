using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Broker
{
    public class Broker : MarshalByRefObject, IBroker
    {
        private String name;
        private String url;
        private IBroker parent;
        private List<IBroker> children = new List<IBroker>();
        private List<IPublisher> publishers = new List<IPublisher>();
        private List<ISubscriber> subscribers = new List<ISubscriber>();
        private List<Event> events = new List<Event>();
        private SubscriptionManager subscriptionManager;
        private Router router;
        

        public Broker(String name, String url)
        {
            this.name = name;
            this.url = url;
        }

        public void Subscribe(Topic topic, String content)
        {

        }

        public void UnSubscribe(String subscriberId, Topic topic)
        {

        }

        public void DiffuseMessage(Event e)
        {
            Console.WriteLine("Diffusing message from {0}", e.PublisherId);
            foreach (ISubscriber subscriber in subscribers)
            {
                subscriber.ReceiveMessage(e);
            }

            foreach (IBroker broker in children)
            {
                broker.DiffuseMessage(e);
            }
        }


        /// <summary>
        /// Diffuse the event to the root
        /// </summary>
        public void DiffuseMessageToRoot(Event even)
        {
            bool root = this.IsRoot();
            if(!root)
            {
                this.parent.DiffuseMessageToRoot(even);
            }

            else 
            {
                this.DiffuseMessage(even);
            }
        }

        public void ReceiveMessage(Topic topic, String content)
        {

        }

        /// <summary>
        /// If the broker has no parent he is the root
        /// </summary>
        public bool IsRoot()
        {
            return parent == null;
        }

        /// <summary>
        /// Notify the broker parent that he has a new born child
        /// </summary>
        /// <param name="parentUrl">Broker parent url</param>
        internal void notifyParent(string parentUrl)
        {
            Console.WriteLine("Registing in parent at {0}", parentUrl);
            this.parent = (IBroker)Activator.GetObject(typeof(IBroker), parentUrl);
            parent.registerNewChild(this.url);
        }

        /// <summary>
        /// Register a new child
        /// </summary>
        /// <param name="url">Url of the new broker child</param>
        public void registerNewChild(string url)
        {
            IBroker child = (IBroker)Activator.GetObject(typeof(IBroker), url);
            children.Add(child);
            Console.WriteLine("New child broker registed: {0}", url);
        }

        public void registerPublisher(string url)
        {
            IPublisher publisher = (IPublisher)Activator.GetObject(typeof(IPublisher), url);
            publishers.Add(publisher);
            Console.WriteLine("New publisher registed: {0}", url);
        }

        /// <summary>
        /// Register a new subscriber
        /// </summary>
        /// <param name="url">Url of the new subscriber</param>
        public void registerSubscriber(string url)
        {
            ISubscriber subscriber = (ISubscriber)Activator.GetObject(typeof(ISubscriber), url);
            subscribers.Add(subscriber);
            Console.WriteLine("New subscriber registed: {0}", url);
        }
    }
}
