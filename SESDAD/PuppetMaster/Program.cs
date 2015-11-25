using CommonTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Program
    {
        private static PuppetMaster puppetMaster;

        public static void Main(string[] args)
        {
            String filename = @"..\..\..\T4-config.txt";
            if (args.Length > 0)
                filename = @args[0];

            Configurations configurations = ReadConfig(filename);
            initializeProcesses(configurations);
            createMenu();
        }

        public static Configurations ReadConfig(String filename)
        {
            FileParser parser = new FileParser();
            return parser.parse(filename, @"..\..\..\puppetMasters.txt");
        }

        public static void initializeProcesses(Configurations configurations)
        {
            ProcessCreator processCreator = new ProcessCreator();
            Console.WriteLine("Initializing processes");

            String url = configurations.PuppetMasterUrl;
            char[] delimiterChars = { ':', '/' }; // "tcp://1.2.3.4:3335/sub"
            string[] urlSplit = url.Split(delimiterChars);
            int port = Convert.ToInt32(urlSplit[4]);

            
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            puppetMaster = new PuppetMaster(configurations.PuppetMasterUrl, configurations.RoutingPolicy, configurations.Ordering, configurations.LoggingLevel, configurations.CentralPuppetMasterUrl, configurations.PuppetMastersUrl);

            
            RemotingServices.Marshal(puppetMaster, "puppet", typeof(IPuppetMasterURL));

            Console.WriteLine("puppetMaster running on {0}", url);

            foreach (Process process in configurations.Processes)
            {
                Console.WriteLine(process.Type);
                if (process.Type.Equals("broker"))
                {
                    processCreator.startBrokerProcess(process.Name, process.Url, process.BrokersUrl, process.NeighbourBrokers, configurations.RoutingPolicy, configurations.Ordering, configurations.CentralPuppetMasterUrl, configurations.LoggingLevel, process.Site);
                    puppetMaster.AddBroker(process.Name, process.Url);
                }
                else if (process.Type.Equals("publisher"))
                {
                    processCreator.startPublisherProcess(process.Name, process.Url, process.BrokersUrl, configurations.CentralPuppetMasterUrl, configurations.LoggingLevel);
                    puppetMaster.AddPublisher(process.Name, process.Url);
                }
                else if (process.Type.Equals("subscriber"))
                {
                    processCreator.startSubscriberProcess(process.Name, process.Url, process.BrokersUrl, configurations.CentralPuppetMasterUrl, configurations.LoggingLevel);
                    puppetMaster.AddSubscriber(process.Name, process.Url);                
                }
                else
                {
                    throw new UnknownProcessException("Unknown Process specified, aborting execution");
                }
                
            }

        }

        public static void readScripts(string script = null)
        {
            if (script == null)
            {
                string[] scriptFiles = { "easyPublishSubscribe", "twoPublishers", "freezeTest" };//, "unSubscribeTest" };
                foreach (string file in scriptFiles)
                {
                    string[] lines = System.IO.File.ReadAllLines(@"../../../" + file + ".txt");
                    foreach (string command in lines)
                    {
                        executeCommand(command);
                    }
                }
            }
            else
            {
                string[] lines = System.IO.File.ReadAllLines("../../../" + script + ".txt");
                foreach (string command in lines)
                {
                    executeCommand(command);
                }
            }


        }


        public static void createMenu()
        {
            Console.WriteLine("-----------------PuppetMaster Console-----------------");
            Console.Write(">");
            String command;

            while ((command = Console.ReadLine()) != null)
            {
                try
                {
                    executeCommand(command);
                }
                catch (UnknownCommandException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                Console.Write("> ");
            }
            while (true) ;
        }

        private static void executeCommand(string command)
        {
            Console.WriteLine(command);
            if (command == null)
            {
                return;
            }
            char[] delimiter = { ' ' };
            String[] words = command.Split(delimiter);
            //Subscriber processname Subscribe topicname
            if (words[0].ToLower().Equals("subscriber") && words[2].ToLower().Equals("subscribe"))
            {
                puppetMaster.Subscribe(words[1], words[3]);
            }
            //Subscriber processname Unsubscribe topicname
            else if (words[0].ToLower().Equals("subscriber") && words[2].ToLower().Equals("unsubscribe"))
            {
                puppetMaster.Unsubscribe(words[1], words[3]);
            }
            //Publisher processname Publish numberofevents Ontopic topicname Interval xms.
            else if (words[0].ToLower().Equals("publisher") && words[2].ToLower().Equals("publish"))
            {
                puppetMaster.Publish(words[1], words[3], words[5], words[7]);
            }
            //Status
            else if (words[0].ToLower().Equals("status"))
            {
                puppetMaster.Status();
            }
            //Crash processname
            else if (words[0].ToLower().Equals("crash"))
            {
                puppetMaster.Crash(words[1]);
            }
            //Freeze processname
            else if (words[0].ToLower().Equals("freeze"))
            {
                puppetMaster.Freeze(words[1]);
            }
            //Unfreeze processname
            else if (words[0].ToLower().Equals("unfreeze"))
            {
                puppetMaster.Unfreeze(words[1]);
            }
            //Wait xms
            else if (words[0].ToLower().Equals("wait"))
            {
                int time = Convert.ToInt32(words[1]);
                Thread.Sleep(time);
            }
            //Clear
            else if (words[0].ToLower().Equals("clear"))
            {
                Console.Clear();
            }
            //Exit
            else if (words[0].Equals("exit"))
            {
                Environment.Exit(0);
            }

                // depois alterem isto caso queiram ja que isto nao faz parte dos comandos para a aplicacao
            else if (words[0].ToLower().Equals("script") && words.Length == 1)
            {
                readScripts();
            }
            else if (words[0].ToLower().Equals("script") && words.Length == 2)
            {
                readScripts(words[1]);
            }
            //help
            else if (words[0].ToLower().Equals("help"))
            {
                Console.WriteLine("Subscriber processname Subscribe topicname");
                Console.WriteLine("Subscriber processname Unsubscribe topicname");
                Console.WriteLine("Publisher processname Publish numberofevents Ontopic topicname Interval xms");
                Console.WriteLine("Status");
                Console.WriteLine("Crash processname");
                Console.WriteLine("Freeze processname");
                Console.WriteLine("Unfreeze processname");
                Console.WriteLine("Wait xms");
                Console.WriteLine("Help");
                Console.WriteLine("Clear");
                Console.WriteLine("Exit");
            }
            else
            {
                throw new UnknownCommandException("Command not recognized\ntype HELP to list all available commands");
            }
        }




    }
}
