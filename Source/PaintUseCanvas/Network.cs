using System;
using System.IO;
using System.Net.Sockets;

namespace PaintUseCanvas
{
    public class Network
    {
        const int PORT = 2015;
        TcpClient client;
        Stream remoteStream;
        StreamWriter clientWriter;
        StreamReader serverReader;

        public bool Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect("192.168.11.23", PORT);
            }
            catch (Exception)
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