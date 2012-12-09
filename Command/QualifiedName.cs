using System;
using System.Collections.Generic;
using Veda.Interface;

namespace Veda.Command
{
    public class QualifiedName : IEquatable<QualifiedName>
    {
        public readonly String PluginName;
        public readonly String Name;

        public QualifiedName(String pluginName, String name)
        {
            PluginName = pluginName;
            Name = name;
        }

        public override bool Equals(object other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return Equals(other as QualifiedName);
        }

        public bool Equals(QualifiedName other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return
                StringComparer.OrdinalIgnoreCase.Equals(this.PluginName, other.PluginName)
             && StringComparer.OrdinalIgnoreCase.Equals(this.Name, other.Name)
            ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(this.PluginName);
                hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name);
                return hash;
            }
        }

        public override String ToString()
        {
            return this.PluginName + "." + this.Name;
        }
    }
}
