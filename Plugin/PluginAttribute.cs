using System;

namespace Veda.Plugin
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class PluginAttribute : Attribute
    {
        public String Name;
        public String Description;
    }
}
