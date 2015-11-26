using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IBroker
    {
        void Subscribe(String name, bool isSubscriber, String topic, bool isClimbing = false);
        bool IsParentInterested(String topic);
        string PrimaryBrokerUrl();
        void notifyChildrenOfSubscription(String name, String topic, bool isClimbing = false);
        void UnSubscribe(String name, bool isSubscriber, String topic);
        void DiffuseMessage(Event even);
        void DiffuseMessageToRoot(Event even);
        void Publish(Event e);

        void ReceiveImAlive();
        void IsAlive();
        void SentEventNotification(Event e);

        void registerNewChildSite(string name, string primaryBroker, string secondaryBroker1, string secondaryBroker2);
        void registerPublisher(string url);
        void registerSubscriber(string name, string url);
        void Status();
        void Freeze();
        void Unfreeze();
        void Crash();        
    }
}
