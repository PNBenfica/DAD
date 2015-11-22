using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class FileParser
    {

        public Configurations parse(String configPath, String puppetMastersPath)
        {

            string[] configLines = System.IO.File.ReadAllLines(configPath);
            string[] puppetMastersLines = System.IO.File.ReadAllLines(puppetMastersPath);

            // Display the file contents by using a foreach loop.
            List<Site> sites = new List<Site>();
            List<Process> processes = new List<Process>();
            String centralPuppetMasterURL = "";
            List<String> puppetMastersURL = new List<String>();
            String ordering = "";
            String routingPolicy = "";
            String loggingLevel = "";
            char[] delimiter = { ' ' };
            String[] words;

            //saving puppetMasters URL 
            foreach (string line in puppetMastersLines)
            {
                words = line.Split(delimiter);
                if (words[0].ToLower().Equals("centralpuppetmaster"))
                {
                    centralPuppetMasterURL = words[1];
                }
                else if (words[0].ToLower().Equals("puppetmaster"))
                {
                    puppetMastersURL.Add(words[1]);
                }
            }

            //saving config 
            foreach (string line in configLines)
            {
                words = line.Split(delimiter);

                if (words[0].ToLower().Equals("site"))
                {
                    //site and site parent
                    sites.Add(new Site(words[1], words[3]));
                }
                else if (words[0].ToLower().Equals("process"))
                {
                    //processName, processType, site, processUrl
                    Console.WriteLine("adding process " +words[1]);
                    processes.Add(new Process(words[1], words[3], words[5], words[7]));
                }
                else if (words[0].ToLower().Equals("routingpolicy"))
                {
                    routingPolicy = words[1];
                }
                else if (words[0].ToLower().Equals("ordering"))
                {
                    ordering = words[1];

                }
                else if (words[0].ToLower().Equals("logginglevel"))
                {
                    loggingLevel = words[1];

                }
                else
                {
                    //Console.WriteLine("unknown line at config or empty line" + line);
                }
            }

            if (ordering.Equals(""))
            {
                ordering = "fifo";
            }

            if (routingPolicy.Equals(""))
            {
                routingPolicy = "flooding";
            }

            if (loggingLevel.Equals(""))
            {
                loggingLevel = "light";
            }

            //get brokers parents if exist && publishers for subscribers
            foreach (Process process in processes)
            {
                if (process.Type.Equals("broker"))
                {
                    process.BrokersUrl = getBrokerParentUrl(process.Site, sites, processes);
                    process.NeighbourBrokers = getBrokersOfSite(process.Site,process.Name, processes);
                }
                else
                {
                    process.BrokersUrl = getBrokerUrl(process.Site, processes);
                }
            }

            Configurations configurations = new Configurations(routingPolicy, ordering, centralPuppetMasterURL, puppetMastersURL, sites, processes, loggingLevel);
            return configurations;
        }

        private String[] getBrokersOfSite(String site, String processName, List<Process> processes)
        {
            int i = 0;
            String[] siteBrokersUrl = new String[2];
            foreach (Process p in processes)
            {
                if (!p.Name.Equals(processName) && p.Site.Equals(site))
                {
                    siteBrokersUrl[i] = p.Url;
                    i++;
                }
                if (i == 2)
                {
                    return siteBrokersUrl;
                }
            }
            return siteBrokersUrl;
        }

        private String[] getBrokerParentUrl(string siteName, List<Site> sites, List<Process> processes)
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
            if (parentSite.Equals("none"))
            {
                String[] parents = {"none", "none", "none"};
                return parents;
            }
            return getBrokerUrl(parentSite, processes);
        }

        private String[] getBrokerUrl(string siteName, List<Process> processes)
        {
            String[] brokersUrl = new String[3];
            int i = 0;
            foreach (Process process in processes)
            {
                if(!process.Type.Equals("broker"))
                {
                    continue;
                }
                else if (process.Site.Equals(siteName))
                {
                    brokersUrl[i] = process.Url;
                    i++;
                    if(i == 3)
                        break;
                }
            }
            return brokersUrl;
        }
    }
}
