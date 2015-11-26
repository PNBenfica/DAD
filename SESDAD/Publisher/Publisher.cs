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
        public Queue<Event> PreviousEvents { get; set; }
        public int NumberOfEvents { get; set; }
        private string loggingLevel;
        private SiteBrokers siteBrokers;

        private bool isFrozen = false;
        Object freezeLock = new Object();
        AutoResetEvent notFreezed = new AutoResetEvent(true);
        private IPuppetMasterURL puppetMaster;

        private const int MAXEVENTSQUEUE = 10;

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
            Console.WriteLine("Name: {0}", Name);
            Console.WriteLine("Freeze: {0}", isFrozen);
            Console.WriteLine("Events Published: {0}", NumberOfEvents);
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
                bool published = false;
                Event ev = ProduceEvent(topic, content);
                while (!published)
                {
                    try
                    {
                        PrimaryBroker().Publish(ev);
                        UpdatePreviousEvents(ev);
                        published = true;
                        //puppetMaster.Log("PubEvent " + this.Name + ", " + this.Name + ", " + ev.Topic + ", " + this.NumberOfEvents); // faz sentido meter duas vezes o nome do processo? no enunciado esta
                    }
                    catch (System.Net.Sockets.SocketException) // primary broker is down. lets ask to see if there is a new one
                    {
                        this.siteBrokers.ConnectPrimaryBroker();
                    }
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
            if (PreviousEvents.Count == MAXEVENTSQUEUE)
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
                CheckFroozen();

                Publish(topic, this.Name + i);
                Thread.Sleep(waitingTime);
            }

        }


        /// <summary>
        /// Must get the primary broker and notify him that a brand new publisher is in town
        /// </summary>
        public void RegisterInSite(String brokerUrl1, String brokerUrl2, String brokerUrl3)
        {
            this.siteBrokers = new SiteBrokers(brokerUrl1, brokerUrl2, brokerUrl3);
            this.siteBrokers.ConnectPrimaryBroker();
            PrimaryBroker().registerPublisher(this.url);
        }


        /// <summary>
        /// Returns the primary broker of the site
        /// </summary>
        public IBroker PrimaryBroker()
        {
            return siteBrokers.PrimaryBroker;
        }

        #endregion
    
    }
}
