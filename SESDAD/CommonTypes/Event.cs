using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public class Event
    {
        private String publisherId;
        private String content;
        private Topic topic;
        private int id;

        public Event()
        {

        }

        public String PublisherId
        {
            get { return publisherId; }
            set { publisherId = value; }
        }

        public String Content
        {
            get { return content; }
            set { content = value; }
        }

        public Topic Topic
        {
            get { return topic; }
            set { topic = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
    }
}
