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
            events = new List<Event>();
        }


        public void Publish(String topic, String content)
        {
            Console.WriteLine("Publish new event in topic {0}", topic);
            Event e = new Event(this.publisherId,content,topic,0);
        //   events.Add(e);

            broker.DiffuseMessageToRoot(e);
           
        }

        public void registerInBroker(String brokerUrl)
        {
            Console.WriteLine("Registing in broker at {0}", brokerUrl);
            this.broker = (IBroker)Activator.GetObject(typeof(IBroker), brokerUrl);
            this.broker.registerPublisher(this.url);
        }


    }
}
