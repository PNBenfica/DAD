using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace Subscriber
{
    class Subscriber: CommonTypes.ISubscriberInterface
    {
        private List<Topic> subscriptions;
        private String subscriberId;
        private OrderStrategy orderStrategy;
        private String url;

        public void Subscribe()
        {

        }

        public void UnSubscribe()
        {

        }

        public void ReceiveMessage()
        {

        }

    }
}
