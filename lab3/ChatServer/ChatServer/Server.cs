using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace ChatServer
{
    class Server
    {
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel,false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ChatServerImpl),
                "ChatServer",
                WellKnownObjectMode.Singleton);
            System.Console.WriteLine("--Chat--");
            System.Console.ReadLine();
        }
    }
}
