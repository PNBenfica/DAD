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
        private List<Event> events;
        private String name;
        private String url;
        private IBroker broker;

        public Publisher(String name, String url)
        {
            this.name = name;
            this.url = url;
            events = new List<Event>();
        }


        public void Publish(String topic, String content)
        {
            Console.WriteLine("Publish new event in topic {0}", topic);
            Event e = new Event(this.name,content,topic,0);
        //   events.Add(e);

            broker.DiffuseMessageToRoot(e);
           
        }

        public void SequencePublish(String numberOfEvents, String topic, String waitXms)
        {
            int eventNumber = Convert.ToInt32(numberOfEvents);
            int waitingTime = Convert.ToInt32(waitXms);
            for (int i = 0; i < eventNumber; i++)
            {
                Publish(topic, this.name + i);
                Thread.Sleep(waitingTime);
            }

        }


        public void registerInBroker(String brokerUrl)
        {
            Console.WriteLine("Registing in broker at {0}", brokerUrl);
            this.broker = (IBroker)Activator.GetObject(typeof(IBroker), brokerUrl);
            this.broker.registerPublisher(this.url);
        }


    }
}
