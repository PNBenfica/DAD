using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    [Serializable]
    public class Topic
    {

        public string Name { get; set; }
        public Topic Parent { get; set; }
        private List<Topic> subTopics;

        public Topic(String name)
        {
            this.Name = name;
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
