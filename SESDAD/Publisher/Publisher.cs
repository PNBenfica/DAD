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

        public String Name { get; set; }
        private String url;
        private IBroker broker;
        public Queue<Event> PreviousEvents { get; set; }
        public int NumberOfEvents { get; set; }
        private const int MAXEVENTSQUEUE = 10;

        private bool isFrozen = false;
        Object freezeLock = new Object();
        AutoResetEvent notFreezed = new AutoResetEvent(true);
        private IPuppetMasterURL puppetMaster;
        private string loggingLevel;

        #endregion variables

        #region classUtils
        
        public Publisher(string name, string url, string puppetMasterUrl, string loggingLevel)
        {
            this.Name = name;
            this.url = url;
            this.puppetMaster = (IPuppetMasterURL)Activator.GetObject(typeof(IPuppetMasterURL), puppetMasterUrl);
            this.loggingLevel = loggingLevel;
            this.PreviousEvents = new Queue<Event>(MAXEVENTSQUEUE);
            this.NumberOfEvents = 0;
        }


        public void Freeze()
        {
            Console.WriteLine("Freezed");
            lock (freezeLock)
            {
                isFrozen = true;
                notFreezed.Reset();
            }
        }

        public void Unfreeze()
        {
            Console.WriteLine("UnFreezed");
            lock (freezeLock)
            {
                isFrozen = false;
                notFreezed.Set();
            }
        }


        private void CheckFroozen()
        {
            AutoResetEvent[] handles = { notFreezed };
            WaitHandle.WaitAll(handles);
            lock (freezeLock)
            {
                if (!isFrozen)
                {
                    notFreezed.Set();
                }
            }
        }

        public void Crash()
        {
            System.Environment.Exit(0);
        }

        public void Status()
        {
            Console.WriteLine("\r\n<------Status------>");
            Console.WriteLine("Freeze: " + isFrozen);
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
            lock (this)
            {
                Event ev = ProduceEvent(topic, content);
                DateTime timeStamp = broker.DiffuseMessageToRoot(ev);
                ev.TimeStamp = timeStamp;
                UpdatePreviousEvents(ev);
                puppetMaster.Log("PubEvent " + this.Name + ", " + this.Name + ", " + ev.Topic + ", " + this.NumberOfEvents); // faz sentido meter duas vezes o nome do processo? no enunciado esta
            }            
        }

        private Event ProduceEvent(string topic, string content)
        {
            Console.WriteLine("New event published\r\nID: {0}\r\nTopic {1}\r\ncontent: {2}\r\n", NumberOfEvents + 1, topic, content);
            return new Event(++NumberOfEvents, Name, topic, content, new List<Event>(PreviousEvents.ToArray()));
        }

        private void UpdatePreviousEvents(Event e)
        {
            if (PreviousEvents.Count == MAXEVENTSQUEUE)
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
                CheckFroozen();

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

        #endregion
    
    }
}
