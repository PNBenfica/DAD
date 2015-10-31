using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IPublisher
    {
        void Publish(String topic, String content);
        void SequencePublish(String numberOfEvents, String topic, String waitXms);
        void Status();
        void Freeze();
        void Unfreeze();
        void Crash();
    }
}
