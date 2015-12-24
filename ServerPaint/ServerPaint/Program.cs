using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerPaint
{
    class Program
    {
        static TcpListener listener;
        const int MAX_CLIENTS = 100;
        const int PORT = 2015;
        const string LOCALHOST = "localhost";

        static List<Socket> listSocketClient = new List<Socket>();
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
                new Thread(ProvideService).Start(i+1);
            }
        }

        private static void ProvideService( object ID )
        {
            Socket socket = listener.AcceptSocket();
            listSocketClient.Add(socket);
           

            NetworkStream stream = new NetworkStream(socket);
            StreamReader clientReader = new StreamReader(stream);

            string name = clientReader.ReadLine();

            while (true)
            {
                string chat = clientReader.ReadLine();

              //  Console.WriteLine(ID.ToString()+ chat+listSocketClient.Count.ToString());

                foreach (Socket value in listSocketClient)
                {
                     NetworkStream  networkStream = new NetworkStream(value);
                     StreamWriter serverWrite = new StreamWriter(networkStream);
                     serverWrite.AutoFlush = true;
                     serverWrite.WriteLine(name);
                     serverWrite.WriteLine(chat);
                }
            }
        }
    }
}
