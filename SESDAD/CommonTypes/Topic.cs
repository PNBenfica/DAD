using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public class Topic<T>
    {

        public string Name { get; set; }
        public Topic<T> Parent { get; set; }
        private Dictionary<string, Topic<T>> subTopics;
        public HashSet<T> Subscribers { get; set; }
        public HashSet<T> SubscribersAllSubTopics { get; set; } // Subscribers that subscribes an entire topic
        public HashSet<T> Brokers { get; set; }
        public HashSet<T> BrokersAllSubTopics { get; set; } // Brokers that subscribes an entire topic


        public Topic(String name, Topic<T> parent = null)
        {
            this.Name = name;
            this.Parent = parent;
            this.subTopics = new Dictionary<string, Topic<T>>();
            this.Subscribers = new HashSet<T>();
            this.SubscribersAllSubTopics = new HashSet<T>();
            this.Brokers = new HashSet<T>();
            this.BrokersAllSubTopics = new HashSet<T>();
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
        public Topic<T> GetSubTopic(string subTopic)
        {
            lock (this)
            {
                if (!subTopics.ContainsKey(subTopic))
                    subTopics.Add(subTopic, new Topic<T>(subTopic, this));
            }
            return subTopics[subTopic];
        }

        /// <summary>
        /// Add a subscriber/broker subscrition
        /// </summary>
        private void addSubscrition(T name, bool isSubscriber)
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
        public void Subscribe(T name, String[] topicArray, bool isSubscriber)
        {
            if (topicArray.Length == 0)
            {
                addSubscrition(name, isSubscriber);
            }
            else if (topicArray.Length == 1 && topicArray[0].Equals("*")) // want to subscribe all
            {
                SubscribeAll(name, isSubscriber);
            }
            else
            {
                SubscribeSubtopic(name, topicArray, isSubscriber);
            }
        }

        public void Unsubscribe(T name, string[] topicArray, bool isSubscriber)
        {
            if (topicArray.Length == 0)
            {
                removeSubscrition(name, isSubscriber);
            }
            else if (topicArray.Length == 1 && topicArray[0].Equals("*")) // want to subscribe all
            {
                UnSubscribeAll(name, isSubscriber);
            }
            else
            {
                UnSubscribeSubtopic(name, topicArray, isSubscriber);
            }
        }

        private void UnSubscribeSubtopic(T name, string[] topicArray, bool isSubscriber)
        {
            throw new NotImplementedException();
        }

        private void removeSubscrition(T name, bool isSubscriber)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Subscribes all the subtopics
        /// </summary>
        private void SubscribeAll(T name, bool isSubscriber)
        {
            //if already exist a subscribe all in the tree
            if (checkAlreadySubscribeAll(name, isSubscriber))
            {
                return;
            }

            if (isSubscriber)
                SubscribersAllSubTopics.Add(name);
            else
                BrokersAllSubTopics.Add(name);

            clearSubTopicsSubscribeAll(name, isSubscriber, true);
        }

        private void UnSubscribeAll(T name, bool isSubscriber)
        {
            if (isSubscriber)
                SubscribersAllSubTopics.Remove(name);
            else
                BrokersAllSubTopics.Remove(name);
        }

        public void clearSubTopicsSubscribeAll(T name, bool isSubscriber, bool isParentTopic)
        {
            if (!isParentTopic)
            {
                if (isSubscriber)
                {
                    SubscribersAllSubTopics.Remove(name);
                }
                else
                {
                    BrokersAllSubTopics.Remove(name);
                }
            }

            foreach (KeyValuePair<string, Topic<T>> entry in subTopics)
            {
                entry.Value.clearSubTopicsSubscribeAll(name, isSubscriber, false);
            }
        }


        public bool checkAlreadySubscribeAll(T name, bool isSubscriber)
        {
            if (isSubscriber && SubscribersAllSubTopics.Contains(name))
            {
                return true;
            }
            else if (!isSubscriber && BrokersAllSubTopics.Contains(name))
            {
                return true;
            }
            else if (!BrokersAllSubTopics.Contains(name) && !SubscribersAllSubTopics.Contains(name) && IsRoot())
            {
                return false;
            }
            else
            {
                return Parent.checkAlreadySubscribeAll(name, isSubscriber);
            }
        }


        /// <summary>
        /// Subscribes the subtopic
        /// </summary>
        /// <param name="topic"> ["subtopic", "sub-subtopic", ...] </param>
        private void SubscribeSubtopic(T name, String[] topic, bool isSubscriber)
        {
            Topic<T> subTopic = GetSubTopic(topic[0]);
            String[] restSubTopics = (String[])topic.Skip(1).ToArray(); // removes first element
            subTopic.Subscribe(name, restSubTopics, isSubscriber);
        }

        /// <summary>
        /// Get all the subscribers of a topic
        /// </summary>
        public List<T> GetSubscribers(String[] topic)
        {
            if (topic.Length == 0)
                return GetTopicSubscribers();
            else
            {
                Topic<T> subTopic = GetSubTopic(topic[0]);
                String[] restSubTopics = (String[])topic.Skip(1).ToArray(); // removes first element
                return subTopic.GetSubscribers(restSubTopics);
            }
        }

        /// <summary>
        /// Get all the brokers with a path that leads to a subscriber
        /// </summary>
        public List<T> GetBrokers(String[] topic)
        {
            if (topic.Length == 0)
                return GetTopicBrokers();

            else
            {
                Topic<T> subTopic = GetSubTopic(topic[0]);
                String[] restSubTopics = (String[])topic.Skip(1).ToArray(); // removes first element
                return subTopic.GetBrokers(restSubTopics);
            }
        }


        /// <summary>
        /// Get all of the subscribers interested in this topic:
        /// The ones who have specifically subscribe this topic (this.Subscribers)
        /// And the ones that have subscribe all topics ("/*") up in the tree
        /// </summary>
        private List<T> GetTopicSubscribers()
        {
            HashSet<T> subscribers = AllSubTopicsSubscribers();
            subscribers.UnionWith(Subscribers);
            return subscribers.ToList();
        }


        /// <summary>
        /// Get the subscribers that have subscribe all topics
        /// from this broker until the root
        /// </summary>
        private HashSet<T> AllSubTopicsSubscribers()
        {
            HashSet<T> subscribers = new HashSet<T>(SubscribersAllSubTopics);
            if (!IsRoot())
                subscribers.UnionWith(Parent.AllSubTopicsSubscribers());
            return subscribers;
        }


        /// <summary>
        /// Get all of the brokers interested in this topic:
        /// The ones who have specifically subscribe this topic (this.Brokers)
        /// And the ones that have subscribe all topics ("/*") up in the tree
        /// </summary>
        private List<T> GetTopicBrokers()
        {
            HashSet<T> brokers = AllSubTopicsBrokers();
            brokers.UnionWith(Brokers);
            return brokers.ToList();
        }


        /// <summary>
        /// Get the brokers that have subscribe all topics
        /// from this broker until the root
        /// </summary>
        private HashSet<T> AllSubTopicsBrokers()
        {
            HashSet<T> brokers = new HashSet<T>(BrokersAllSubTopics);
            if (!IsRoot())
                brokers.UnionWith(Parent.AllSubTopicsBrokers());
            return brokers;
        }


        /// <summary>
        /// returns true if there is any interested (subscriber/broker) in the event
        /// </summary>
        public bool HaveSubscribers(String topic)
        {
            return GetTopicSubscribers().Count() > 0 || GetTopicBrokers().Count > 0;
        }


        /// <summary>
        /// returns true if the subscriber has a subscrition in the topic
        /// </summary>
        public bool HasSubscrition(T subscriberName, string[] topic)
        {
            List<T> subscribers = GetSubscribers(topic);
            return subscribers.Contains(subscriberName);
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

            foreach (Topic<T> subtopic in subTopics.Values)
                subtopic.Status();
            Console.WriteLine("");
        }

        private void PrintInfo(String description, HashSet<T> collection)
        {
            if (collection.Count() > 0)
            {
                Console.WriteLine(description);
                foreach (T s in collection)
                {
                    Console.WriteLine("-> {0}", s);
                }
            }
        }
    }
}
