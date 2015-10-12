using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public class Topic
    {
        private List<Topic> subTopics;
        private Topic parent;

        public Topic()
        {

        }

        public Topic Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public void AddTopic(Topic subTopic)
        {
            if (subTopics == null)
            {
                subTopics = new List<Topic>();
            }
            subTopics.Add(subTopic);
        }
    }
}
