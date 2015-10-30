using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Subscriber
{
    class FifoOrder: OrderStrategy
    {

        public Dictionary<string, int> PublishersPosts { get; set; }
        public HashSet<string> HasPublisherInfo { get; set; } // if a publisher name is here this subscriber already received a message from him
        public Dictionary<string, List<Event>> QueuedEvents { get; set; }


        public FifoOrder(Subscriber subscriber) 
            : base(subscriber)
        {
            this.PublishersPosts = new Dictionary<String, int>();
            this.QueuedEvents = new Dictionary<string, List<Event>>();
            this.HasPublisherInfo = new HashSet<string>();
        }


        public override void DeliverMessage(Event e)
        {
            if (IsInOrder(e))
            {
                UpdatePublisherPost(e);
                Subscriber.PrintMessage(e);
                ResendQueuedEvents(e);
            }
            else
            {
                AddEventToQueue(e);
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
                if (previousEvent.Id < e.Id && previousEvent.Id > GetNumPublisherPost(e.PublisherId) && Subscriber.HasSubscrition(previousEvent.Topic))
                    return true;
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

            PreventMessageDeadLock(e);
        }


        /// <summary>
        /// This happens for example when: Publisher publisher post Id 1
        /// The subscriber subscribes after. Publisher post Id 2. Subscriber waits for Id 1
        /// To prevent that after a defined set of time, the message will all messages will be sent
        /// </summary>
        private void PreventMessageDeadLock(Event e)
        {
            if (!HasPublisherInfo.Contains(e.PublisherId)) // if we dont know who this guy is, lets wait a little
            {
                HasPublisherInfo.Add(e.PublisherId);
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((obj) =>
                        {
                            lock (this)
                            {
                                List<Event> queuedEvents = GetQueuedEvents(e.PublisherId);
                                if (queuedEvents != null && queuedEvents.Count > 0)
                                {
                                    UpdatePublisherPost(queuedEvents[0]);
                                    ResendQueuedEvents(e);
                                }
                            }
                            timer.Dispose();
                        },
                    null, 3000, System.Threading.Timeout.Infinite);

            }
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
        private void ResendQueuedEvents(Event e)
        {
            List<Event> queuedEvents = GetQueuedEvents(e.PublisherId);
            if (queuedEvents != null && queuedEvents.Count > 0)
            {
                Event nextEvent = queuedEvents[0];
                RemoveEventFromQueue(nextEvent);
                DeliverMessage(nextEvent);
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
