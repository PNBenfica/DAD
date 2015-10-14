using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Subscriber
{
    class Subscriber: ISubscriber
    {
        private List<Topic> subscriptions;
        private String subscriberId;
        private OrderStrategy orderStrategy;
        private String url;



        public void Subscribe(string topic)
        {

        }

        public void UnSubscribe(string topic)
        {

        }

        public void ReceiveMessage(Event evento)
        {

        }
    }
}
