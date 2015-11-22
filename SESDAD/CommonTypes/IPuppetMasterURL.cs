using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IPuppetMasterURL
    {
        void Notify(String processName, String message);

        void CreateProcess(String type, String processName, String url, String[] brokersUrl, String site, String[] brokerNeighbours);

        void Log(String logMessage);
    }
}
