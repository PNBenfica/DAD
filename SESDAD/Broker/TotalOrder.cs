using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Threading;

namespace Broker
{
    class TotalOrder : OrderStrategy
    {
        public int SequenceID { get; set; }
        public int ReceivedEvents { get; set; }
        public List<Event> SentEvents { get; set; }
        public List<Event> QueuedEvents { get; set; }

        public const int UNDEFINEDID = -1;

        public TotalOrder(Broker broker) 
            : base(broker) 
        {
            this.SequenceID = 0;
            this.ReceivedEvents = 0;
            this.QueuedEvents = new List<Event>();
            this.SentEvents = new List<Event>();
        }

        public override void DeliverInOrder(Event e)
        {
            if (e.Id == UNDEFINEDID)
            {
                lock (this)
                    RouteEvent(e);
            }

            else if (IsInOrder(e))
            {
                lock (this) { 
                    ReceivedEvents++;
                    RouteEvent(e);
                }
                ResendQueuedEvents();
            }
            else
            {
                QueuedEvents.Add(e);
            }
        }


        private void RouteEvent(Event e)
        {
            e.Id = ++SequenceID;
            e.PreviousEvents = new List<Event>(SentEvents.Skip(SentEvents.Count - 10).Take(10));
            SentEvents.Add(new Event(e.Id, e.PublisherId, e.Topic));
            if (Broker.isPrimaryBroker)
            {
                Broker.Log(e);
                Broker.Router.route(e);
            }
        }


        /// <summary>
        /// The current message is in order if :
        /// The id is the numPosts + 1
        /// Or if there aren't subscribed incoming messsages (the posts missing are from non subscribed topics)
        /// </summary>
        private bool IsInOrder(Event e)
        {
            return (e.Id == ReceivedEvents + 1) || !HaveMessageIncoming(e);
        }


        private bool HaveMessageIncoming(Event e)
        {
            foreach (Event previousEvent in e.PreviousEvents)
            {
                bool wasSentBeforeNewEvent = previousEvent.Id < e.Id; // sent before the new event that arrived
                bool wasSentAfterLastEvent = previousEvent.Id > ReceivedEvents; // sent after last event recorded
                bool isSubscribed = Broker.Router.HasSubscrition(previousEvent.Topic);

                if (wasSentBeforeNewEvent && wasSentAfterLastEvent && isSubscribed && Broker.HasSentEvent(e, Broker.SiteName))
                    return true;
            }
            return false;
        }


        private void OrderQueuedEvents()
        {
            QueuedEvents = QueuedEvents.OrderBy(o => o.Id).ToList();
        }


        /// <summary>
        /// If there are waiting events lets resend the one with lower id
        /// </summary>
        private void ResendQueuedEvents()
        {
            OrderQueuedEvents();
            if (QueuedEvents.Count > 0)
            {
                Event nextEvent = QueuedEvents[0];
                QueuedEvents.Remove(nextEvent);
                DeliverInOrder(nextEvent);
            }
        }

    }
}
