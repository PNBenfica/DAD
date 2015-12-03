using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Reflection;
using System.Threading;


namespace Subscriber
{
    public class Subscriber : MarshalByRefObject, ISubscriber
    {

        #region variables
        private String name;
        private String url;
        private SiteBrokers siteBrokers;
        private IPuppetMasterURL puppetMaster;
        private string loggingLevel;

        private bool isFrozen = false;
        Object freezeLock = new Object();
        AutoResetEvent notFreezed = new AutoResetEvent(true);
        #endregion

        #region classUtils

        public Subscriber(string name, string url, string puppetMasterUrl, string loggingLevel)
        {
            this.name = name;
            this.url = url;
            this.puppetMaster = (IPuppetMasterURL)Activator.GetObject(typeof(IPuppetMasterURL), puppetMasterUrl);
            this.loggingLevel = loggingLevel;
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
                CheckFroozen();
                Console.WriteLine("Subscrition | Topic: {0}", topic);
                SendToParent(() => PrimaryBroker().Subscribe(this.name, true, topic));
            }
        }

        public void UnSubscribe(string topic)
        {
            lock (this)
            {
                CheckFroozen();
                Console.WriteLine("Unsubscrition | Topic: {0}", topic);
                SendToParent(() => PrimaryBroker().UnSubscribe(this.name, true, topic));
            }
        }

        public void SendToParent(Action method)
        {
            while (true)
            {
                try
                {
                    method();
                    break;
                }
                catch (System.Net.Sockets.SocketException) // primary broker is down. lets ask to see if there is a new one
                {
                    this.siteBrokers.ConnectPrimaryBroker();
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
                CheckFroozen();
                PrintMessage(e);
            }
        }

        public void PrintMessage(Event e)
        {
            puppetMaster.Log("SubEvent " + this.name + ", " + e.PublisherId + ", " + e.Topic + ", " + e.Id);
            Console.WriteLine("Event ID: {0} | Publisher: {1} | Topic: {2} | Content: {3}", e.Id, e.PublisherId, e.Topic, e.Content);
        }

        public void Status()
        {
            Console.WriteLine("\r\n<------Status------>");
            Console.WriteLine("Name: {0}", name);
            Console.WriteLine("");

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

        #endregion

    }
}
