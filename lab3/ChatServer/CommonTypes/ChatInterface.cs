using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    interface IChatInterface
    {
        public void Register(String nickName, String Url);
   

        public void sendMessage(String nickName, String message);
  
    }
}
