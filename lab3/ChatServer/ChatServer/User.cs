using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class User
    {
        public String Nick { get; set; }
        public String Port { get; set; }

        public User(String nick, String port)
        {
            this.Nick = nick;
            this.Port = port;
        }
    }
}
