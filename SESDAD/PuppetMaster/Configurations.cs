﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMaster
{
    public class Configurations
    {
        private String routingPolicy;
        private String ordering;
        List<String> puppetMastersUrl;
        String centralPuppetMasterUrl;
        String puppetMasterUrl;
        private List<Site> sites;
        private List<Process> processes;
        private String loggingLevel;

        public Configurations(String createdPuppetMaster, String routingPolicy, String ordering, String centralPuppetMasterUrl, List<String> puppetMastersUrl, List<Site> sites, List<Process> processes, String loggingLevel)
        {
            this.puppetMasterUrl = createdPuppetMaster;
            this.routingPolicy = routingPolicy;
            this.ordering = ordering;
            this.centralPuppetMasterUrl = centralPuppetMasterUrl;
            this.puppetMastersUrl = puppetMastersUrl;
            this.sites = sites;
            this.processes = processes;
            this.loggingLevel = loggingLevel;
        }

        public String CentralPuppetMasterUrl
        {
            get { return centralPuppetMasterUrl; }
            set { centralPuppetMasterUrl = value; }
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

        public String PuppetMasterUrl
        {
            get { return puppetMasterUrl; }
            set { puppetMasterUrl = value; }
        }


        public List<String> PuppetMastersUrl
        {
            get { return puppetMastersUrl; }
            set { puppetMastersUrl = value; }
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
            info += "PuppetMasterURL: " + centralPuppetMasterUrl + "\r\n";
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
