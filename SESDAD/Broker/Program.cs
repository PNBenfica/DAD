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
        // arga[1] -> site name
        // args[2] -> broker url
        // args[3] -> broker1 url
        // args[4] -> broker2 url
        // args[5] -> parent1 url
        // args[6] -> parent2 url
        // args[7] -> parent3 url
        // args[8] -> router
        // args[9] -> puppetMaster
        // args[10] -> logLevel
        static void Main(string[] args)
        {
            String name = args[0];
            String siteName = args[1];
            String url = args[2];
            String siteBroker1Url = args[3];
            String siteBroker2Url = args[4];
            String parentUrl1 = args[5];
            String parentUrl2 = args[6];
            String parentUrl3 = args[7];
            String router = args[8];
            String puppetMasterUrl = args[9];
            String loggingLevel = args[10];


            char[] delimiterChars = { ':', '/' }; // "tcp://1.2.3.4:3333/broker"
            string[] urlSplit = url.Split(delimiterChars);
            int port = Convert.ToInt32(urlSplit[4]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            Broker broker = new Broker(name, siteName, url, router, puppetMasterUrl, loggingLevel);
            RemotingServices.Marshal(broker, "broker", typeof(IBroker));

            Console.WriteLine("Broker {0} running on {1}", name, url);

            // if there is no parent URL, this is the root!!
            // if there is a parent, this broker must let him know he has a son

            broker.RegisterSiteBrokers(siteBroker1Url, siteBroker2Url);

            if (!parentUrl1.Equals("none"))
            {
                broker.RegisterParentSite(parentUrl1, parentUrl2, parentUrl3);
            }

            Console.ReadLine();
        }
    }
}
