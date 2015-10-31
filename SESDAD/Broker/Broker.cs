using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Threading;

namespace Broker
{
    public class Broker : MarshalByRefObject, IBroker
    {
        #region variables
        public String Name { get; set; }
        public String URL { get; set; }
        public IBroker Parent { get; set; }
        public Dictionary<string, IBroker> Children { get; set; }
        public List<IPublisher> Publishers { get; set; }
        public Dictionary<string, ISubscriber> Subscribers { get; set; }
        public List<Event> Events { get; set; }
        public Router Router { get; set; }
        #endregion

        #region classUtils

        public Broker(String name, String url)
        {
            this.Name = name;
            this.URL = url;
            this.Router = new FilteredRouter(this);
            this.Children = new Dictionary<string, IBroker>();
            this.Publishers = new List<IPublisher>();
            this.Subscribers = new Dictionary<string, ISubscriber>();
        }

        /// <summary>
        /// If the broker has no parent he is the root
        /// </summary>
        public bool IsRoot()
        {
            return this.Parent == null;
        }

        #endregion

        #region remoteMethods

        public DateTime Subscribe(String Id, bool isSubscriber, String topic)
        {
            Console.WriteLine("New subscrition from: {0} on topic: {1}", Id, topic);
            return Router.addSubscrition(Id, isSubscriber, topic);
        }

        public void UnSubscribe(String Id, bool isSubscriber, String topic)
        {
            Router.deleteSubscrition(Id, isSubscriber, topic);
        }

        /// <summary>
        /// Diffuse the message down the tree. Router knows where this need to go
        /// </summary>
        public void DiffuseMessage(Event e)
        {
            Thread thread = new Thread(() =>
            {
                Console.WriteLine("Diffusing message {0} from {1}", e.Id, e.PublisherId);
                Router.route(e);
            });
            thread.Start();
        }

        /// <summary>
        /// Diffuse the event to the root
        /// </summary>
        public DateTime DiffuseMessageToRoot(Event e)
        {
            DateTime timeStamp;

            if (IsRoot())  // If it is the root let s go down!
            {
                timeStamp = DateTime.Now;
                e.TimeStamp = timeStamp;
                Thread thread = new Thread(() => { this.DiffuseMessage(e); });
                thread.Start();
            }

            else
            {
                timeStamp = this.Parent.DiffuseMessageToRoot(e);
            }

            return timeStamp;
        }

        /// <summary>
        /// Notify the broker parent that he has a new born child
        /// </summary>
        /// <param name="parentUrl">Broker parent url</param>
        internal void notifyParent(string parentUrl)
        {
            Console.WriteLine("Registing in parent at {0}", parentUrl);
            this.Parent = (IBroker)Activator.GetObject(typeof(IBroker), parentUrl);
            this.Parent.registerNewChild(this.Name, this.URL);
        }

        /// <summary>
        /// Register a new child
        /// </summary>
        /// <param name="url">Url of the new broker child</param>
        public void registerNewChild(string name, string url)
        {
            IBroker child = (IBroker)Activator.GetObject(typeof(IBroker), url);
            Children.Add(name, child);
            Console.WriteLine("New child broker registed: {0}", url);
        }

        public void registerPublisher(string url)
        {
            IPublisher publisher = (IPublisher)Activator.GetObject(typeof(IPublisher), url);
            Publishers.Add(publisher);
            Console.WriteLine("New publisher registed: {0}", url);
        }

        /// <summary>
        /// Register a new subscriber
        /// </summary>
        /// <param name="url">Url of the new subscriber</param>
        public void registerSubscriber(string name, string url)
        {
            ISubscriber subscriber = (ISubscriber)Activator.GetObject(typeof(ISubscriber), url);
            Subscribers.Add(name, subscriber);
            Console.WriteLine("New subscriber registed: {0}", url);
        }

        /// <summary>
        /// Write in console the current status
        /// </summary>
        public void Status()
        {
            Console.WriteLine("\r\n<------Status------>");
            Router.TopicManager.Status();
            Console.WriteLine("");
        }

        #endregion
    }
}
