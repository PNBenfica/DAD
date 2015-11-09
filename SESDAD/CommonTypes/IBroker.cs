using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IBroker
    {
        DateTime Subscribe(String Id, bool isSubscriber, String topic);
        void UnSubscribe(String Id, bool isSubscriber, String topic);
        void DiffuseMessage(Event even);
        DateTime DiffuseMessageToRoot(Event even);
        void registerNewChild(string name, string url);
        void registerPublisher(string url);
        void registerSubscriber(string name, string url);
        bool ParentHaveSubscription(Event even);
        void Status();
        void Freeze();
        void Unfreeze();
        void Crash();
        
    }
}
