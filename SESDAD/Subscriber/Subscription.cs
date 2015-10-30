using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subscriber
{
    public class Subscription
    {

        public string Subscriber { get; set; }

        public DateTime TimeStamp { get; set; }

        public Subscription(String subscriber, DateTime timeStamp)
        {
            this.Subscriber = subscriber;
            this.TimeStamp = timeStamp;
        }

        public Subscription(String subscriber)
        {
            this.Subscriber = subscriber;
            this.TimeStamp = DateTime.Now;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            
            Subscription s = obj as Subscription;
            if ((Object)s == null)
                return false;

            return Subscriber == s.Subscriber;
        }

        public bool Equals(Subscription s)
        {
            if ((Object)s == null)
                return false;

            return Subscriber == s.Subscriber;
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = result * prime + Subscriber.GetHashCode();
            result = result * prime + TimeStamp.ToString("hh.mm.ss.ffffff").GetHashCode();
            return result;
        }
    }
}
