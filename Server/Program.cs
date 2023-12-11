using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Server
{
    class Program
    {
        static TcpListener server;

        static int port = 13000;

        static List<User> Users;

        static int NextID = 1;

        static void Main()
        {
            Console.Title = "Server";
            Users = new List<User>();

            try
            {
                IPAddress LocalAddress = IPAddress.Parse("127.0.0.1");

                server = new TcpListener(LocalAddress, port);
                server.Start();

                Console.WriteLine("Server Started!");

                Thread ReadStreamsThread = new Thread(ReadUserStreams);
                ReadStreamsThread.Start();
                
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    ConnectUser(client);
                }
            }
            catch(SocketException error)
            {
                Console.WriteLine($"SocketException: {error}");
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("To Stop Server Press 'ESC' ...");

            while (true)
            {
                ConsoleKey Key = Console.ReadKey(true).Key;

                if (Key == ConsoleKey.Escape) StopServer();
            }
        }

        static void ReadUserStreams()
        {
            try
            {
                while (true)
                {
                    foreach (User user in Users.ToList())
                    {
                        if (!user.Stream.DataAvailable)
                            continue;
                        using (StreamReader sr = new StreamReader(user.Stream, System.Text.Encoding.ASCII, true, 1, true))
                        {
                            string line = sr.ReadLine();
                            if (line == "/Leave")
                            {
                                Console.WriteLine("DISCONNECT");
                                DisconnectUser(user);
                            }
                            else
                            {
                                SendMessageToAllUsers(user.ID, $"{user.Username} : {line}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //Server Send Message Format
        // Username : Message

        //Client Send Message Format
        // Message

        static void SendMessageToAllUsers(int SenderID, string message)
        {
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(message + '\n');
            foreach (User user in Users.ToList())
            {
                if (user.ID == SenderID) continue;

                user.Stream.Write(msg, 0, msg.Length);
            }
            Console.WriteLine(message);
        }

        static void SendMessageToUser(int SenderID, User user, string message)
        {
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(message + '\n');
            user.Stream.Write(msg, 0, msg.Length);

            Console.WriteLine(message);
        }

        static void ListenForConnection()
        {
            TcpClient client = server.AcceptTcpClient();
            ConnectUser(client);
        }

        static void ConnectUser(TcpClient Client)
        {
            User newUser = new User(NextID, Client);
            Users.Add(newUser);
            SendMessageToAllUsers(newUser.ID, $"{newUser.Username} Has Joined");
            NextID += 1;
        }

        static void DisconnectUser(User user)
        {
            Console.WriteLine("LEAVE");
            Users.Remove(user);
            //user.Client.Close();
            SendMessageToAllUsers(0, $"{user.Username} Disconnected");
        }

        static void StopServer()
        {
            foreach (User user in Users.ToList())
            {
                DisconnectUser(user);
            }
            server.Stop();
            Environment.Exit(0);
        }
    }
}
