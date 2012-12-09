using System;
using System.Collections.Generic;
using System.Linq;
using Veda.Interface;

namespace Veda.Command
{
    public class QualifiedNameTypes : IEquatable<QualifiedNameTypes>
    {
        public readonly String PluginName;
        public readonly String Name;
        public readonly Type[] ParameterTypes;

        public QualifiedNameTypes(String pluginName, String name, Type[] parameterTypes)
        {
            PluginName = pluginName;
            Name = name;
            ParameterTypes = parameterTypes;
        }

        public override bool Equals(object other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return Equals(other as QualifiedNameTypes);
        }

        public bool Equals(QualifiedNameTypes other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return
                StringComparer.OrdinalIgnoreCase.Equals(this.PluginName, other.PluginName)
             && StringComparer.OrdinalIgnoreCase.Equals(this.Name, other.Name)
             && this.ParameterTypes.SequenceEqual(other.ParameterTypes)
            ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(this.PluginName);
                hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name);
                hash = hash * 23 + EqualityComparer<Type[]>.Default.GetHashCode(this.ParameterTypes);
                return hash;
            }
        }

        public override String ToString()
        {
            return this.PluginName + "." + this.Name + "(" + ParameterTypes.Select(t => t.Name).ToString(", ") + ")";
        }
    }
}
