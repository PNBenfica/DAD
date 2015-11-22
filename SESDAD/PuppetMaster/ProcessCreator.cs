using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class ProcessCreator
    {

        public void startSubscriberProcess(String processName, String processUrl, String[] brokersUrl, String puppetMasterUrl, String logginglevel)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "" + processName + " " + processUrl + " " + brokersUrl[0] + " " + brokersUrl[1] + " " + brokersUrl[2] + " " + puppetMasterUrl + " " + logginglevel;
            Console.WriteLine(startInfo.Arguments);
            startInfo.FileName = @"Subscriber.exe";
            System.Diagnostics.Process.Start(startInfo);
        }

        public void startPublisherProcess(String processName, String processUrl, String[] brokersUrl, String puppetMasterUrl, String logginglevel)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "" + processName + " " + processUrl + " " + brokersUrl[0] + " " + brokersUrl[1] + " " + brokersUrl[2] + " " + puppetMasterUrl + " " + logginglevel;
            Console.WriteLine(startInfo.Arguments);
            startInfo.FileName = @"Publisher.exe";
            System.Diagnostics.Process.Start(startInfo);
        }

        public void startBrokerProcess(String processName, String processUrl, String[] brokersUrl, String[] neighbourBrokers, String router, String ordering, String puppetMasterUrl, String logginglevel, String site)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "" + processName + " " + processUrl + " " + brokersUrl[0] + " " + brokersUrl[1] + " " + brokersUrl[2] + " " + neighbourBrokers[0] + " " + neighbourBrokers[1] + " " + router + " " + ordering + " " + puppetMasterUrl + " " + logginglevel + " " + site;
            Console.WriteLine(startInfo.Arguments);
            startInfo.FileName = @"Broker.exe";
            System.Diagnostics.Process.Start(startInfo);
        }
    }
}
