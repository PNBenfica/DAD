using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IBroker
    {
        void Subscribe(String subscriberId, bool client_broker, Topic topic);
        void UnSubscribe(String subscriberId, bool client_broker, Topic topic);
        void DiffuseMessage(Event even);
        void DiffuseMessageToRoot(Event even);
        void ReceiveMessage(Topic topic, String content);
        void registerNewChild(string url);
        void registerPublisher(string url);
        void registerSubscriber(string url);
        string Name { get; set; }
    }
}
