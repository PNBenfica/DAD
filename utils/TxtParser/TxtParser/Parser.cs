using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxtParser
{
    class Parser
    {

        public static void Main(string[] args)
        {

            // Read each line of the file into a string array. Each element
            // of the array is one line of the file.
            // prints current dir (where to put the file)
            Console.WriteLine(Directory.GetCurrentDirectory());
            string[] lines = System.IO.File.ReadAllLines(@"config.txt");

            // Display the file contents by using a foreach loop.
            List<Site> sites = new List<Site>();
            List<Process> processes = new List<Process>();
            String ordering = "";
            String routingPolicy = "";
            foreach (string line in lines)
            {
                String[] words;
                char[] delimiter = { ' ' };
                //Console.WriteLine(line);
                if (line.Contains("Site"))
                {
                    words = line.Split(delimiter);
                    sites.Add(new Site(words[1], words[3]));
                }
                else if (line.Contains("Process"))
                {
                    words = line.Split(delimiter);
                    processes.Add(new Process(words[1], words[3], words[5], words[7]));
                }
                else if (line.Contains("RoutingPolicy"))
                {
                    words = line.Split(delimiter);
                    routingPolicy = words[1];
                }
                else if (line.Contains("Ordering"))
                {
                    words = line.Split(delimiter);
                    ordering = words[1];

                }
                else
                {
                    //Console.WriteLine("unknown line at config or empty line" + line);
                }
            }
            Configurations configurations = new Configurations(routingPolicy, ordering, sites, processes);
            Console.WriteLine(configurations.ToString());

            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            System.Console.ReadKey();
        }
    }
}

