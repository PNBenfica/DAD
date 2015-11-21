using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CommonTypes
{
    public class SiteBrokers
    {
        public string Name { get; set;}
        public IBroker PrimaryBroker { get; set; }
        private List<string> secondariesBrokers = new List<string>();

        public SiteBrokers(String brokerUrl1, String brokerUrl2, String brokerUrl3)
        {
            AddSiteBrokers(brokerUrl1, brokerUrl2, brokerUrl3);
        }

        public SiteBrokers(String name, String brokerUrl1, String brokerUrl2, String brokerUrl3)
            : this(brokerUrl1, brokerUrl2, brokerUrl3)
        {
            this.Name = name;
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
        /// Returns true if sucessfully connected 
        /// </summary>
        public bool ConnectPrimaryBroker()
        {
            bool connected = false;
            if (secondariesBrokers.Count > 0)
            {
                try
                {
                    String primaryBrokerUrl = GetPrimaryBrokerUrl();
                    secondariesBrokers.Remove(primaryBrokerUrl);
                    SetPrimaryBroker(primaryBrokerUrl);
                    connected = true;
                }
                catch (System.Net.Sockets.SocketException)
                {
                    secondariesBrokers.RemoveAt(0);
                    connected = ConnectPrimaryBroker();
                }
            }
            else
                Console.WriteLine("Can't connect to any broker...");
            return connected;
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
            this.PrimaryBroker = (IBroker)Activator.GetObject(typeof(IBroker), primaryBrokerUrl);
            Console.WriteLine("Connecting with primary broker at {0}", primaryBrokerUrl);
        }
    }
}
