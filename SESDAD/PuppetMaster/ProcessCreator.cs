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

        public void startSubscriberProcess(String processName, String processUrl, String brokerUrl, String ordering, String puppetMasterUrl, String logginglevel)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "" + processName + " " + processUrl + " " + brokerUrl + " " + ordering + " " + puppetMasterUrl + " " + logginglevel;
            Console.WriteLine(startInfo.Arguments);
            startInfo.FileName = @"Subscriber.exe";
            System.Diagnostics.Process.Start(startInfo);
        }

        public void startPublisherProcess(String processName, String processUrl, String brokerUrl, String puppetMasterUrl, String logginglevel)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "" + processName + " " + processUrl + " " + brokerUrl + " " + puppetMasterUrl + " " + logginglevel;
            Console.WriteLine(startInfo.Arguments);
            startInfo.FileName = @"Publisher.exe";
            System.Diagnostics.Process.Start(startInfo);
        }

        public void startBrokerProcess(String processName, String processUrl, String brokerUrl, String[] neighbourBrokers, String router, String puppetMasterUrl, String logginglevel)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = "" + processName + " " + processUrl + " " + brokerUrl + " " + neighbourBrokers[0] + " " + neighbourBrokers[1] + " " + router + " " + puppetMasterUrl + " " + logginglevel;
            Console.WriteLine(startInfo.Arguments);
            startInfo.FileName = @"Broker.exe";
            System.Diagnostics.Process.Start(startInfo);
        }
    }
}
