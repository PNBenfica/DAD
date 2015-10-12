using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using CommonTypes;

namespace ClientChat
{
    public partial class Form1 : Form
    {

        private IChatServer chatServer;
        private String nickname;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            String nickName = textBox_nick.Text;

            // a user can register if the input is valid and has not yet connected to the server
            bool canRegister = nickName.Length != 0 && textBox_Port.Text.Length != 0 && chatServer == null;

            if (canRegister)
            {
                ChatClientImpl.form = this;

                int port = Int32.Parse(textBox_Port.Text);
                this.nickname = nickName;


                TcpChannel channel = new TcpChannel(port);
                ChannelServices.RegisterChannel(channel, false);
                RemotingServices.Marshal(new ChatClientImpl(), "ChatClient",
                    typeof(ChatClientImpl));

                this.chatServer = (IChatServer)Activator.GetObject(
                    typeof(IChatServer),
                    "tcp://localhost:8086/ChatServer");
                chatServer.Register(nickName, port.ToString());
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (chatServer != null && textBox_Message.Text.Length != 0)
            {
                chatServer.sendMessage(this.nickname, textBox_Message.Text);

                addNewChatMessage(this.nickname, textBox_Message.Text);
                textBox_Message.Clear();
            }
        }

        public void addNewChatMessage(String nick, String message)
        {
            DateTime localDate = DateTime.Now;
            String hours = "[" + localDate.Hour.ToString() + ":" + localDate.Minute.ToString() + "]";
            textBox_chatMessage.Text += hours + nick + ":\r\n" + message + "\r\n";
        }
    }
}
