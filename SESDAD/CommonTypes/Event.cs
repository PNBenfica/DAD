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

        public string PublisherId { get; set; }

        public string Content { get; set; }

        public Topic Topic { get; set; }

        public int Id { get; set; }

        public Event(String publisherId, String content, String topic, int id)
        {
            this.PublisherId = publisherId;
            this.Content = content;
            this.Topic = new Topic(topic);
            this.Id = id;
        }
    }
}
