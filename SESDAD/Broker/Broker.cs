using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Threading;
using System.Reflection;
using System.Timers;

namespace Broker
{
    public class Broker : MarshalByRefObject, IBroker
    {
        #region variables

        private bool isFrozen = false;
        public IPuppetMasterURL puppetMaster;
        private bool fullLogging;
        public String Name { get; set; }
        public String SiteName { get; set; }
        private String url;
        private int port;

        public SiteBrokers ParentBrokers { get; set; }
        public Dictionary<string, SiteBrokers> ChildrenSites { get; set; }

        private Dictionary<string, IBroker> siteBrokers;
        public bool isPrimaryBroker { get; set;}
        private IBroker primaryBroker;
        private string primaryBrokerUrl;
        private Thread pingThread; // thread used by primary broker to ping secondaries
        private System.Timers.Timer secondaryBrokerTimer; // timer used by secondaries to see how long the primary broker send the last ping         
        private List<Event> queuedEvents = new List<Event>();  // queued events in replicas waiting the confirmation of primary that they were sent
    
        public List<IPublisher> Publishers { get; set; }
        public Dictionary<string, ISubscriber> Subscribers { get; set; }

        public Router Router { get; set; }
        public OrderStrategy OrderStrategy { get; set; }

        public const int UNDEFINEDID = -1;

        #endregion

        #region classUtils

        public Broker(string name, string url, string siteName, string router, String ordering, string puppetMasterUrl, string loggingLevel)
        {
            this.Name = name;
            this.url = url;
            this.SiteName = siteName;
            this.port = GetPortFromURL(url);
            this.puppetMaster = (IPuppetMasterURL)Activator.GetObject(typeof(IPuppetMasterURL), puppetMasterUrl);
            this.fullLogging = loggingLevel.ToLower().Equals("full");
            if (router.Equals("filter"))
            {
                this.Router = new FilteredRouter(this);
            }
            else
            {
               this.Router = new FloodingRouter(this);
           }

            this.OrderStrategy = GetOrderByRefletion(ordering);
            this.ChildrenSites = new Dictionary<string, SiteBrokers>();

            //OrderStrategy = new TotalOrder(this);
            this.OrderStrategy = GetOrderByRefletion(ordering);
            this.Publishers = new List<IPublisher>();
            this.Subscribers = new Dictionary<string, ISubscriber>();
        }

        /// <summary>
        /// If the broker has no parent he is the root
        /// </summary>
        public bool IsRoot()
        {
            return this.ParentBrokers == null;
        }

        private OrderStrategy GetOrderByRefletion(string order)
        {
            order = "Broker." + Char.ToUpper(order[0]) + order.Substring(1).ToLower() + "Order";
            Assembly assembly = Assembly.Load("Broker");
            Type t = assembly.GetType(order);
            return (OrderStrategy)Activator.CreateInstance(t, new Object[] { this });
        }


        private int GetPortFromURL(string url)
        {
            char[] delimiterChars = { ':', '/' }; // "tcp://1.2.3.4:3333/broker"
            string[] urlSplit = url.Split(delimiterChars);
            int port = Convert.ToInt32(urlSplit[4]);
            return port;
        }


        public void Log(Event e)
        {
            Console.WriteLine("Diffusing message {0} from {1}", e.Id, e.PublisherId);
            //if (fullLogging)
            //    puppetMaster.Log("BroEvent " + Name + ", " + e.PublisherId + ", " + e.Topic + ", " + e.Id);
        }

        public void notifyChildrenOfSubscription(string name, string topic, bool isClimbing = false)
        {
            Router.notifyChildrenOfSubscription(name, topic, isClimbing);
        }


        public bool IsParentInterested(string topic)
        {
            return Router.IsParentInterested(topic);
        }

        #endregion

        #region remoteMethods

