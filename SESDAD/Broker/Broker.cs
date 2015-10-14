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
        private List<IBroker> children;
        private List<IPublisher> publishers;
        private List<ISubscriber> subscribers;
        private List<Event> messages;
        private SubscriptionManager subscriptionManager;
        private Router router;
        

        public Broker(String name, String url)
        {
            this.name = name;
            this.url = url;
            this.children = new List<IBroker>();
        }

        public void Subscribe(Topic topic, String content)
        {

        }

        public void UnSubscribe(String subscriberId, Topic topic)
        {

        }

        public void DiffuseMessage(Event even)
        {

        }

        public void DiffuseMessageToRoot(Event even)
        {

        }

        public void ReceiveMessage(Topic topic, String content)
        {

        }

        public bool IsRoot()
        {
            return parent == null;
        }


        internal void notifyParent(string parentUrl)
        {
            Console.WriteLine("Registing in parent at {0}", parentUrl);
            this.parent = (IBroker)Activator.GetObject(typeof(IBroker), parentUrl);
            parent.registerNewChild(this.url);
        }


        public void registerNewChild(string url)
        {
            Console.WriteLine(url);
            IBroker child = (IBroker)Activator.GetObject(typeof(IBroker), url);
            children.Add(child);
            Console.WriteLine("New child broker registed: {0}", url);
        }
    }
}
