using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Publisher
{
    class Publisher: CommonTypes.IPublisherInterface
    {
        private List<Event> events;
        private String publisherId;
        private String url;

        public void Publish(String topic, String content)
        {

        }


    }
}
