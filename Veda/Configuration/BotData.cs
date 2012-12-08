using System.Collections.Generic;

namespace Veda.Configuration
{
    public class BotData
    {
        public IList<ConnectionData> Connections { get; set; }

        public BotData()
        {
            Connections = new List<ConnectionData>();
        }
    }
}
