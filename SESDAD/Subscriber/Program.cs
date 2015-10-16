using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;

namespace Subscriber
{
    class Program
    {
        // args[0] -> subscriber name
        // args[1] -> subscriber url
        // args[2] -> broker url
        static void Main(string[] args)
        {
            String name = args[0];
            String url = args[1];
            String brokerUrl = args[2];
            String ordering = args[3];
            String loggingLevel = args[4];

            char[] delimiterChars = { ':', '/' }; // "tcp://1.2.3.4:3335/sub"
            string[] urlSplit = url.Split(delimiterChars);
            int port = Convert.ToInt32(urlSplit[4]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            Subscriber subscriber = new Subscriber(name, url);
            RemotingServices.Marshal(subscriber, "sub", typeof(ISubscriber));

            Console.WriteLine("Subscriber {0} running on {1}", name, url);

            subscriber.registerInBroker(brokerUrl);
            Console.ReadLine();
            subscriber.Subscribe("/benfica/campeao");
            subscriber.Subscribe("/benfica");
            Console.ReadLine();
        }
    }
}
