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
        // args[2] -> broker1 url
        // args[3] -> broker2 url
        // args[4] -> broker3 url
        // args[5] -> order
        // args[6] -> puppetMaster
        // args[7] -> logLevel
        static void Main(string[] args)
        {
            String name = args[0];
            String url = args[1];
            String brokerUrl1 = args[2];
            String brokerUrl2 = args[3];
            String brokerUrl3 = args[4];
            String ordering = args[5];
            String puppetMasterUrl = args[6];
            String loggingLevel = args[7];

            char[] delimiterChars = { ':', '/' }; // "tcp://1.2.3.4:3335/sub"
            string[] urlSplit = url.Split(delimiterChars);
            int port = Convert.ToInt32(urlSplit[4]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            Subscriber subscriber = new Subscriber(name, url, ordering, puppetMasterUrl, loggingLevel);
            RemotingServices.Marshal(subscriber, "sub", typeof(ISubscriber));

            Console.WriteLine("Subscriber {0} running on {1}", name, url);

            subscriber.RegisterInSite(brokerUrl1, brokerUrl2, brokerUrl3);

            Console.ReadLine();
            subscriber.Subscribe("/benfica/ola/*");
            Console.ReadLine();
            subscriber.Subscribe("/benfica/Samaris");
            Console.ReadLine();
        }
    }
}
