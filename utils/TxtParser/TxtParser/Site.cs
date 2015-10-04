using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TxtParser
{
    public class Site
    {

        private String siteName;
        private String parent;

        public Site(String siteName, String parent)
        {
            this.siteName = siteName;
            this.parent = parent;
        }

        public String SiteName
        {
            get { return siteName; }
            set { siteName = value; }
        }


        public String Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public override String ToString()
        {
            return "name: " + siteName + " parent: " + parent + "\r\n";
        }

    }
}
