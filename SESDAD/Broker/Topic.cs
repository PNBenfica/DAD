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
        public List<String> Subscribers { get; set;}
        public List<String> Brokers { get; set; }


        public Topic(String name, Topic parent = null)
        {
            this.Name = name;
            this.Parent = parent;
            this.subTopics = new Dictionary<string, Topic>();
            this.Subscribers = new List<String>();
            this.Brokers = new List<String>();
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
                if (Name.Equals("*")) // want to subscribe all
                    Parent.SubscribeAll(name, isSubscriber);
                else
                    addSubscrition(name, isSubscriber);
            }
            else
            {
                Topic subTopic = GetSubTopic(topic[0]);
                String[] restSubTopics = (String[])topic.Skip(1).ToArray(); // removes first element
                subTopic.Subscribe(name, restSubTopics, isSubscriber);
            }
        }


        /// <summary>
        /// Subscribes in all the subtopics
        /// </summary>
        private void SubscribeAll(string name, bool isSubscriber)
        {
            throw new NotImplementedException();
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
                return Subscribers;

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
            return Brokers; //TODO
        }


        public bool HaveSubscribers(String topic)
        {
            throw new NotImplementedException();
        }
    }
}
