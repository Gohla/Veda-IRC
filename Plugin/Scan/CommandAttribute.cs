using System;

namespace Veda.Plugin.Scan
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public String Name;
        public String Description;
    }
}
