using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMaster
{
    public class Configurations
    {
        private String routingPolicy;
        private String ordering;
        private List<Site> sites;
        private List<Process> processes;
        private String loggingLevel;

        public Configurations(String routingPolicy, String ordering, List<Site> sites, List<Process> processes, String loggingLevel)
        {
            this.routingPolicy = routingPolicy;
            this.ordering = ordering;
            this.sites = sites;
            this.processes = processes;
            this.loggingLevel = loggingLevel;
        }

        public String RoutingPolicy
        {
            get { return routingPolicy; }
            set { routingPolicy = value; }
        }


        public String Ordering
        {
            get { return ordering; }
            set { ordering = value; }
        }


        public List<Site> Sites
        {
            get { return sites; }
            set { sites = value; }
        }


        public List<Process> Processes
        {
            get { return processes; }
            set { processes = value; }
        }

        public String LoggingLevel
        {
            get { return loggingLevel; }
            set { loggingLevel = value; }
        }

        public override String ToString()
        {
            String info = "Config Content:\r\n";
            foreach (Site site in sites)
            {
                info += site.ToString();
            }
            foreach (Process process in processes)
            {
                info += process.ToString();
            }
            info += "RoutingPolicy: " + routingPolicy + "\r\n";
            info += "Ordering: " + ordering + "\r\n";
            return info;
        }
    }
    
}
