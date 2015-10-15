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
                    //site and site parent
                    sites.Add(new Site(words[1], words[3]));
                }
                else if (line.Contains("Process"))
                {
                    words = line.Split(delimiter);
                    //processName, processType, site, processUrl
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

            //get brokers parents if exist && publishers for subscribers
            foreach (Process process in processes)
            {
                if (process.Type.Equals("broker"))
                {
                    process.BrokerUrl = getBrokerParentUrl(process.Site, sites, processes);
                }
                else
                {
                    process.BrokerUrl = getBrokerUrl(process.Site, processes);
                }
            }

            Configurations configurations = new Configurations(routingPolicy, ordering, sites, processes, loggingLevel);
            return configurations;
        }

        private string getBrokerParentUrl(string siteName, List<Site> sites, List<Process> processes)
        {
            String parentSite = "";
            foreach(Site site in sites)
            {
                if (siteName.Equals(site.SiteName))
                {
                    parentSite = site.Parent;
                    break;
                }
            }

            return getBrokerUrl(parentSite, processes);
        }

        private String getBrokerUrl(string siteName, List<Process> processes)
        {
            String brokerUrl = ""; 
            foreach (Process process in processes)
            {
                if(!process.Type.Equals("broker"))
                {
                    continue;
                }
                else if (process.Site.Equals(siteName))
                {
                    brokerUrl = process.Url;
                    break;
                }
            }
            return brokerUrl;
        }
    }
}
