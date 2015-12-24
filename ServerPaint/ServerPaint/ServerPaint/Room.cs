using System.Collections.Generic;

namespace ServerPaint
{
    public class Room
    {
        public string name;
        public ClientInfo host;
        public List<ClientInfo> member;

        public RoomData ToDataRoom()
        {
            RoomData result = new RoomData()
            {
                Name = name,
                Host = host.name,
                Member = new List<string>()
            };
            foreach (var clientInfo in member)
            {
                result.Member.Add(clientInfo.name);
            }
            return result;
        }
    }

    public class RoomData
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public List<string> Member { get; set; }
    }
}