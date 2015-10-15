using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Program
    {
        public static void Main(string[] args)
        {
            Configurations configurations = ReadConfig();
            Console.WriteLine(configurations.ToString());
            initializeProcesses(configurations);
            Console.ReadLine();
        }

        public static Configurations ReadConfig()
        {
            FileParser parser = new FileParser();
            return parser.parse("../../../config.txt");
        }

        public static void initializeProcesses(Configurations configurations)
        {
            Console.WriteLine("Initializing processes");
            foreach(Process process in configurations.Processes)
            {
                if (process.Type.Equals("broker"))
                {
                    startBrokerProcess(process);
                }
                else if(process.Type.Equals("publisher"))
                {
                    //startPublisherProcess(process);
                }
                else if (process.Type.Equals("subscriber"))
                {
                    startSubscriberProcess(process);
                }
                else
                {
                    throw new UnknownProcessException("Unknown Process specified, aborting execution");
                }

            }
            
        }

        private static void startSubscriberProcess(Process process)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "" + process.Name + " " + process.Url + " " + process.BrokerUrl;
            Console.WriteLine(startInfo.Arguments);
            startInfo.FileName = @"Subscriber.exe";
            System.Diagnostics.Process.Start(startInfo);
        }

        private static void startPublisherProcess(Process process)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "" + process.Name + " " + process.Url + " " + process.BrokerUrl;
            Console.WriteLine(startInfo.Arguments);
            startInfo.FileName = @"Publisher.exe";
            System.Diagnostics.Process.Start(startInfo);
        }

        private static void startBrokerProcess(Process process)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "" + process.Name + " " + process.Url + " " +process.BrokerUrl;
            Console.WriteLine(startInfo.Arguments);
            startInfo.FileName = @"Broker.exe";
            System.Diagnostics.Process.Start(startInfo);
        }

        public static void createMenu()
        {

        }

    }
}
