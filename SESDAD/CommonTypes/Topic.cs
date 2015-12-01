using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public class Topic<T>
    {
        #region variables
        
        public string Name { get; set; }
        public Topic<T> Parent { get; set; }
        private Dictionary<string, Topic<T>> subTopics;
        public HashSet<T> Subscribers { get; set; }
        public HashSet<T> SubscribersAllSubTopics { get; set; } // Subscribers that subscribes an entire topic

        #endregion

        #region classUtils

        public Topic(String name, Topic<T> parent = null)
        {
            this.Name = name;
            this.Parent = parent;
            this.subTopics = new Dictionary<string, Topic<T>>();
            this.Subscribers = new HashSet<T>();
            this.SubscribersAllSubTopics = new HashSet<T>();
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
        /// returns true if there is any interested (subscriber/broker) in the event
        /// </summary>
        public bool HaveSubscribers(String[] topic)
        {
            return GetSubscribers(topic).Count() > 0;
        }



        public List<string> GetAllSubscriptions(T subscriber)
        {
            List<string> subscriptions = new List<string>();
            if (Subscribers.Contains(subscriber))
                subscriptions.Add(this.ToString());
            else if (SubscribersAllSubTopics.Contains(subscriber))
                subscriptions.Add(this.ToString() + "/*");

            foreach (Topic<T> subtopic in subTopics.Values)
                subscriptions.AddRange(subtopic.GetAllSubscriptions(subscriber));
            return subscriptions;
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
            PrintInfo("Single Subscription", Subscribers);
            PrintInfo("Full Subscription", SubscribersAllSubTopics);

            foreach (Topic<T> subtopic in subTopics.Values)
                subtopic.Status();
        }

        private void PrintInfo(String description, HashSet<T> collection)
        {
            if (collection.Count() > 0)
            {
                foreach (T s in collection)
                {
                    Console.WriteLine("{0} | {1} | Topic: {2}", s, description, this);
                }
            }
        }

        public override string ToString()
        {
            if (IsRoot())
                return "";
            else
                return Parent.ToString() + "/" + this.Name;
        }

        #endregion

        #region subscribeTopic


        /// <summary>
        /// Add a new subscrition
        /// </summary>
        /// <param name="processName">Name of the broker/subscriber</param>
        public void Subscribe(T processName, String[] topicArray)
        {
            if (topicArray.Length == 0)
            {
                addSubscrition(processName);
            }
            else if (topicArray.Length == 1 && topicArray[0].Equals("*")) // want to subscribe all
            {
                SubscribeAll(processName);
            }
            else
            {
                SubscribeSubtopic(processName, topicArray);
            }
        }

        /// <summary>
        /// Subscribes all the subtopics
        /// </summary>
        private void SubscribeAll(T name)
        {
            //if already exist a subscribe all in the tree
            if (checkAlreadySubscribeAll(name))
            {
                return;
            }

            SubscribersAllSubTopics.Add(name);

            clearSubTopicsSubscribeAll(name, true);
        }


        /// <summary>
        /// Subscribes the subtopic
        /// </summary>
        /// <param name="topic"> ["subtopic", "sub-subtopic", ...] </param>
        private void SubscribeSubtopic(T name, String[] topic)
        {
            Topic<T> subTopic = GetSubTopic(topic[0]);
            String[] restSubTopics = (String[])topic.Skip(1).ToArray(); // removes first element
            subTopic.Subscribe(name, restSubTopics);
        }

        /// <summary>
        /// Add a subscriber/broker subscrition
        /// </summary>
        private void addSubscrition(T name)
        {
            Subscribers.Add(name);
        }

        public void clearSubTopicsSubscribeAll(T name, bool isParentTopic)
        {
            if (!isParentTopic)
            {
                SubscribersAllSubTopics.Remove(name);
            }

            foreach (Topic<T> subTopic in subTopics.Values)
            {
                subTopic.clearSubTopicsSubscribeAll(name, false);
            }
        }

        public bool checkAlreadySubscribeAll(T name)
        {
            if (SubscribersAllSubTopics.Contains(name))
            {
                return true;
            }
            else if (IsRoot())
            {
                return false;
            }
            else
            {
                return Parent.checkAlreadySubscribeAll(name);
            }
        }

        #endregion

        #region unsubscribeTopic
        private void removeSubscrition(T processName)
        {
            Subscribers.Remove(processName);
        }

        public void UnSubscribe(T processName, string[] topicArray)
        {
            if (topicArray.Length == 0)
            {
                removeSubscrition(processName);
            }
            else if (topicArray.Length == 1 && topicArray[0].Equals("*")) // want to subscribe all
            {
                UnSubscribeAll(processName);
            }
            else
            {
                UnSubscribeSubtopic(processName, topicArray);
            }
        }

        private void UnSubscribeAll(T processName)
        {
            SubscribersAllSubTopics.Remove(processName);
            UnsubscribeAllSubTopics(processName, true);
        }

        private void UnsubscribeAllSubTopics(T processName, bool isParent)
        {
            if (!isParent)
            {
                Subscribers.Remove(processName);
                SubscribersAllSubTopics.Remove(processName);
            }
            foreach (Topic<T> topic in subTopics.Values)
            {
                topic.UnsubscribeAllSubTopics(processName, false);
            }

        }

        private void UnSubscribeSubtopic(T processName, string[] topicArray)
        {
            Topic<T> subTopic = GetSubTopic(topicArray[0]);
            String[] restSubTopics = (String[])topicArray.Skip(1).ToArray(); // removes first element
            subTopic.UnSubscribe(processName, restSubTopics);
        }
        #endregion    
    }
}
