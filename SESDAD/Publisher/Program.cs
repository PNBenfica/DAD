﻿using System;
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
        // args[2] -> broker url
        static void Main(string[] args)
        {
            String name = args[0];
            String url = args[1];
            String brokerUrl = args[2];
            String loggingLevel = args[3];

            char[] delimiterChars = { ':', '/' }; // "tcp://1.2.3.4:3335/pub"
            string[] urlSplit = url.Split(delimiterChars);
            int port = Convert.ToInt32(urlSplit[4]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            Publisher publisher = new Publisher(name, url);
            RemotingServices.Marshal(publisher, "pub", typeof(IPublisher));

            Console.WriteLine("Publisher {0} running on {1}", name, url);

            publisher.registerInBroker(brokerUrl);

            Console.ReadLine();
            //publisher.Publish("/benfica/campeao", "somos campeoes");
            //publisher.Publish("/benfica", "benfica benfica benfica");
            //publisher.Publish("/benfica/ola", "somos campeoes");
            Thread thread = new Thread(() =>
            {
                publisher.SequencePublish("20", "/benfica/ola", "0");
            });
            thread.Start();
            Thread thread1 = new Thread(() =>
            {
                publisher.SequencePublish("0", "/benfica/Samaris", "0");
            });
            thread1.Start();
           
            Console.ReadLine();
        }
    }
}
