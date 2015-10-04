using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxtParser
{
    public class Configurations
    {
        private String routingPolicy;
        private String ordering;
        private List<Site> sites;
        private List<Process> processes;

        public Configurations(String routingPolicy, String ordering, List<Site> sites, List<Process> processes)
        {
            this.routingPolicy = routingPolicy;
            this.ordering = ordering;
            this.sites = sites;
            this.processes = processes;
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


        internal List<Site> Sites
        {
            get { return sites; }
            set { sites = value; }
        }


        internal List<Process> Brokers
        {
            get { return processes; }
            set { processes = value; }
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
