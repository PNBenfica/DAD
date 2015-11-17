using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IBroker
    {
        DateTime Subscribe(String name, bool isSubscriber, String topic, bool isClimbing = false);
        bool IsParentInterested(String topic);
        void notifyChildrenOfSubscription(String name, String topic, bool isClimbing = false);
        void UnSubscribe(String name, bool isSubscriber, String topic);
        void DiffuseMessage(Event even);
        DateTime DiffuseMessageToRoot(Event even);
        DateTime Publish(Event e);
        void registerNewChild(string name, string url);
        void registerPublisher(string url);
        void registerSubscriber(string name, string url);
        void Status();
        void Freeze();
        void Unfreeze();
        void Crash();        
    }
}
