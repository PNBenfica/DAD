using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetMaster : MarshalByRefObject, IPuppetMasterURL
    {
        List<String> puppetMastersUrl;
        String url;
        String routingPolicy;
        String ordering;
        String loggingLevel;

        //key: processName value: processProxy
        private Dictionary<String, IBroker> brokers = new Dictionary<String, IBroker>();
        private Dictionary<String, ISubscriber> subscribers = new Dictionary<String, ISubscriber>();
        private Dictionary<String, IPublisher> publishers = new Dictionary<String, IPublisher>();

        public PuppetMaster(String url, String routingPolicy, String ordering, String loggingLevel)
        {
            this.url = url;
            this.routingPolicy = routingPolicy;
            this.ordering = ordering;
            this.loggingLevel = loggingLevel;
        }

        public void notify(String processName, String message)
        {
            Console.WriteLine(processName + " send: " + message);
        }

        public void createProcess(String type, String processName, String url, String brokerUrl)
        {
            ProcessCreator processCreator = new ProcessCreator();
            if (type.Equals("broker"))
            {
                processCreator.startBrokerProcess(processName, url, brokerUrl, this.routingPolicy, this.loggingLevel);
            }
            else if (type.Equals("publisher"))
            {
                processCreator.startPublisherProcess(processName, url, brokerUrl, this.loggingLevel);
            }
            else if (type.Equals("subscriber"))
            {
                processCreator.startSubscriberProcess(processName, url, brokerUrl, this.ordering, this.loggingLevel);
            }
            else
            {
                throw new UnknownProcessException("Unknown Process specified, aborting execution");
            }
        }



        public void AddBroker(String processName, string url)
        {
            IBroker broker = (IBroker)Activator.GetObject(typeof(IBroker), url);
            brokers.Add(processName, broker);
        }

        public void AddPublisher(String processName, string url)
        {
            IPublisher publisher = (IPublisher)Activator.GetObject(typeof(IPublisher), url);
            publishers.Add(processName, publisher);
        }

        public void AddSubscriber(String processName, string url)
        {
            ISubscriber subscriber = (ISubscriber)Activator.GetObject(typeof(IBroker), url);
            subscribers.Add(processName, subscriber);
        }

        public void Subscribe(string processName, string topic)
        {
            subscribers[processName].Subscribe(topic);
        }

        public void Unsubscribe(string processName, string topic)
        {
            subscribers[processName].UnSubscribe(topic);
        }

        public void Publish(string processName, string numberOfEvents, string topic, string waitXms)
        {
            //missing function at interface
        }

        public void Status()
        {
            //missing function at interface
        }

        public void Crash(string processName)
        {
            //missing function at interface
        }

        public void Freeze(string processName)
        {
            //missing function at interface
        }

        public void Unfreeze(string processName)
        {
            //missing function at interface
        }
    }
}
