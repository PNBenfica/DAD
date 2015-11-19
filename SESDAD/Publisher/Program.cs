using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;
using System.Threading;

namespace Publisher
{
    class Program
    {
        // args[0] -> Publisher name
        // args[1] -> Publisher url
        // args[2] -> broker1 url
        // args[3] -> broker2 url
        // args[4] -> broker3 url
        // args[5] -> puppetMaster
        // args[6] -> logLevel
        static void Main(string[] args)
        {
            String name = args[0];
            String url = args[1];
            String brokerUrl1 = args[2];
            String brokerUrl2 = args[3];
            String brokerUrl3 = args[4];
            String puppetMasterUrl = args[5];
            String loggingLevel = args[6];

            char[] delimiterChars = { ':', '/' }; // "tcp://1.2.3.4:3335/pub"
            string[] urlSplit = url.Split(delimiterChars);
            int port = Convert.ToInt32(urlSplit[4]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            Publisher publisher = new Publisher(name, url, puppetMasterUrl, loggingLevel);
            RemotingServices.Marshal(publisher, "pub", typeof(IPublisher));

            Console.WriteLine("Publisher {0} running on {1}", name, url);

            publisher.RegisterInSite(brokerUrl1, brokerUrl2, brokerUrl3);

            publisher.SequencePublish("100","/benfica", "500");

            Console.ReadLine();
        }
    }
}
