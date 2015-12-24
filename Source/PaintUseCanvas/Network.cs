using System;
using System.IO;
using System.Net.Sockets;

namespace PaintUseCanvas
{
    public class Network
    {
        const int PORT = 2015;
        const string LOCALHOST = "localhost";
        TcpClient client;
        Stream remoteStream;
        StreamWriter clientWriter;
        StreamReader serverReader;

        public bool Connect()
        {
            // string host = LOCALHOST;

            client = new TcpClient();
            client.Connect("127.0.0.1", PORT);

            if (client == null)
            {
                return false;
            }

            remoteStream = client.GetStream();
            clientWriter = new StreamWriter(remoteStream);
            serverReader = new StreamReader(remoteStream);

            return true;
        }

        public void Disconnect()
        {
            client.Close();
        }

        public void ClientSend(string text)
        {
            try
            {
                clientWriter.AutoFlush = true;
                clientWriter.WriteLine(text);
            }
            catch (Exception)
            {

            }
        }

        public string ClientRecieve()
        {
            return serverReader.ReadLine();
        }
    }
}