        public void Subscribe(String Id, bool isSubscriber, String topic, bool isClimbing = false)
        {
            lock (this)
            {
                Console.WriteLine("New subscrition from: {0} on topic: {1}", Id, topic);
                if (isPrimaryBroker)
                    SendSubscribeToReplicas(Id, isSubscriber, topic, isClimbing);
                Router.addSubscrition(Id, isSubscriber, topic, isClimbing);
            }
        }

        private void SendSubscribeToReplicas(string Id, bool isSubscriber, string topic, bool isClimbing)
        {
            foreach (IBroker broker in new List<IBroker>(siteBrokers.Values)) // removing while iterating... better create a new list
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        broker.Subscribe(Id, isSubscriber, topic, isClimbing);
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        RemoveSiteBroker(broker);
                    }
                });
                thread.Start();
            }
        }

        public void UnSubscribe(String Id, bool isSubscriber, String topic)
        {
            lock (this)
            {
                if (isPrimaryBroker)
                    SendUnSubscribeToReplicas(Id, isSubscriber, topic);
                Router.deleteSubscrition(Id, isSubscriber, topic);
            }
        }

        private void SendUnSubscribeToReplicas(string Id, bool isSubscriber, string topic)
        {
            foreach (IBroker broker in new List<IBroker>(siteBrokers.Values)) // removing while iterating... better create a new list
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        broker.UnSubscribe(Id, isSubscriber, topic);
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        RemoveSiteBroker(broker);
                    }
                });
                thread.Start();
            }
        }


        private void SendQueuedEvents()
        {
            lock (this)
            {
                Console.WriteLine(queuedEvents.Count);
                foreach (Event e in queuedEvents)
                {
                    Log(e);
                    Router.route(e);
                }
                queuedEvents = new List<Event>();
            }
        }

        /// <summary>
        /// Diffuse the message down the tree. Router knows where this need to go
        /// </summary>
        public void DiffuseMessage(Event e)
        {
            if (isPrimaryBroker)
                SendEventToReplicas(e);
            else
                queuedEvents.Add(e);
            OrderStrategy.DeliverInOrder(e);
        }


        public void SendEventToReplicas(Event e)
        {
            foreach (IBroker broker in new List<IBroker>(siteBrokers.Values)) // removing while iterating... better create a new list
            {
                Thread thread = new Thread(() => {
                    try
                    { 
                        broker.DiffuseMessage(e); 
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        RemoveSiteBroker(broker);
                    }
                });
                thread.Start();
            }
        }


        /// <summary>
        /// notify replicas that a event was sent
        /// </summary>
        public void SendReplicasEventSent(Event e)
        {
            foreach (IBroker broker in new List<IBroker>(siteBrokers.Values)) // removing while iterating... better create a new list
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        broker.SentEventNotification(e);
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        RemoveSiteBroker(broker);
                    }
                });
                thread.Start();
            }
        }


        /// <summary>
        /// primary broker notify replica that have send an event to all interested
        /// </summary>
        public void SentEventNotification(Event e)
        {
            queuedEvents.Remove(e);
        }

                
        /// <summary>
        /// Diffuse the event to the root
        /// </summary>
        public void DiffuseMessageToRoot(Event e)
        {
            //start difusing if is root or parent not interested
            if (IsRoot() || !IsParentInterested(e.Topic))
            {
                Thread thread = new Thread(() => { this.DiffuseMessage(e); });
                thread.Start();         
            }
            else
            {
                ParentPrimaryBroker().DiffuseMessageToRoot(e);                
            }
        }



        /// <summary>
        /// Receive a new event from a publisher
        /// It must send it to the root, and atach info to that event
        /// No order does nothing, Fifo does nothing, Total order makes ID undefined
        /// </summary>
        public void Publish(Event e)
        {
            if (OrderStrategy is TotalOrder)
            {
                e.Id = UNDEFINEDID;
                e.PreviousEvents = new List<Event>();
            }
            DiffuseMessageToRoot(e);
        }


        /// <summary>
        /// Notify the broker parent that he has a new born child
        /// </summary>
        /// <param name="parentUrl">Broker parent url</param>
        public void RegisterParentSite(string parentUrl1, string parentUrl2, string parentUrl3)
        {
            Console.WriteLine("Registing in parent...");
            ParentBrokers = new SiteBrokers(parentUrl1, parentUrl2, parentUrl3);
            ParentBrokers.ConnectPrimaryBroker();
            if (isPrimaryBroker)
            {
                List<string> siteBrokersUrl = siteBrokers.Keys.ToList();
                ParentPrimaryBroker().registerNewChildSite(this.SiteName, this.url, siteBrokersUrl[0], siteBrokersUrl[1]);
            }
        }

        /// <summary>
        /// Register a new child site
        /// </summary>
        /// <param name="url">Url of the new broker child</param>
        public void registerNewChildSite(string siteName, string primaryBroker, string secondaryBroker1, string secondaryBroker2)
        {
            Console.WriteLine("New child site registered: {0}", siteName);
            SiteBrokers childrenSite = new SiteBrokers(siteName, primaryBroker, secondaryBroker1, secondaryBroker2);
            childrenSite.SetPrimaryBroker(primaryBroker);
            ChildrenSites.Add(siteName, childrenSite);
            if (isPrimaryBroker)
            {
                foreach (IBroker broker in siteBrokers.Values)
                    broker.registerNewChildSite(siteName, primaryBroker, secondaryBroker1, secondaryBroker2);
            }
        }


        public void registerPublisher(string url)
        {
            IPublisher publisher = (IPublisher)Activator.GetObject(typeof(IPublisher), url);
            Publishers.Add(publisher);
            Console.WriteLine("New publisher registered: {0}", url);
            if (isPrimaryBroker)
            {
                foreach (IBroker broker in siteBrokers.Values)
                    broker.registerPublisher(url);
            }
        }


        /// <summary>
        /// Register a new subscriber
        /// </summary>
        /// <param name="url">Url of the new subscriber</param>
        public void registerSubscriber(string name, string url)
        {
            ISubscriber subscriber = (ISubscriber)Activator.GetObject(typeof(ISubscriber), url);
            Subscribers.Add(name, subscriber);
            Console.WriteLine("New subscriber registered: {0}", url);
            if (isPrimaryBroker)
            {
                foreach (IBroker broker in siteBrokers.Values)
                    broker.registerSubscriber(name, url);
            }
        }


        /// <summary>
        /// Returns the primary broker of the parent site
        /// </summary>
        public IBroker ParentPrimaryBroker()
        {
            return ParentBrokers.PrimaryBroker;
        }

        public string ParentSiteName()
        {
            return ParentBrokers.Name;
        }


        #region Replication


        /// <summary>
        /// Register the other brokers of this site and elects the new primary broker
        /// </summary>
        public void RegisterSiteBrokers(string siteBroker1Url, string siteBroker2Url)
        {
            IBroker broker1 = (IBroker)Activator.GetObject(typeof(IBroker), siteBroker1Url);
            IBroker broker2 = (IBroker)Activator.GetObject(typeof(IBroker), siteBroker2Url);
            siteBrokers = new Dictionary<string, IBroker>();
            siteBrokers.Add(siteBroker1Url, broker1);
            siteBrokers.Add(siteBroker2Url, broker2);
            ElectSiteLeader();
        }

        /// <summary>
        /// Elect the new site leader. Criteria: highest port number
        /// If it is leader it starts sending Im alives else starts a timer to wait for them
        /// </summary>
        public void ElectSiteLeader()
        {
            int newLeaderPort = this.port;
            this.isPrimaryBroker = true;
            this.primaryBroker = this;
            this.primaryBrokerUrl = this.url;

            foreach (string url in this.siteBrokers.Keys)
            {
                int siteBrokerPort = GetPortFromURL(url);
                if (siteBrokerPort > newLeaderPort){
                    newLeaderPort = siteBrokerPort;
                    this.isPrimaryBroker = false;
                    this.primaryBroker = this.siteBrokers[url];
                    this.primaryBrokerUrl = url;
                }
            }

            if (isPrimaryBroker)
                StartPing();
            else
                SetSecondaryBrokerTimer();
            Console.WriteLine(isPrimaryBroker ? "Primary Broker in " + SiteName: "Replication Broker in " + SiteName);
        }
        

        /// <summary>
        /// Creates a thread in the primary broker that sends I'm alives to the secondary brokers
        /// </summary>
        private void StartPing()
        {
            ThreadStart ts = new ThreadStart(this.SendImAlives);
            this.pingThread = new Thread(ts);
            this.pingThread.Start();
        }


        /// <summary>
        /// Sends a Im Alive message every 1500 ms
        /// If a replication broker is down it is removed from the list
        /// </summary>
        private void SendImAlives()
        {
            while (true)
            {
                foreach (IBroker broker in new List<IBroker>(siteBrokers.Values)) // removing while iterating... better create a new list
                {
                    Thread thread = new Thread(() => 
                    {
                        try
                        {
                            broker.ReceiveImAlive();
                        }
                        catch (System.Net.Sockets.SocketException) // this exception could take a while, so we send all Im alives in different threads
                        {
                            RemoveSiteBroker(broker);
                        }
                    });
                    thread.Start(); 
                }
                Thread.Sleep(1500);
            }
        }


        /// <summary>
        /// When a secondary broker receives an Im alive from the primary broker the timer is reset
        /// </summary>
        public void ReceiveImAlive()
        {
            RestartTimer();
        }


        /// <summary>
        /// The timer start the counting from zeroooo
        /// </summary>
        private void RestartTimer()
        {
            secondaryBrokerTimer.Stop();
            secondaryBrokerTimer.Start();
        }
        

        /// <summary>
        /// The timer starts counting until 3000ms, if there is no Im alive message 
        /// from the leader lets pick another one
        /// </summary>
        private void SetSecondaryBrokerTimer()
        {
            secondaryBrokerTimer = new System.Timers.Timer(3000);
            secondaryBrokerTimer.Elapsed += ReElectSiteLider;
            secondaryBrokerTimer.AutoReset = true;
            secondaryBrokerTimer.Enabled = true;
        }


        /// <summary>
        /// If the primary broker is down its time to elect a new leader
        /// </summary>
        private void ReElectSiteLider(Object source, ElapsedEventArgs e)
        {
            lock (this)
            {
                Console.WriteLine("Primary server gone down. Re-electing...");
                secondaryBrokerTimer.Enabled = false;
                RemoveSiteBroker(this.primaryBroker);
                if (siteBrokers.Count == 1) // check if the other broker is alive
                {
                    try { siteBrokers[siteBrokers.Keys.First()].IsAlive(); }
                    catch (System.Net.Sockets.SocketException)
                    { RemoveSiteBroker(siteBrokers[siteBrokers.Keys.First()]); }
                }
                ElectSiteLeader();
                if (isPrimaryBroker)
                {
                    SendQueuedEvents();
                }
            }
        }


        /// <summary>
        /// Removes a broker from the Site Brokers list
        /// </summary>
        private void RemoveSiteBroker(IBroker broker)
        {
            var entry = siteBrokers.FirstOrDefault(kvp => kvp.Value == broker);
            if (entry.Key != null)
                siteBrokers.Remove(entry.Key);
        }

        public string PrimaryBrokerUrl()
        {
            return primaryBrokerUrl;
        }


        /// <summary>
        /// Dumb Method Just to say that it is Alive
        /// </summary>
        public void IsAlive()
        {
            Console.WriteLine("I'm Alive");
        }

        #endregion

        public void Freeze()
        {
            this.isFrozen = true;
        }

        public void Unfreeze()
        {
            this.isFrozen = false;
        }

        public void Crash()
        {
            System.Environment.Exit(0);
        }


        /// <summary>
        /// Write in console the current status
        /// </summary>
        public void Status()
        {
            Console.WriteLine("\r\n<------Status------>");
            Console.WriteLine("Name: {0}", Name);
            Router.Status();
            Console.WriteLine("");
        }

        #endregion

    }
}
