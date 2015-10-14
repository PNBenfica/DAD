using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Publisher
{
    class Publisher : MarshalByRefObject, IPublisher
    {
        private List<Event> events;
        private String publisherId;
        private String url;
        private IBroker broker;
        public Publisher(String publisherId, String url)
        {
            this.publisherId = publisherId;
            this.url = url;
        }


        public void Publish(String topic, String content)
        {

        }

        public void registerInBroker(String brokerUrl)
        {
            Console.WriteLine("Registing in broker at {0}", brokerUrl);
            this.broker = (IBroker)Activator.GetObject(typeof(IBroker), brokerUrl);
            this.broker.registerSubscriber(this.url);
        }


    }
}
