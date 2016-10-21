using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerData;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Client
{
    class Client
    {
        public static Socket master;
        public static string name;
        public static string ID;

        static void Main(string[] args)
        {
            A: Console.Clear();
            Console.Write("Enter yout name:");
            name = Console.ReadLine();

            Console.Write("Enter host ip:");
            var ip = Console.ReadLine();

            master = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), 2929);

            try
            {
                master.Connect(ipe);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could no connect to host:" + e.Message);
                Thread.Sleep(2000);
                goto A;
            }

            Thread t = new Thread(DataIN);
            t.Start();

            while (true)
            {
                Console.Write("::>");
                string input = Console.ReadLine();

                Packet p = new Packet(PacketType.Chat, ID);
                p.Data.Add(name);
                p.Data.Add(input);
                master.Send(p.ToBytes());
            }
        }

        static void DataIN()
        {
            byte[] buffer;
            int readbytes;

            while (true)
            {
                try
                {
                    buffer = new byte[master.SendBufferSize];
                    readbytes = master.Receive(buffer);

                    if (readbytes > 0)
                    {
                        DataManager(new Packet(buffer));
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("The server has down!");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
        }


        static void DataManager(Packet p)
        {
            switch (p.PacketType)
            {
                case PacketType.Registration:
                    ID = p.Data[0];
                    break;
                case PacketType.Chat:
                    ConsoleColor c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(p.Data[0] + ": " + p.Data[1]);
                    Console.ForegroundColor = c;
                    break;
            }
        }
    }
}
