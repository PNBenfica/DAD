using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Reflection;


namespace Subscriber
{
    public class Subscriber : MarshalByRefObject, ISubscriber
    {

        #region variables
        private String name;
        private bool freeze = false;
        private String url;
        private SiteBrokers siteBrokers;
        private IPuppetMasterURL puppetMaster;
        private string loggingLevel;
        public Topic<Subscription> Subscriptions { get; set; }
        #endregion

        #region classUtils

        public Subscriber(string name, string url, string ordering, string puppetMasterUrl, string loggingLevel)
        {
            this.name = name;
            this.url = url;
            this.puppetMaster = (IPuppetMasterURL)Activator.GetObject(typeof(IPuppetMasterURL), puppetMasterUrl);
            this.loggingLevel = loggingLevel;
            this.Subscriptions = new Topic<Subscription>("/");
        }

        public void PrintMessage(Event e)
        {
            //puppetMaster.Log("SubEvent " + this.name + ", " + e.PublisherId + ", " + e.Topic + ", " + e.Id);
            Console.WriteLine("");
            Console.WriteLine("------- New Message -------");
            Console.WriteLine("Post ID:" + e.Id);
            Console.WriteLine("Publisher: {0}\r\nTopic: {1}\r\nContent: {2}", e.PublisherId, e.Topic, e.Content);
        }

        /// <summary>
        /// Divides the string topic into String[]
        /// </summary>
        public String[] tokenize(String topic)
        {
            char[] delimiterChars = { '/' };
            string[] topicSplit = topic.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            return topicSplit;
        }

        /// <summary>
        /// returns true if the subscriber has a subscrition in the topic
        /// </summary>
        public bool HasSubscrition(string topic)
        {
            return Subscriptions.HasSubscrition(new Subscription(name), tokenize(topic));
        }

        public DateTime TopicTimeStamp(string topic)
        {
            List<Subscription> subscriptions = Subscriptions.GetSubscribers(tokenize(topic));
            if (subscriptions != null && subscriptions.Count > 0)
                return subscriptions[0].TimeStamp;
            return DateTime.Now;
        }


        /// <summary>
        /// Must get the primary broker and notify him that a brand new subscriber is in town
        /// </summary>
        public void RegisterInSite(String brokerUrl1, String brokerUrl2, String brokerUrl3)
        {
            this.siteBrokers = new SiteBrokers(brokerUrl1, brokerUrl2, brokerUrl3);
            this.siteBrokers.ConnectPrimaryBroker();
            PrimaryBroker().registerSubscriber(this.name, this.url);
        }


        /// <summary>
        /// Returns the primary broker of the site
        /// </summary>
        public IBroker PrimaryBroker()
        {
            return siteBrokers.PrimaryBroker;
        }

        #endregion

        #region remoteMethods

        public void Subscribe(string topic)
        {
            lock (this)
            {
                Console.WriteLine("New Subscrition on Topic: {0}", topic);
                bool subscribed = false;
                while (!subscribed)
                {
                    try
                    {
                        DateTime timeStamp = PrimaryBroker().Subscribe(this.name, true, topic);
                        Subscription subscription = new Subscription(name, timeStamp);
                        Subscriptions.Subscribe(subscription, tokenize(topic));
                        subscribed = true;
                    }
                    catch (System.Net.Sockets.SocketException) // primary broker is down. lets ask to see if there is a new one
                    {
                        this.siteBrokers.ConnectPrimaryBroker();
                    }
                }
            }
        }

        public void UnSubscribe(string topic)
        {
            lock (this)
            {
                Console.WriteLine("Unsubscrition on Topic: {0}", topic);
                bool unsubscribed = false;
                while (!unsubscribed)
                {
                    try
                    {
                        Subscriptions.UnSubscribe(new Subscription(name), tokenize(topic));
                        PrimaryBroker().UnSubscribe(this.name, true, topic);
                    }
                    catch (System.Net.Sockets.SocketException) // primary broker is down. lets ask to see if there is a new one
                    {
                        this.siteBrokers.ConnectPrimaryBroker();
                    }
                }
            }
        }

        /// <summary>
        /// This method is called when a new event arrives from the broker
        /// </summary>
        /// <param name="e"></param>
        public void ReceiveMessage(Event e)
        {
            lock (this)
            {
                PrintMessage(e);
            }
        }

        public void Status()
        {
            Console.WriteLine("\r\n<------Status------>");
            Console.WriteLine("Name: {0}", name);
            Console.WriteLine("--Subscriptions--");
            Subscriptions.Status();
            Console.WriteLine("");

        }

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

        #endregion

    }
}
