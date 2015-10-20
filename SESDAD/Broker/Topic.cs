using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker
{
    public class Topic
    {

        public string Name { get; set; }
        public Topic Parent { get; set; }
        private Dictionary<string, Topic> subTopics;
        public HashSet<String> Subscribers { get; set; }
        public HashSet<String> SubscribersAllSubTopics { get; set; } // Subscribers that subscribes an entire topic
        public HashSet<String> Brokers { get; set; }
        public HashSet<String> BrokersAllSubTopics { get; set; } // Brokers that subscribes an entire topic


        public Topic(String name, Topic parent = null)
        {
            this.Name = name;
            this.Parent = parent;
            this.subTopics = new Dictionary<string, Topic>();
            this.Subscribers = new HashSet<String>();
            this.SubscribersAllSubTopics = new HashSet<String>();
            this.Brokers = new HashSet<String>();
            this.BrokersAllSubTopics = new HashSet<String>();
        }

        /// <summary>
        /// Is this the root "/"
        /// </summary>
        public bool IsRoot()
        {
            return this.Parent == null;
        }


        /// <summary>
        /// Returns a subtopic (creates if it isn t there)
        /// </summary>
        public Topic GetSubTopic(string subTopic)
        {
            if (!subTopics.ContainsKey(subTopic))
                subTopics.Add(subTopic, new Topic(subTopic, this));
            return subTopics[subTopic];
        }

        /// <summary>
        /// Add a subscriber/broker subscrition
        /// </summary>
        private void addSubscrition(String name, bool isSubscriber)
        {
            if (isSubscriber)
                Subscribers.Add(name);
            else
                Brokers.Add(name);
        }


        /// <summary>
        /// Add a new subscrition
        /// </summary>
        /// <param name="name">Name of the broker/subscriber</param>
        /// <param name="isSubscriber">Type of the entitie interested</param>
        public void Subscribe(String name, String[] topic, bool isSubscriber)
        {
            if (topic.Length == 0)
            {
                addSubscrition(name, isSubscriber);
            }
            else if (topic.Length == 1 && topic[0].Equals("*")) // want to subscribe all
            {
                SubscribeAll(name, isSubscriber);
            }
            else
            {
                SubscribeSubtopic(name, topic, isSubscriber);
            }
        }


        /// <summary>
        /// Subscribes all the subtopics
        /// </summary>
        private void SubscribeAll(string name, bool isSubscriber)
        {
            if (isSubscriber)
                SubscribersAllSubTopics.Add(name);
            else
                BrokersAllSubTopics.Add(name);
        }


        /// <summary>
        /// Subscribes the subtopic
        /// </summary>
        /// <param name="topic"> ["subtopic", "sub-subtopic", ...] </param>
        private void SubscribeSubtopic(String name, String[] topic, bool isSubscriber)
        {
            Topic subTopic = GetSubTopic(topic[0]);
            String[] restSubTopics = (String[])topic.Skip(1).ToArray(); // removes first element
            subTopic.Subscribe(name, restSubTopics, isSubscriber);
        }
        

        public void Unsubscribe(string subscriberName, string[] topicSplit, bool isSubscriber)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Get the subscribers of a topic
        /// </summary>
        public List<String> GetSubscribers(String[] topic)
        {
            if (topic.Length == 0)
                return GetTopicSubscribers();
            else
            {
                Topic subTopic = GetSubTopic(topic[0]);
                String[] restSubTopics = (String[])topic.Skip(1).ToArray(); // removes first element
                return subTopic.GetSubscribers(restSubTopics);
            }
        }

        /// <summary>
        /// Get the brokers with a path that leads to a subscriber
        /// </summary>
        public List<String> GetBrokers(String[] topic)
        {
            if (topic.Length == 0)
                return GetTopicBrokers();

            else
            {
                Topic subTopic = GetSubTopic(topic[0]);
                String[] restSubTopics = (String[])topic.Skip(1).ToArray(); // removes first element
                return subTopic.GetBrokers(restSubTopics);
            }
        }


        /// <summary>
        /// returns true if there is any interested (subscriber/broker) in the event
        /// </summary>
        public bool HaveSubscribers(String topic)
        {
            return GetTopicSubscribers().Count() > 0 || GetTopicBrokers().Count > 0;
        }


        /// <summary>
        /// Get all of the subscribers interested in this topic:
        /// The ones who have specifically subscribe this topic (this.Subscribers)
        /// And the ones that have subscribe all topics ("/*") up in the tree
        /// </summary>
        private List<string> GetTopicSubscribers()
        {
            HashSet<String> subscribers = Parent.AllSubTopicsSubscribers();
            subscribers.UnionWith(Subscribers);
            return subscribers.ToList();
        }


        /// <summary>
        /// Get the subscribers that have subscribe all topics
        /// from this broker until the root
        /// </summary>
        private HashSet<String> AllSubTopicsSubscribers()
        {
            HashSet<String> subscribers = new HashSet<String>(SubscribersAllSubTopics);
            if (!IsRoot())
                subscribers.UnionWith(Parent.AllSubTopicsSubscribers());
            return subscribers;
        }


        /// <summary>
        /// Get all of the brokers interested in this topic:
        /// The ones who have specifically subscribe this topic (this.Brokers)
        /// And the ones that have subscribe all topics ("/*") up in the tree
        /// </summary>
        private List<string> GetTopicBrokers()
        {
            HashSet<String> brokers = Parent.AllSubTopicsBrokers();
            brokers.UnionWith(Brokers);
            return brokers.ToList();
        }

        /// <summary>
        /// Get the brokers that have subscribe all topics
        /// from this broker until the root
        /// </summary>
        private HashSet<String> AllSubTopicsBrokers()
        {
            HashSet<String> brokers = new HashSet<String>(BrokersAllSubTopics);
            if (!IsRoot())
                brokers.UnionWith(Parent.AllSubTopicsSubscribers());
            return brokers;
        }

        /// <summary>
        /// just to Debug
        /// </summary>
        public void Status()
        {
            Console.WriteLine("Topic: {0}", this.Name);
            PrintInfo("Subscribers", Subscribers);
            PrintInfo("Subscribers all subtopics", SubscribersAllSubTopics);
            PrintInfo("Brokers", Brokers);
            PrintInfo("Brokers all subtopics", BrokersAllSubTopics);

            foreach (Topic subtopic in subTopics.Values)
                subtopic.Status();
            Console.WriteLine("");
        }

        private void PrintInfo(String description, HashSet<String> collection)
        {
            if (collection.Count() > 0)
            {
                Console.WriteLine(description);
                foreach (String s in collection)
                {
                    Console.WriteLine("-> {0}", s);
                }
            }
        }
    }
}
