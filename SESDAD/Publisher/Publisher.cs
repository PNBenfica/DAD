﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Threading;

namespace Publisher
{
    class Publisher : MarshalByRefObject, IPublisher
    {
        private List<Event> events;
        public String Name { get; set; }
        private String url;
        private IBroker broker;
        public Queue<Event> PreviousEvents { get; set; }
        public int NumberOfEvents { get; set; }
        private const int MAXEVENTSQUEUE = 10;

        public Publisher(String name, String url)
        {
            this.Name = name;
            this.url = url;
            events = new List<Event>();
            this.PreviousEvents = new Queue<Event>(MAXEVENTSQUEUE);
            this.NumberOfEvents = 0;
        }


        public void Publish(String topic, String content)
        {
            Console.WriteLine("New event published\r\nID: {0}\r\nTopic {1}\r\n",NumberOfEvents + 1,  topic);
            Event ev;
            lock (this)
            {
                ev = ProduceEvent(topic, content);
                UpdatePreviousEvents(ev);
            }
            PrintQueuedEvents();
            broker.DiffuseMessageToRoot(ev);
        }


        private Event ProduceEvent(string topic, string content)
        {
            return new Event(++NumberOfEvents, Name, topic, content, new List<Event>(PreviousEvents.ToArray()));
        }


        private void UpdatePreviousEvents(Event e)
        {
            if (PreviousEvents.Count == 10)
            {
                PreviousEvents.Dequeue();
            }
            PreviousEvents.Enqueue(new Event(e.Id, e.PublisherId, e.Topic));
        }


        public void SequencePublish(String numberOfEvents, String topic, String waitXms)
        {
            int eventNumber = Convert.ToInt32(numberOfEvents);
            int waitingTime = Convert.ToInt32(waitXms);
            for (int i = 0; i < eventNumber; i++)
            {
                Publish(topic, this.Name + i);
                Thread.Sleep(waitingTime);
            }

        }


        public void registerInBroker(String brokerUrl)
        {
            Console.WriteLine("Registing in broker at {0}", brokerUrl);
            this.broker = (IBroker)Activator.GetObject(typeof(IBroker), brokerUrl);
            this.broker.registerPublisher(this.url);
        }

        public void Status()
        {
            //TODO
        }

        private void PrintQueuedEvents()
        {
            Console.WriteLine("--- Queued Events Id's ---");
            foreach (Event ev in PreviousEvents)
            {
                Console.Write("{0} ", ev.Id);
            }
            Console.WriteLine("");
        }
    }
}
