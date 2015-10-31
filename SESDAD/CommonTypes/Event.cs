using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    [Serializable]
    public class Event
    {
        #region variables
        public String PublisherId { get; set; }

        public String Content { get; set; }

        public String Topic { get; set; }

        public int Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public List<Event> PreviousEvents { get; set; }

        #endregion

        #region classUtils
        public Event(int id, String publisherId, String topic, String content, List<Event> previousEvents)
        {
            this.Id = id;
            this.PublisherId = publisherId;
            this.Topic = topic;
            this.Content = content;
            this.PreviousEvents = new List<Event>(previousEvents.ToArray());
        }


        public Event(int id, DateTime timeStamp, String publisherId, String topic)
        {
            this.Id = id;
            this.TimeStamp = timeStamp;
            this.PublisherId = publisherId;
            this.Topic = topic;
        }

        #endregion

    }
}
