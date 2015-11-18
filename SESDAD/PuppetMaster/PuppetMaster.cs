using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetMaster : MarshalByRefObject, IPuppetMasterURL
    {
        #region variables

        List<String> puppetMastersUrl;
        String centralPuppetMasterUrl;
        String url;
        String routingPolicy;
        String ordering;
        String loggingLevel;
        String datefile;


        //key: processName value: processProxy
        private Dictionary<String, IBroker> brokers = new Dictionary<String, IBroker>();
        private Dictionary<String, ISubscriber> subscribers = new Dictionary<String, ISubscriber>();
        private Dictionary<String, IPublisher> publishers = new Dictionary<String, IPublisher>();

        #endregion

        public PuppetMaster(String url, String routingPolicy, String ordering, String loggingLevel, String centralPuppetMasterUrl)
        {
            this.url = url;
            this.routingPolicy = routingPolicy;
            this.ordering = ordering;
            this.loggingLevel = loggingLevel;
            this.centralPuppetMasterUrl = centralPuppetMasterUrl;
            this.datefile = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            Log("\n\n");
        }

        #region remoteMethods

        public void Notify(String processName, String message)
        {
            Console.WriteLine(processName + " send: " + message);
        }

        public void CreateProcess(String type, String processName, String url, String brokerUrl, String[] brokerNeighbours)
        {
            ProcessCreator processCreator = new ProcessCreator();
            if (type.Equals("broker"))
            {
                processCreator.startBrokerProcess(processName, url, brokerUrl, brokerNeighbours, this.routingPolicy, this.loggingLevel, this.centralPuppetMasterUrl);
            }
            else if (type.Equals("publisher"))
            {
                processCreator.startPublisherProcess(processName, url, brokerUrl, this.loggingLevel, this.centralPuppetMasterUrl);
            }
            else if (type.Equals("subscriber"))
            {
                processCreator.startSubscriberProcess(processName, url, brokerUrl, this.ordering, this.loggingLevel, this.centralPuppetMasterUrl);
            }
            else
            {
                throw new UnknownProcessException("Unknown Process specified, aborting execution");
            }
        }


        public void Log(string logMessage)
        {
            lock (this)
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(@"log-" + datefile + ".txt", true);
                file.WriteLine(logMessage);
                Console.WriteLine(logMessage);
                file.Close();
            }
        }


        #endregion

        #region localMethods

        public void AddBroker(String processName, String url)
        {
            IBroker broker = (IBroker)Activator.GetObject(typeof(IBroker), url);
            brokers.Add(processName, broker);
        }

        public void AddPublisher(String processName, String url)
        {
            IPublisher publisher = (IPublisher)Activator.GetObject(typeof(IPublisher), url);
            publishers.Add(processName, publisher);
        }

        public void AddSubscriber(String processName, String url)
        {
            ISubscriber subscriber = (ISubscriber)Activator.GetObject(typeof(IBroker), url);
            subscribers.Add(processName, subscriber);
        }

        public void Subscribe(String processName, String topic)
        {
            subscribers[processName].Subscribe(topic);
        }

        public void Unsubscribe(String processName, String topic)
        {
            subscribers[processName].UnSubscribe(topic);
        }

        public void Publish(String processName, String numberOfEvents, String topic, String waitXms)
        {
            Thread thread = new Thread(() =>
            {
            publishers[processName].SequencePublish(numberOfEvents, topic, waitXms);


            });
            thread.Start();
        }

        public void Status()
        {
            foreach (String name in brokers.Keys)
            {
                brokers[name].Status();
            }
            foreach (String name in publishers.Keys)
            {
                publishers[name].Status();
            }
            foreach (String name in subscribers.Keys)
            {
                subscribers[name].Status();
            }
        }

        public void Crash(String processName)
        {
            if (publishers.ContainsKey(processName))
                publishers[processName].Crash();

            if (subscribers.ContainsKey(processName))
                subscribers[processName].Crash();

            if (brokers.ContainsKey(processName))
                brokers[processName].Crash();
        }

        public void Freeze(String processName)
        {
            if (publishers.ContainsKey(processName))
                publishers[processName].Freeze();

            if (subscribers.ContainsKey(processName))
               subscribers[processName].Freeze();

            if (brokers.ContainsKey(processName))
                brokers[processName].Freeze();
        }

        public void Unfreeze(String processName)
        {
            if (publishers.ContainsKey(processName))
                publishers[processName].Unfreeze();

            if (subscribers.ContainsKey(processName))
                subscribers[processName].Unfreeze();

            if (brokers.ContainsKey(processName))
                brokers[processName].Unfreeze();
        }

        #endregion

    }
}
