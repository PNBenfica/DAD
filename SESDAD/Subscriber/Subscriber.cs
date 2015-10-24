using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;


namespace Subscriber
{
    class Subscriber : MarshalByRefObject, ISubscriber
    {
        private String name;
        private String url;
        private IBroker broker;
        private bool isOrdering;
        public Dictionary<string, int> PublishersPosts { get; set; }
        public Dictionary<string, List<Event>> QueuedEvents { get; set; }
        public Topic Subscriptions { get; set; }


        /// <summary>
        /// Subscriber Construtor
        /// </summary>
        /// <param name="name">subscriber name</param>
        public Subscriber(String name, String url, String ordering)
        {
            this.name = name;
            this.url = url;
            this.isOrdering = ordering.Equals("FIFO");
            this.PublishersPosts = new Dictionary<String, int>();
            this.QueuedEvents = new Dictionary<string, List<Event>>();
            this.Subscriptions = new Topic("/");
        }

        public void Subscribe(string topic)
        {
            Console.WriteLine("New Subscrition on Topic: {0}", topic);
            Subscriptions.Subscribe(name, tokenize(topic), true);
            broker.Subscribe(this.name, true, topic);
        }

        public void UnSubscribe(string topic)
        {
            Console.WriteLine("Unsubscrition on Topic: {0}", topic);
            Subscriptions.Unsubscribe(name, tokenize(topic), true);
            broker.UnSubscribe(this.name, true, topic);
        }


        /// <summary>
        /// This method is called when a new event arrives from the broker
        /// </summary>
        /// <param name="e"></param>
        public void ReceiveMessage(Event e)
        {
            if (isOrdering){
                if (IsInOrder(e))
                {
                    UpdatePublisherPost(e);
                    PrintMessage(e);
                    ResendQueuedEvents(e);
                }
                else
                {
                    AddEventToQueue(e);
                }
            }
            else
                PrintMessage(e);
        }


        private void PrintMessage(Event e)
        {
            Console.WriteLine("");
            Console.WriteLine("------- New Message -------");
            Console.WriteLine("Post ID:" + e.Id);
            Console.WriteLine("Publisher: {0}\r\nTopic: {1}\r\nContent: {2}", e.PublisherId, e.Topic, e.Content);
        }


        /// <summary>
        /// The current message is in order if :
        /// The id is the (current num of posts of publisher) + 1
        /// Or if there aren't subscribed incoming messsages (the posts missing are from non subscribed topics)
        /// </summary>
        private bool IsInOrder(Event e)
        {
            return (e.Id == GetNumPublisherPost(e.PublisherId) + 1) || !HaveMessageIncoming(e) ;
        }


        /// <summary>
        /// Check if there is some late message sent that havent arrived and it is subscripted
        /// </summary>
        private bool HaveMessageIncoming(Event e)
        {
            foreach (Event previousEvent in e.PreviousEvents)
            {
                if (previousEvent.Id < e.Id && previousEvent.Id > GetNumPublisherPost(e.PublisherId) && HasSubscrition(previousEvent.Topic))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Add an event to the queue of events waiting for other publisher message
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
        private void ResendQueuedEvents(Event e)
        {
            List<Event> queuedEvents = GetQueuedEvents(e.PublisherId);
            if (queuedEvents != null && queuedEvents.Count > 0)
            {
                Event nextEvent = queuedEvents[0];
                RemoveEventFromQueue(nextEvent);
                ReceiveMessage(nextEvent);
            }
        }


        /// <summary>
        /// returns true if the subscriber has a subscrition in the topic
        /// </summary>
        private bool HasSubscrition(string topic)
        {
            return Subscriptions.HasSubscrition(name, tokenize(topic));
        }


        private void UpdatePublisherPost(Event e)
        {
            PublishersPosts[e.PublisherId] = e.Id;
        }


        private int GetNumPublisherPost(string publisher)
        {
            if (!PublishersPosts.ContainsKey(publisher))
                PublishersPosts[publisher] = 0;
            return PublishersPosts[publisher];
        }


        /// <summary>
        /// notify site broker to add this new subscriber
        /// </summary>
        internal void registerInBroker(string brokerUrl)
        {
            Console.WriteLine("Registing in broker at {0}", brokerUrl);
            this.broker = (IBroker)Activator.GetObject(typeof(IBroker), brokerUrl);
            this.broker.registerSubscriber(this.name, this.url);
        }


        /// <summary>
        /// Divides the string topic into String[]
        /// </summary>
        public String[] tokenize(String topic)
        {
            char[] delimiterChars = { '/' };
            string[] topicSplit = topic.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            return topicSplit;
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


        public void Status()
        {
            Subscriptions.Status();
        }
    }
}
