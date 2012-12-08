using System;
using System.Collections.Generic;

namespace Veda.Configuration
{
    public class ConnectionData
    {
        public String Address { get; set; }
        public ushort Port { get; set; }
        public String Nickname { get; set; }
        public String Username { get; set; }
        public String Realname { get; set; }
        public String Password { get; set; }

        public IDictionary<String, ChannelData> Channels { get; set; }

        public ConnectionData()
        {
            Channels = new Dictionary<String, ChannelData>();
        }
    }
}
