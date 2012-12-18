using System;
using Veda.Interface;

namespace Veda.Authentication
{
    public class BotGroup : IBotGroup
    {
        public String Name { get; private set; }

        public BotGroup(String name)
        {
            Name = name;
        }
    }
}
