using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IPuppetMasterURL
    {
        void notify(String processName, String message);

        void createProcess(String type, String processName, String url, String brokerUrl);
    }
}
