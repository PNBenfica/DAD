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

        public String PublisherId { get; set; }

        public String Content { get; set; }

        public String Topic { get; set; }

        public int Id { get; set; }

        public Event(String publisherId, String content, String topic, int id)
        {
            this.PublisherId = publisherId;
            this.Content = content;
            this.Topic = topic;
            this.Id = id;
        }
    }
}
