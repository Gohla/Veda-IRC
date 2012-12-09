using System;
using System.Collections.Generic;
using System.Linq;
using Veda.Interface;

namespace Veda.Command
{
    public abstract class Command : ICommand
    {
        public IPlugin Plugin { get; private set; }
        public String Name { get; private set; }
        public String Description { get; private set; }
        public Type[] ParameterTypes { get; private set; }

        public Command(IPlugin plugin, String name, String description, params Type[] parameterTypes)
        {
            Plugin = plugin;
            Name = name;
            Description = description;
            ParameterTypes = parameterTypes;
        }

        public bool IsCompatible(params Type[] argumentTypes)
        {
            if(argumentTypes.Length != ParameterTypes.Length)
                return false;

            bool compatible = true;
            for(int i = 0; i < ParameterTypes.Length; ++i)
                compatible &= ParameterTypes[i].IsAssignableFrom(argumentTypes[i]);
            return compatible;
        }

        public bool IsPartialCompatible(params Type[] argumentTypes)
        {
            if(argumentTypes.Length > ParameterTypes.Length)
                return false;

            bool compatible = true;
            for(int i = 0; i < argumentTypes.Length; ++i)
                compatible &= ParameterTypes[i].IsAssignableFrom(argumentTypes[i]);
            return compatible;
        }

        public abstract object Call(params object[] arguments);

        public override bool Equals(object other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return Equals(other as ICommand);
        }

        public bool Equals(ICommand other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return
                EqualityComparer<IPlugin>.Default.Equals(this.Plugin, other.Plugin)
             && StringComparer.OrdinalIgnoreCase.Equals(this.Name, other.Name)
             && this.ParameterTypes.SequenceEqual(other.ParameterTypes)
             ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + EqualityComparer<IPlugin>.Default.GetHashCode(this.Plugin);
                hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name);
                hash = hash * 23 + EqualityComparer<Type[]>.Default.GetHashCode(this.ParameterTypes);
                return hash;
            }
        }

        public override String ToString()
        {
            return
                "("
              + this.Name
              + (ParameterTypes.Length == 1 ? String.Empty : " ")
              + String.Join(", ", ParameterTypes.Skip(1).Select(t => t.Name)) 
              + ")";
        }
    }
}
