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


        private bool isFrozen = false;
        Object freezeLock = new Object();
        AutoResetEvent notFreezed = new AutoResetEvent(true);

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
            Console.WriteLine("Routing | Id: {0} | Pub: {1} | Topic: {2} | Content: {3}", e.Id, e.PublisherId, e.Topic, e.Content);
            if (fullLogging)
                puppetMaster.Log("BroEvent " + Name + ", " + e.PublisherId + ", " + e.Topic + ", " + e.Id);
        }


        public void notifyChildrenOfSubscription(string name, string topic, bool isClimbing = false)
        {
            Router.notifyChildrenOfSubscription(name, topic, isClimbing);
        }


        /// <summary>
        /// returns true if the broker parent is interested in this topic
        /// </summary>
        public bool IsParentInterested(string topic)
        {
            return Router.IsParentInterested(topic);
        }

        #endregion

        #region remoteMethods

        /// <summary>
        /// received a new subscription. Lets add to the router so that he knows where to send stuff and also replicate this info
        /// </summary>
        public void Subscribe(String name, bool isSubscriber, String topic, bool isClimbing = false)
        {
            lock (Subscribers)
            {
                if (isPrimaryBroker)
                    SendToReplicas("Subscribe", name, isSubscriber, topic, isClimbing);
                CheckFroozen();
                Console.WriteLine("Subscrition | Subscriber: {0} | Topic: {1}", name, topic);
                Router.addSubscrition(name, isSubscriber, topic, isClimbing);
            }
        }


        /// <summary>
        /// received a new unsubscription. Lets add to the router so that he knows where to send stuff and also replicate this info
        /// </summary>
        public void UnSubscribe(String Id, bool isSubscriber, String topic)
        {
            lock (Subscribers)
            {
                if (isPrimaryBroker)
                    SendToReplicas("UnSubscribe", Id, isSubscriber, topic);
                CheckFroozen();
                Console.WriteLine("Unsubscrition | Subscriber: {0} | Topic: {1}", Id, topic);
                Router.deleteSubscrition(Id, isSubscriber, topic);
            }
        }


        /// <summary>
        /// Generic funtion that receives a method and an unspecified number of args and dynamically
        /// call this method in every replica of the site
        /// </summary>
        public void SendToReplicas(string method, params object[] args)
        {
            lock (this)
            {
                foreach (IBroker broker in new List<IBroker>(siteBrokers.Values)) // removing while iterating... better create a new list
                {
                    Thread thread = new Thread(() =>
                    {
                        try
                        {
                            typeof(IBroker).GetMethod(method).Invoke(broker, args);
                        }
                        catch (TargetInvocationException tie)
                        {
                            if (tie.InnerException is System.Net.Sockets.SocketException)
                                RemoveSiteBroker(broker);
                        }
                    });
                    thread.Start();
                }
            }
        }

        
        /// <summary>
        /// calls the action on the parent site
        /// </summary>
        public void SendToSite(SiteBrokers site, Action action)
        {
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (System.Net.Sockets.SocketException) // primary broker is down. lets ask to see if there is a new one
                {
                    site.ConnectPrimaryBroker();
                }
            }
        }

        
        /// <summary>
        /// this method is called when a broker is elected as the new leader and is recovering from a crash
        /// The broker must send all the queued messages
        /// </summary>
        private void SendQueuedEvents()
        {
            lock (this)
            {
                queuedEvents = queuedEvents.OrderBy(o => o.Id).ToList();
                foreach (Event e in queuedEvents)
                {
                    Log(e);
                    Router.route(e);
                }
                queuedEvents = new List<Event>();
            }
        }

        /// <summary>
        /// Diffuse the message down the tree. First lets order the event, then give the event o to router
        /// </summary>
        public void DiffuseMessage(Event e)
        {
            lock (this)
            {
                if (isPrimaryBroker)
                {
                    SendToReplicas("DiffuseMessage", e);
                    CheckFroozen();
                }
                else
                    queuedEvents.Add(e);
            }
            OrderStrategy.DeliverInOrder(e);
        }

        
        /// <summary>
        /// Method call when the primary broker send an event to all interested childs
        /// He must call this method on the replicas so that
        /// In case of crash, this event doenst have to be sent again
        /// </summary>
        public void SentEventNotification(Event e)
        {
            lock (queuedEvents)
            {
                queuedEvents.Remove(e);
            }
        }


        /// <summary>
        /// Method call when the primary broker send an event to a interested childs
        /// He must call this method on the replicas
        /// </summary>
        public void UpdateSentEvents(Event e, string name, bool isSubscriber)
        {
            lock (this)
            {
                this.Router.RecordSentInfo(e, name, isSubscriber);
            }
        }


        /// <summary>
        /// return true if broker has sent this event to the site
        /// </summary>
        public bool HasSentEvent(Event e, string name, bool isSubscriber)
        {
            return Router.HasSentEvent(e, name, isSubscriber);
        }
              
  
        /// <summary>
        /// Diffuse the event to the root
        /// </summary>
        public void DiffuseMessageToRoot(Event e)
        {
            if (IsRoot() || !IsParentInterested(e.Topic))
            {
                this.DiffuseMessage(e);
            }
            else
            {
                lock (this)
                {
                    SendToSite(ParentBrokers, () => ParentPrimaryBroker().DiffuseMessageToRoot(e));
                }
            }
        }


        /// <summary>
        /// Receive a new event from a publisher
        /// It must send it to the root, and atach info to that event
        /// No order does nothing, Fifo does nothing, Total order makes ID undefined
        /// </summary>
        public void Publish(Event e)
        {
            lock (this)
            {
                if (OrderStrategy is TotalOrder)
                {
                    e.Id = UNDEFINEDID;
                    e.PreviousEvents = new List<Event>();
                }
                DiffuseMessageToRoot(e);
            }
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
        /// Removes a subscriber when he crashes
        /// </summary>
        public void RemoveSubscriber(string subscriber)
        {
            if (isPrimaryBroker)
                SendToReplicas("RemoveSubscriber", subscriber);
            Subscribers.Remove(subscriber);
            Router.UnsubscribeAllTopics(subscriber);
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
            ThreadStart ts = new ThreadStart(this.SendImAlive);
            this.pingThread = new Thread(ts);
            this.pingThread.Start();
        }


        /// <summary>
        /// Sends a Im Alive message every 1500 ms
        /// If a replication broker is down it is removed from the list
        /// </summary>
        private void SendImAlive()
        {
            while (isPrimaryBroker)
            {
                SendImAlives();
                Thread.Sleep(1500);
            }
        }


        /// <summary>
        /// Generic funtion that receives a method and an unspecified number of args and dynamically
        /// call this method in every replica of the site
        /// </summary>
        public void SendImAlives()
        {
            foreach (IBroker broker in new List<IBroker>(siteBrokers.Values)) // removing while iterating... better create a new list
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        broker.ReceiveImAlive();
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
        /// When a secondary broker receives an Im alive from the primary broker the timer is reset
        /// </summary>
        public void ReceiveImAlive()
        {
            lock (this)
            {
                RestartTimer();
            }
        }


        /// <summary>
        /// The timer start the counting from zeroooo
        /// </summary>
        private void RestartTimer()
        {
            if (secondaryBrokerTimer != null)
            {
                secondaryBrokerTimer.Stop();
                secondaryBrokerTimer.Start();
            }
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
                if (!IsBrokerAlive(primaryBroker))
                {
                    Console.WriteLine("Primary server gone down. Re-electing...");
                    secondaryBrokerTimer.Enabled = false;

                    RemoveSiteBroker(this.primaryBroker);   // it must be removed because the election uses the current active site brokers

                    CheckAnyBrotherAlive();
                    ElectSiteLeader();

                    if (isPrimaryBroker)
                    {
                        SendQueuedEvents();
                    }
                }
            }
        }
        

        /// <summary>
        /// check if the other possible primary broker of the site is alive
        /// </summary>
        private void CheckAnyBrotherAlive()
        {
            if (siteBrokers.Count == 1)
            {
                IBroker brokerBrother = siteBrokers[siteBrokers.Keys.First()];
                if (!IsBrokerAlive(brokerBrother))
                    RemoveSiteBroker(brokerBrother);
            }
        }


        private bool IsBrokerAlive(IBroker broker)
        {
            try
            {
                broker.IsAlive();
                return true;
            }
            catch (System.Net.Sockets.SocketException)
            {
                return false;
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
        }

        #endregion
        
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
