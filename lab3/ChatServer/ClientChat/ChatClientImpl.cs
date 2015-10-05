using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;

namespace ClientChat
{

    delegate void DelAddMsg(string nickname, string mensagem);

    class ChatClientImpl : MarshalByRefObject, IChatClient
    {
        public static Form1 form;

        public void receiveMsg(String nickname, String message)
        {
            form.Invoke(new DelAddMsg(form.addNewChatMessage), new Object[]{nickname, message});
        }
    }
}
