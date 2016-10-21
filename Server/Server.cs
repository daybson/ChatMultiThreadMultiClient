using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerData;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Server
{
    class Server
    {
        static Socket listenerSocket;
        static List<ClientData> clients;
        public string id;

        //start server
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server on " + Packet.GetIP4Address());

            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clients = new List<ClientData>();

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(Packet.GetIP4Address()), 2929);
            listenerSocket.Bind(ip);

            var listenThread = new Thread(ListenThread);
            listenThread.Start();
        }


        //listens for clients trying to connect
        static void ListenThread()
        {
            while (true)
            {
                listenerSocket.Listen(0);
                clients.Add(new ClientData(listenerSocket.Accept()));
            }
        }


        //clientdata thread - receives data from client individually
        public static void DataIN(object cSocket)
        {
            Socket clientSocket = (Socket)cSocket;
            byte[] buffer;
            int readBytes;

            while (true)
            {
                try
                {
                    buffer = new byte[clientSocket.SendBufferSize];
                    readBytes = clientSocket.Receive(buffer);

                    if (readBytes > 0)
                    {
                        var packet = new Packet(buffer);
                        DataManager(packet);
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("A client was disconnected!");
                }
            }
        }


        //data manager 
        public static void DataManager(Packet p)
        {
            switch (p.PacketType)
            {
                case PacketType.Chat:
                    foreach (ClientData c in clients)
                    {
                        c.clientSocket.Send(p.ToBytes());
                    }
                    break;
            }
        }
    }

    //==============================================================================================================================

    class ClientData
    {
        public Socket clientSocket;
        public Thread clientThread;
        public string id;

        public ClientData()
        {
            this.id = Guid.NewGuid().ToString();
            clientThread = new Thread(Server.DataIN);
            clientThread.Start(clientSocket);
            SendRegistrationPacket();
        }

        public ClientData(Socket clientSocket)
        {
            this.id = Guid.NewGuid().ToString();
            this.clientSocket = clientSocket;
            clientThread = new Thread(Server.DataIN);
            clientThread.Start(clientSocket);
            SendRegistrationPacket();
        }

        public void SendRegistrationPacket()
        {
            var p = new Packet(PacketType.Registration, "server");
            p.Data.Add(id);
            clientSocket.Send(p.ToBytes());
        }
    }
}
