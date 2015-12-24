using System.Collections.Generic;

namespace ServerPaint
{
    public class Room
    {
        public string name;
        public ClientInfo host;
        public List<ClientInfo> member;
    }
}