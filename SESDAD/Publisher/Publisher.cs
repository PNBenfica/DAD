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
        private IBroker primaryBroker;
        private List<string> secondariesBrokers = new List<string>();
        public Queue<Event> PreviousEvents { get; set; }
        public int NumberOfEvents { get; set; }
        private string loggingLevel;

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
                        DateTime timeStamp = primaryBroker.Publish(ev);
                        ev.TimeStamp = timeStamp;
                        UpdatePreviousEvents(ev);
                        published = true;
                        //puppetMaster.Log("PubEvent " + this.Name + ", " + this.Name + ", " + ev.Topic + ", " + this.NumberOfEvents); // faz sentido meter duas vezes o nome do processo? no enunciado esta
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        ConnectPrimaryBroker();
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


        /// <summary>
        /// Must get the primary broker and notify him that a brand new publisher is in town
        /// </summary>
        public void RegisterInSite(String brokerUrl1, String brokerUrl2, String brokerUrl3)
        {
            AddSiteBrokers(brokerUrl1, brokerUrl2, brokerUrl3);
            ConnectPrimaryBroker();
            this.primaryBroker.registerPublisher(this.url);
        }


        /// <summary>
        /// Adds the three brokers to the secondaries brokers. Currently the publisher doesn't know who is primary broker
        /// </summary>
        private void AddSiteBrokers(String brokerUrl1, String brokerUrl2, String brokerUrl3)
        {
            secondariesBrokers.Add(brokerUrl1);
            secondariesBrokers.Add(brokerUrl2);
            secondariesBrokers.Add(brokerUrl3);
        }


        /// <summary>
        /// Asks the first replication broker what is the primary broker URL
        /// </summary>
        private void ConnectPrimaryBroker()
        {
            if (secondariesBrokers.Count > 0)
            {
                try
                {
                    String primaryBrokerUrl = GetPrimaryBrokerUrl();
                    secondariesBrokers.Remove(primaryBrokerUrl);
                    SetPrimaryBroker(primaryBrokerUrl);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    secondariesBrokers.RemoveAt(0);
                    ConnectPrimaryBroker();
                }
            }
            else
                Console.WriteLine("Can't connect to any broker...");
        }


        /// <summary>
        /// Asks the URL of the primary broker to a secondary broker
        /// </summary>
        private string GetPrimaryBrokerUrl()
        {
            IBroker broker = (IBroker)Activator.GetObject(typeof(IBroker), secondariesBrokers[0]);
            String primaryBrokerUrl = broker.PrimaryBrokerUrl();
            while (!secondariesBrokers.Contains(primaryBrokerUrl)) // if it doesnt contain means that the reeletion isnt over
            {
                Thread.Sleep(200);
                primaryBrokerUrl = broker.PrimaryBrokerUrl();
            }
            return primaryBrokerUrl;
        }


        /// <summary>
        /// Gets the remote object from the URL of the primary broker
        /// </summary>
        /// <param name="primaryBrokerUrl">Url of the primary broker</param>
        private void SetPrimaryBroker(String primaryBrokerUrl)
        {
            this.primaryBroker = (IBroker)Activator.GetObject(typeof(IBroker), primaryBrokerUrl);
            Console.WriteLine("Connecting with primary broker at {0}", primaryBrokerUrl);
        }

        #endregion
    
    }
}
