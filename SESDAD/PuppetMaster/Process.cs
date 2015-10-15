using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMaster
{
    public class Process
    {
        private String name;
        private String type;
        private String siteName;
        private String url;
        private String brokerUrl;

       

        public Process(String name, String type, String site, String url)
        {
            this.name = name;
            this.type = type;
            this.siteName = site;
            this.url = url;
        }

        public String BrokerUrl
        {
            get { return brokerUrl; }
            set { brokerUrl = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }


        public String Type
        {
            get { return type; }
            set { type = value; }
        }


        public String Site
        {
            get { return siteName; }
            set { siteName = value; }
        }


        public String Url
        {
            get { return url; }
            set { url = value; }
        }

        public override String ToString()
        {
            return "name: " + name + " type: " + type + " siteName: " + siteName + " Url: " + url + "\r\n";
        }

    }
}
