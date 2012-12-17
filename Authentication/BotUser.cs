using System;
using Veda.Interface;

namespace Veda.Authentication
{
    public class BotUser : IBotUser
    {
        public String Username { get; set; }
        public IBotGroup Group { get; set; }
    }
}
