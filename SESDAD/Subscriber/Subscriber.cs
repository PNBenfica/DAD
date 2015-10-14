using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Subscriber
{
    class Subscriber : MarshalByRefObject, ISubscriber
    {
        private String name;
        private String brokerUrl;
        private IBroker broker;
        private OrderStrategy orderStrategy;

        /// <summary>
        /// Subscriber Construtor
        /// </summary>
        /// <param name="name">subscriber name</param>
        /// <param name="brokerUrl">url of the site broker</param>
        public Subscriber(String name, String brokerUrl)
        {
            this.name = name;
            this.brokerUrl = brokerUrl;
        }

        public void Subscribe(string topic)
        {

        }

        public void UnSubscribe(string topic)
        {

        }

        public void ReceiveMessage(Event evento)
        {

        }
    }
}
