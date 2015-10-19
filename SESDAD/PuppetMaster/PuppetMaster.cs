﻿using CommonTypes;
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
            publishers[processName].SequencePublish(numberOfEvents, topic, waitXms);
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
            //missing function at interface
        }

        public void Freeze(String processName)
        {
            //missing function at interface
        }

        public void Unfreeze(String processName)
        {
            //missing function at interface
        }
    }
}
