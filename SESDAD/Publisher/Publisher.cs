using System;
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

        #region variables

        private List<Event> events;
        public String Name { get; set; }
        private String url;
        private IBroker broker;
        public Queue<Event> PreviousEvents { get; set; }
        public LinkedList<Event> frozenEvents { get; set; }
        public int NumberOfEvents { get; set; }
        private const int MAXEVENTSQUEUE = 10;
        private bool freeze = false;

        #endregion variables

        #region classUtils
        
        public Publisher(String name, String url)
        {
            this.Name = name;
            this.url = url;
            events = new List<Event>();
            this.PreviousEvents = new Queue<Event>(MAXEVENTSQUEUE);
            this.frozenEvents = new LinkedList<Event>();
            this.NumberOfEvents = 0;
        }


        public void Freeze()
        {
            this.freeze = true;
        }

        public void Unfreeze() 
        {
            this.freeze = false;
            int frozensize = frozenEvents.Count;
            for (int i = 0; i < frozensize; i++ )
            {
                Event ev = frozenEvents.First<Event>();
                frozenEvents.RemoveFirst();
             
                DateTime timeStamp = broker.DiffuseMessageToRoot(ev);
                ev.TimeStamp = timeStamp;
              
            }
        }

        public void Crash()
        {
            System.Environment.Exit(0);
        }

        public void Status()
        {
            Console.WriteLine("\r\n<------Status------>");
            Console.WriteLine("Freeze: " + freeze);
            PrintQueuedEvents();
            Console.WriteLine("");
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

        #endregion

        #region publishEvent

        public void Publish(String topic, String content)
        {
            Event ev;
            lock (this)
            {
                ev = ProduceEvent(topic, content);
                if (!this.freeze)
                {
                    DateTime timeStamp = broker.DiffuseMessageToRoot(ev);
                    ev.TimeStamp = timeStamp;
                    UpdatePreviousEvents(ev);
                }
                else
                {
                    frozenEvents.AddLast(ev);
                    UpdatePreviousEvents(ev);
                }
            }            
        }

        private Event ProduceEvent(string topic, string content)
        {
            Console.WriteLine("New event published\r\nID: {0}\r\nTopic {1}\r\ncontent: {2}\r\n", NumberOfEvents + 1, topic, content);
            return new Event(++NumberOfEvents, Name, topic, content, new List<Event>(PreviousEvents.ToArray()));
        }

        private void UpdatePreviousEvents(Event e)
        {
            if (PreviousEvents.Count == 10)
            {
                PreviousEvents.Dequeue();
            }
            PreviousEvents.Enqueue(new Event(e.Id, e.TimeStamp, e.PublisherId, e.Topic));
        }

        public void SequencePublish(String numberOfEvents, String topic, String waitXms)
        {
            int eventNumber = Convert.ToInt32(numberOfEvents);
            int waitingTime = Convert.ToInt32(waitXms);
            for (int i = 0; i < eventNumber; i++)
            {
                //Thread thread = new Thread(() =>
                //{
                    Publish(topic, this.Name + i);
                //});
                //thread.Start();
                Thread.Sleep(waitingTime);
            }

        }

        public void registerInBroker(String brokerUrl)
        {
            Console.WriteLine("Registing in broker at {0}", brokerUrl);
            this.broker = (IBroker)Activator.GetObject(typeof(IBroker), brokerUrl);
            this.broker.registerPublisher(this.url);
        }

        #endregion
    
    }
}
