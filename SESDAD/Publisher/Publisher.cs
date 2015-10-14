using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Publisher
{
    class Publisher: IPublisher
    {
        private List<Event> events;
        private String publisherId;
        private String url;

        public void Publish(String topic, String content)
        {

        }


    }
}
