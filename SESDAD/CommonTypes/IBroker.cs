using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IBroker
    {
        void Subscribe(Topic topic, String content);
        void UnSubscribe(String subscriberId, Topic topic);
        void DiffuseMessage(Event even);
        void DiffuseMessageToRoot(Event even);
        void ReceiveMessage(Topic topic, String content);
        void registerNewChild(string url);
        void registerPublisher(string url);
        void registerSubscriber(string url);
    }
}
