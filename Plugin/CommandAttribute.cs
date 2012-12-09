using System;

namespace Veda.Plugin
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public String Name;
        public String Description;
    }
}
