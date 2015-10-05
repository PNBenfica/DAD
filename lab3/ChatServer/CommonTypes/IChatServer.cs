using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IChatServer
    {
        void Register(String nickName, String Url);   
        void sendMessage(String nickName, String message);  
    }
}
