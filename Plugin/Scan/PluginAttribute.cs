using System;

namespace Veda.Plugin.Scan
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class PluginAttribute : Attribute
    {
        public String Name;
        public String Description;
    }
}
