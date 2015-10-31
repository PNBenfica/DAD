using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface ISubscriber
    {
        void Subscribe(String topic);
        void UnSubscribe(String topic);
        void ReceiveMessage(Event evento);
        void Status();
        void Freeze();
        void Unfreeze();
        void Crash();
    }
}
