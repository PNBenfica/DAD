using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface ISubscriberInterface
    {
        void Subscribe();
        void UnSubscribe();
        void ReceiveMessage();
    }
}
