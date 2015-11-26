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


        public Event(int id, String publisherId, String topic)
        {
            this.Id = id;
            this.PublisherId = publisherId;
            this.Topic = topic;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Event e = obj as Event;
            if ((Object)e == null)
                return false;

            return this.Id == e.Id && this.PublisherId.Equals(e.PublisherId) && this.Content.Equals(e.Content);
        }

        public bool Equals(Event e)
        {
            if ((Object)e == null)
                return false;

            return this.Id == e.Id && this.PublisherId.Equals(e.PublisherId);
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = result * prime + (""+ this.PublisherId).GetHashCode();
            return result;
        }
        #endregion

    }
}
