using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class ChatImpl : CommonTypes.IChatInterface
    {
        Hashtable users = new Hashtable();
        public void Register(String nickName, String Url)
        {

             users.Add(nickName, Url);
        }

        public void sendMessage(String nickName, String message)
        {
            foreach (DictionaryEntry entry in users)
            {
                if (!entry.Key.Equals(nickName))
                {

                }
            }

        }

    }
}
