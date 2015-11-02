using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;

namespace Broker
{
    class Program
    {
        // args[0] -> subscriber name
        // args[1] -> subscriber url
        // args[2] -> parent url
        // args[3] -> router
        // args[4] -> puppetMaster
        // args[5] -> logLevel
        static void Main(string[] args)
        {
            String name = args[0];
            String url = args[1];
            String parentUrl = args[2];
            String router = args[3];
            String puppetMasterUrl = args[4];
            String loggingLevel = args[5];

            char[] delimiterChars = { ':', '/' }; // "tcp://1.2.3.4:3333/broker"
            string[] urlSplit = url.Split(delimiterChars);
            int port = Convert.ToInt32(urlSplit[4]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            Broker broker = new Broker(name, url,  router, puppetMasterUrl, loggingLevel);
            RemotingServices.Marshal(broker, "broker", typeof(IBroker));

            Console.WriteLine("Broker {0} running on {1}", name, url);

            // if there is no parent URL, this is the root!!
            // if there is a parent, this broker must let him know he has a son
            
            if (!parentUrl.Equals("none")) 
            {
                broker.notifyParent(parentUrl);
            }

            Console.ReadLine();
        }
    }
}
