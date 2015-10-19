using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IBroker
    {
        void Subscribe(String Id, bool isSubscriber, String topic);
        void UnSubscribe(String Id, bool isSubscriber, String topic);
        void DiffuseMessage(Event even);
        void DiffuseMessageToRoot(Event even);
        void registerNewChild(string name, string url);
        void registerPublisher(string url);
        void registerSubscriber(string name, string url);
        void Status();
        
    }
}
