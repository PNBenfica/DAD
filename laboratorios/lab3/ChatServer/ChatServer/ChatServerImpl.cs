using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;


namespace ChatServer
{
    class ChatServerImpl : MarshalByRefObject, IChatServer
    {
        List<User> users = new List<User>();

        public ChatServerImpl()
        {
            users = new List<User>();
        }

        public void Register(String nickName, String port)
        {
            User newChatUser = new User(nickName, port);
            users.Add(newChatUser);

            Console.WriteLine("New chat user registed: {0} at tcp://localhost:{1}/ChatClient", nickName, port);
        }

        public void sendMessage(String nickName, String message)
        {
            foreach (User user in users)
            {
                bool isSender = user.Nick.Equals(nickName);
                if (!isSender)
                {
                    String url = "tcp://localhost:" + user.Port + "/ChatClient";
                    IChatClient chatClient = (IChatClient)Activator.GetObject(typeof(IChatClient), url);
                    chatClient.receiveMsg(nickName, message);
                }
            }
            Console.WriteLine("{0} Send message: {1}", nickName, message);
        }

    }
}
