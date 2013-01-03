using System;

namespace Veda.Interface
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public String Name;
        public String Description;
        public bool Private = false;
    }
}
