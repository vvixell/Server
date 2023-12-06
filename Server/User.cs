using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Server
{
    class User
    {
        public string Username;

        public TcpClient Client;

        public NetworkStream Stream { get { return Client.GetStream(); } }

        public int ID;

        public User(int ID, TcpClient Client)
        {
            this.Client = Client;
            this.ID = ID;

            using (StreamReader sr = new StreamReader(Client.GetStream()))
            {
                Username = sr.ReadLine();
            }
        }
    }
}
