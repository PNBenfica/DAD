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

        private bool freeze = false;
        public IPuppetMasterURL puppetMaster;
        private bool FullLogging;
        public String Name { get; set; }
        public String URL { get; set; }
        public int Port { get; set; }

        public IBroker Parent { get; set; }
        public String ParentName { get; set; }
        public Dictionary<string, IBroker> Children { get; set; }

        public Dictionary<string, IBroker> SiteBrokers { get; set; }
        public bool IsPrimaryBroker { get; set; }
        public IBroker PrimaryBroker { get; set; }
        public Thread PingThread { get; set; } // thread used by primary broker to ping secondaries
        private System.Timers.Timer SecondaryBrokerTimer { get; set; } // timer used by secondaries to see how long the primary broker send the last ping

        public List<IPublisher> Publishers { get; set; }
        public Dictionary<string, ISubscriber> Subscribers { get; set; }

        public Router Router { get; set; }
        public OrderStrategy OrderStrategy { get; set; }

        public const int UNDEFINEDID = -1;

        #endregion

        #region classUtils

        public Broker(string name, string url, string router, string puppetMasterUrl, string loggingLevel)
        {
            this.Name = name;
            this.URL = url;
            this.Port = GetPortFromURL(url);
            this.puppetMaster = (IPuppetMasterURL)Activator.GetObject(typeof(IPuppetMasterURL), puppetMasterUrl);
            this.FullLogging = loggingLevel.ToLower().Equals("full");
            if (router.Equals("filter"))
            {
                this.Router = new FilteredRouter(this);
            }
            else
            {
               this.Router = new FloodingRouter(this);
           }

            OrderStrategy = new TotalOrder(this);
            //this.OrderStrategy = GetOrderByRefletion(ordering);
            this.Children = new Dictionary<string, IBroker>();
            this.Publishers = new List<IPublisher>();
            this.Subscribers = new Dictionary<string, ISubscriber>();
        }

        /// <summary>
        /// If the broker has no parent he is the root
        /// </summary>
        public bool IsRoot()
        {
            return this.Parent == null;
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
            if (FullLogging)
                puppetMaster.Log("BroEvent " + Name + ", " + e.PublisherId + ", " + e.Topic + ", " + e.Id);
        }


        #endregion

        #region remoteMethods

        public DateTime Subscribe(String Id, bool isSubscriber, String topic, bool isClimbing = false)
        {
            lock (this)
            {
                Console.WriteLine("New subscrition from: {0} on topic: {1}", Id, topic);
                return Router.addSubscrition(Id, isSubscriber, topic, isClimbing);
            }
        }

        public void UnSubscribe(String Id, bool isSubscriber, String topic)
        {
            lock (this)
            {
                Router.deleteSubscrition(Id, isSubscriber, topic);
            }
        }

        /// <summary>
        /// Diffuse the message down the tree. Router knows where this need to go
        /// </summary>
        public void DiffuseMessage(Event e)
        {
            Thread thread = new Thread(() =>
            {
                OrderStrategy.DeliverInOrder(e);
            });
            thread.Start();
        }

        
        /// <summary>
        /// Diffuse the event to the root
        /// </summary>
        public DateTime DiffuseMessageToRoot(Event e)
        {
            DateTime timeStamp;

            //start difusing if is root or parent not interested
            if (IsRoot() || !IsParentInterested(e.Topic))
            {
                timeStamp = DateTime.Now;
                e.TimeStamp = timeStamp;
                Thread thread = new Thread(() => { this.DiffuseMessage(e); });
                thread.Start();         
            }
            else
            {
                timeStamp = this.Parent.DiffuseMessageToRoot(e);                
            }

            return timeStamp;
        }



        /// <summary>
        /// Receive a new event from a publisher
        /// It must send it to the root, and atach info to that event
        /// No order does nothing, Fifo does nothing, Total order makes ID undefined
        /// </summary>
        public DateTime Publish(Event e)
        {
            if (OrderStrategy is TotalOrder)
            {
                e.Id = UNDEFINEDID;
                e.PreviousEvents = new List<Event>();
            }
            return DiffuseMessageToRoot(e);
        }


        /// <summary>
        /// Notify the broker parent that he has a new born child
        /// </summary>
        /// <param name="parentUrl">Broker parent url</param>
        internal void notifyParent(string parentUrl)
        {
            Console.WriteLine("Registing in parent at {0}", parentUrl);
            this.Parent = (IBroker)Activator.GetObject(typeof(IBroker), parentUrl);
            this.Parent.registerNewChild(this.Name, this.URL);
        }

        /// <summary>
        /// Register a new child
        /// </summary>
        /// <param name="url">Url of the new broker child</param>
        public void registerNewChild(string name, string url)
        {
            IBroker child = (IBroker)Activator.GetObject(typeof(IBroker), url);
            Children.Add(name, child);
            Console.WriteLine("New child broker registed: {0}", url);
        }


        public void registerPublisher(string url)
        {
            IPublisher publisher = (IPublisher)Activator.GetObject(typeof(IPublisher), url);
            Publishers.Add(publisher);
            Console.WriteLine("New publisher registed: {0}", url);
        }

        /// <summary>
        /// Register a new subscriber
        /// </summary>
        /// <param name="url">Url of the new subscriber</param>
        public void registerSubscriber(string name, string url)
        {
            ISubscriber subscriber = (ISubscriber)Activator.GetObject(typeof(ISubscriber), url);
            Subscribers.Add(name, subscriber);
            Console.WriteLine("New subscriber registed: {0}", url);
        }


        #region Replication

        /// <summary>
        /// Register the other brokers of this site and elects the new primary broker
        /// </summary>
        public void RegisterSiteBrokers(string siteBroker1Url, string siteBroker2Url)
        {
            IBroker broker1 = (IBroker)Activator.GetObject(typeof(IBroker), siteBroker1Url);
            IBroker broker2 = (IBroker)Activator.GetObject(typeof(IBroker), siteBroker2Url);
            SiteBrokers = new Dictionary<string, IBroker>();
            SiteBrokers.Add(siteBroker1Url, broker1);
            SiteBrokers.Add(siteBroker2Url, broker2);
            ElectSiteLeader();
        }

        /// <summary>
        /// Elect the new site leader. Criteria: highest port number
        /// If it is leader it starts sending Im alives else starts a timer to wait for them
        /// </summary>
        public void ElectSiteLeader()
        {
            int newLeaderPort = this.Port;
            this.IsPrimaryBroker = true;
            this.PrimaryBroker = null;

            foreach (string url in this.SiteBrokers.Keys)
            {
                int siteBrokerPort = GetPortFromURL(url);
                if (siteBrokerPort > newLeaderPort){
                    newLeaderPort = siteBrokerPort;
                    this.IsPrimaryBroker = false;
                    this.PrimaryBroker = this.SiteBrokers[url];
                }
            }

            if (IsPrimaryBroker)
                StartPing();
            else
                SetSecondaryBrokerTimer();
            Console.WriteLine(IsPrimaryBroker ? "Primary Broker of the site" : "Replication Broker of the site");
        }
        

        /// <summary>
        /// Creates a thread in the primary broker that sends I'm alives to the secondary brokers
        /// </summary>
        private void StartPing()
        {
            ThreadStart ts = new ThreadStart(this.SendImAlives);
            this.PingThread = new Thread(ts);
            this.PingThread.Start();
        }


        /// <summary>
        /// Sends a Im Alive message every 1500 ms
        /// If a replication broker is down it is removed from the list
        /// </summary>
        private void SendImAlives()
        {
            while (true)
            {
                foreach (IBroker broker in new List<IBroker>(SiteBrokers.Values)) // removing while iterating... better create a new list
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
            Console.WriteLine("Ping From Leader");
        }


        /// <summary>
        /// The timer start the counting from zeroooo
        /// </summary>
        private void RestartTimer()
        {
            SecondaryBrokerTimer.Stop();
            SecondaryBrokerTimer.Start();
        }
        

        /// <summary>
        /// The timer starts counting until 3000ms, if there is no Im alive message 
        /// from the leader lets pick another one
        /// </summary>
        private void SetSecondaryBrokerTimer()
        {
            SecondaryBrokerTimer = new System.Timers.Timer(3000);
            SecondaryBrokerTimer.Elapsed += ReElectSiteLider;
            SecondaryBrokerTimer.AutoReset = true;
            SecondaryBrokerTimer.Enabled = true;
        }


        /// <summary>
        /// If the primary broker is down its time to elect a new leader
        /// </summary>
        private void ReElectSiteLider(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Primary server gone down. Re-electing...");
            SecondaryBrokerTimer.Enabled = false;
            RemoveSiteBroker(this.PrimaryBroker);
            if (SiteBrokers.Count == 1) // check if the other broker is alive
            {
                try { SiteBrokers[SiteBrokers.Keys.First()].IsAlive(); }
                catch (System.Net.Sockets.SocketException)
                { RemoveSiteBroker(SiteBrokers[SiteBrokers.Keys.First()]); }
            }
            ElectSiteLeader();
        }


        /// <summary>
        /// Removes a broker from the Site Brokers list
        /// </summary>
        private void RemoveSiteBroker(IBroker broker)
        {
            var entry = SiteBrokers.FirstOrDefault(kvp => kvp.Value == broker);
            if (entry.Key != null)
                SiteBrokers.Remove(entry.Key);
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
            this.freeze = true;
        }

        public void Unfreeze()
        {
            this.freeze = false;
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

        public void notifyChildrenOfSubscription(string name, string topic, bool isClimbing = false)
        {
            Router.notifyChildrenOfSubscription(name, topic, isClimbing);
        }


        public bool IsParentInterested(string topic)
        {
            return Router.IsParentInterested(topic);
        }
    }
}
