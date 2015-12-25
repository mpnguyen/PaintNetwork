﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerPaint
{
    class Program
    {
        static TcpListener listener;
        const int MAX_CLIENTS = 100;
        const int PORT = 2015;
        const string LOCALHOST = "localhost";

        static List<ClientInfo> listSocketClient = new List<ClientInfo>();

        static object lockRoom = new Object();
        static List<Room> roomList = new List<Room>();

        static void Main(string[] args)
        {
            //  string host = LOCALHOST;
            //  IPHostEntry hostDnsEntry = Dns.GetHostEntry(host);
            //  IPAddress serverIp = hostDnsEntry.AddressList[0];

            IPAddress serverIp = IPAddress.Parse("127.0.0.1");
            listener = new TcpListener(serverIp, PORT);
            listener.Start();

            Console.WriteLine("Server listening.");

            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                Socket socket = listener.AcceptSocket();

                Console.WriteLine(socket.RemoteEndPoint + " Connected");
                Thread th = new Thread((obj) =>
                {
                    ProvideService((Socket)obj);
                });

                th.Start(socket);
            }
        }

        private static void ProvideService(Socket socket)
        {
            NetworkStream stream = new NetworkStream(socket);
            StreamReader clientReader = new StreamReader(stream);

            string name = clientReader.ReadLine();
            ClientInfo clientSoc = new ClientInfo() { client = socket, name = name };
            if (!IsNameAvailable(clientSoc.name))
            {
                SendMessage("OK||CN", clientSoc);
                Thread.CurrentThread.Abort();
            }
            listSocketClient.Add(clientSoc);

            string json = CreateListRoom(roomList);
            SendMessage("RL||" + json, clientSoc);
            json = CreateListClient(listSocketClient);
            BroadCast(json, listSocketClient, null);

            while (true)
            {
                string msg = clientReader.ReadLine();
                string[] msgPart = msg.Split(new string[] { "||" }, StringSplitOptions.None);

                switch (msgPart[0])
                {
                    case "CR":
                        lock (lockRoom)
                        {
                            CreateRoom(msgPart[1], clientSoc);
                        }
                        break;
                    case "JR":
                        lock (lockRoom)
                        {
                            JoinRoom(msgPart[1], clientSoc);
                        }
                        break;
                    case "LR":
                        lock (lockRoom)
                        {
                            LeaveRoom(msgPart[1], clientSoc);
                        }
                        break;

                    case "IV":
                        Invite();
                        break;
                    case "RQ":
                        AcceptRequest(msgPart[1], msgPart[2], clientSoc);
                        break;
                    case "CH":
                        lock (lockRoom)
                        {
                            foreach (var room in roomList)
                            {
                                foreach (var clientInfo in room.member)
                                {
                                    if (clientInfo == clientSoc)
                                    {
                                        BroadCast("CH||" + clientSoc.name + "||" + msgPart[1], room.member, clientSoc);
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

            }
        }

        public static void CreateRoom(string name, ClientInfo client)
        {
            foreach (var room in roomList)
            {
                if (room.name == name)
                {
                    SendMessage("FAIL||Cant not create room", client);
                    return;
                }
            }
            Room newRoom = new Room() { host = client, name = name, member = new List<ClientInfo>() };
            newRoom.member.Add(client);
            roomList.Add(newRoom);
            SendMessage("OK||Room created", client);
            string json = CreateListRoom(roomList);
            BroadCast("RL||" + json, listSocketClient, client);
            SendMessage("RL||" + json, client);
            var listJson = JsonConvert.DeserializeObject<List<RoomData>>(json);
        }

        public static void JoinRoom(string name, ClientInfo client)
        {
            foreach (var room in roomList.Where(room => room.name == name))
            {
                //room.member.Add(client);
                //SendMessage("Server: Success", client);
                //return;
                SendMessage("RQ||" + client.name, room.host);
                return;
            }
            SendMessage("FAIL||Cannot join room", client);
        }

        public static void LeaveRoom(string name, ClientInfo client)
        {
            foreach (var room in roomList.Where(room => room.name == name))
            {
                room.member.Remove(client);
                SendMessage("", client);
                return;
            }
        }

        public static void Invite()
        {
        }

        public static void SendMessage(string msg, ClientInfo client)
        {
            NetworkStream s = new NetworkStream(client.client);
            StreamWriter serverWrite = new StreamWriter(s);
            serverWrite.AutoFlush = true;
            serverWrite.WriteLine(msg);
        }

        public static void BroadCast(string msg, List<ClientInfo> clientInfos, ClientInfo sender)
        {
            if (sender != null)
            {
                foreach (var clientInfo in clientInfos)
                {
                    if (clientInfo != sender)
                    {
                        SendMessage(msg, clientInfo);
                    }
                }
            }
            else
            {
                foreach (var clientInfo in clientInfos)
                {
                    SendMessage(msg, clientInfo);
                }
            }
        }

        public static string CreateListRoom(List<Room> roomList)
        {
            List<RoomData> result = new List<RoomData>();
            foreach (var room in roomList)
            {
                result.Add(room.ToDataRoom());
            }
            return JsonConvert.SerializeObject(result);
        }

        public static string CreateListClient(List<ClientInfo> clientList)
        {
            List<ClientData> result = new List<ClientData>();
            foreach (var clientInfo in clientList)
            {
                result.Add(clientInfo.ToClientData());
            }
            return JsonConvert.SerializeObject(result);
        }

        public static bool IsNameAvailable(string name)
        {
            foreach (var clientInfo in listSocketClient)
            {
                if (clientInfo.name == name)
                {
                    return false;
                }
            }
            return true;
        }

        public static void AcceptRequest(string result, string name, ClientInfo client)
        {
            foreach (var room in roomList)
            {
                if (room.host == client)
                {
                    foreach (var clientInfo in listSocketClient.Where(clientInfo => clientInfo.name == name))
                    {
                        if (result == "Yes")
                        {
                            room.member.Add(clientInfo);
                            SendMessage("OK||JR", clientInfo);
                            return;
                        }
                        else
                        {
                            SendMessage("FAIL||Cant join room", clientInfo);
                            return;
                        }
                    }
                }
            }
        }
    }
}
