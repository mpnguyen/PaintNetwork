using System.Net.Sockets;

namespace ServerPaint
{
    public class ClientInfo 
    {
        public Socket client;
        public string name;

        public ClientData ToClientData()
        {
            ClientData result = new ClientData();
            result.Name = name;
            return result;
        }
    }

    public class ClientData
    {
        public string Name { get; set; }    
    }
}