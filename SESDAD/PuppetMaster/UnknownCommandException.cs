using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMaster
{
    public class UnknownCommandException : Exception
    {
        public UnknownCommandException(string message)
            : base(message)
        {
        }
    }
}
