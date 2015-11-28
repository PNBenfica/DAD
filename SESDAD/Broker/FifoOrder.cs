using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Threading;

namespace Broker
{
    class FifoOrder: OrderStrategy
    {

        public Dictionary<string, int> PublishersPosts { get; set; }
        public Dictionary<string, List<Event>> QueuedEvents { get; set; } //queued events of a publisher
        public Dictionary<string, Object> PublisherLocks { get; set; }


        public FifoOrder(Broker broker) 
            : base(broker)
        {
            this.PublishersPosts = new Dictionary<String, int>();
            this.QueuedEvents = new Dictionary<string, List<Event>>();
            this.PublisherLocks = new Dictionary<string, Object>(); 
        }


        public override void DeliverInOrder(Event e)
        {
            if (IsInOrder(e))
            {
                UpdatePublisherPost(e);
                if (Broker.isPrimaryBroker)
                    RouteEvent(e);
                ResendQueuedEvents(e.PublisherId);
            }
            else
            {
                AddEventToQueue(e);
            }
        }

        private void RouteEvent(Event e)
        {
            if (!PublisherLocks.ContainsKey(e.PublisherId))
                PublisherLocks[e.PublisherId] = new Object();

            lock (PublisherLocks[e.PublisherId])
            {
                Broker.Log(e);
                Broker.Router.route(e);
            }
        }


        /// <summary>
        /// The current message is in order if :
        /// The id is the (current num of posts of publisher) + 1
        /// Or if there aren't subscribed incoming messsages (the posts missing are from non subscribed topics)
        /// </summary>
        private bool IsInOrder(Event e)
        {
            return (e.Id == GetNumPublisherPost(e.PublisherId) + 1) || !HaveMessageIncoming(e);
        }


        /// <summary>
        /// Check if there is some late message sent that havent arrived and it is subscripted
        /// </summary>
        private bool HaveMessageIncoming(Event e)
        {
            foreach (Event previousEvent in e.PreviousEvents)
            {
                bool wasSentBeforeNewEvent = previousEvent.Id < e.Id; // sent before the new event that arrived
                bool wasSentAfterLastEvent = previousEvent.Id > GetNumPublisherPost(e.PublisherId); // sent after last event recorded of this publisher
                bool isSubscribed = Broker.Router.HasSubscrition(previousEvent.Topic);
                //bool subscribedBeforePublish = previousEvent.TimeStamp.CompareTo(Broker.TopicTimeStamp(previousEvent.Topic)) > 0;

                if (wasSentBeforeNewEvent && wasSentAfterLastEvent && isSubscribed && Broker.HasSentEvent(e, Broker.SiteName, false))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Add an event to the queue of events waiting for other publisher message
        /// It resend the message when an incoming message have arrived
        /// Or if no message arrives in X seconds
        /// </summary>
        private void AddEventToQueue(Event e)
        {
            if (!QueuedEvents.ContainsKey(e.PublisherId))
            {
                QueuedEvents[e.PublisherId] = new List<Event>();
            }
            QueuedEvents[e.PublisherId].Add(e);
        }
        

        /// <summary>
        /// Remove an event from the queue of events waiting for other publisher message
        /// </summary>
        private void RemoveEventFromQueue(Event e)
        {
            QueuedEvents[e.PublisherId].Remove(e);
        }


        /// <summary>
        /// returns the events queued that are waiting ordered by Id of the messages
        /// </summary>
        private List<Event> GetQueuedEvents(string publisher)
        {
            if (QueuedEvents.ContainsKey(publisher))
            {
                QueuedEvents[publisher] = QueuedEvents[publisher].OrderBy(o => o.Id).ToList();
                return QueuedEvents[publisher];
            }
            return null;
        }


        /// <summary>
        /// If there are waiting events from a publisher
        /// lets resend the one with lower id
        /// </summary>
        private void ResendQueuedEvents(string publisher)
        {
            List<Event> queuedEvents = GetQueuedEvents(publisher);
            if (queuedEvents != null && queuedEvents.Count > 0)
            {
                Event nextEvent = queuedEvents[0];
                RemoveEventFromQueue(nextEvent);
                DeliverInOrder(nextEvent);
            }
        }


        /// <summary>
        /// Returns the num of posts recorded in this subscriber by a publisher
        /// </summary>
        private int GetNumPublisherPost(string publisher)
        {
            if (!PublishersPosts.ContainsKey(publisher))
                PublishersPosts[publisher] = 0;
            return PublishersPosts[publisher];
        }


        private void UpdatePublisherPost(Event e)
        {
            PublishersPosts[e.PublisherId] = e.Id;
        }


        private void PrintMessagesQueue()
        {
            Console.WriteLine("Message queue");
            foreach (List<Event> queue in QueuedEvents.Values)
            {
                foreach (Event e in queue)
                {
                    Console.WriteLine("" + e.Id);
                }
            }
        }
    }
}
