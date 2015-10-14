using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Broker
{
    public class Broker : IBroker
    {
        private Broker parent;
        private List<Broker> children;
        private String url;
        private List<Event> messages;
        private SubscriptionManager subscriptionManager;
        private List<IPublisher> publishers;
        private Router router;
        

        public Broker()
        {

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
            return true;
        }

    }
}
