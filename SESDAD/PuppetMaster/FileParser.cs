using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class FileParser
    {

        public Configurations parse(String filePath)
        {

            string[] lines = System.IO.File.ReadAllLines(@filePath);

            // Display the file contents by using a foreach loop.
            List<Site> sites = new List<Site>();
            List<Process> processes = new List<Process>();
            String ordering = "";
            String routingPolicy = "";
            String loggingLevel = "";
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
                else if (line.Contains("LoggingLevel"))
                {
                    words = line.Split(delimiter);
                    loggingLevel = words[1];

                }
                     
                else
                {
                    //Console.WriteLine("unknown line at config or empty line" + line);
                }
            }

            Configurations configurations = new Configurations(routingPolicy, ordering, sites, processes, loggingLevel);
            return configurations;
        }
    }
}
