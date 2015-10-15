using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMaster
{
    public class UnknownProcessException : Exception
    {
        public UnknownProcessException(string message)
            : base(message)
        {
        }
    }
}
