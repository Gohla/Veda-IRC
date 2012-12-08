using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Veda.Command
{
    public abstract class Command : ICommand
    {
        public String Name { get; private set; }
        public String DisplayName { get; private set; }
        public Type[] ParameterTypes { get; private set; }

        public Command(String name, String displayName, params Type[] parameterTypes)
        {
            Name = name;
            DisplayName = displayName;
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
                EqualityComparer<String>.Default.Equals(this.Name, other.Name)
             && this.ParameterTypes.SequenceEqual(other.ParameterTypes);
             ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + EqualityComparer<String>.Default.GetHashCode(this.Name);
                hash = hash * 23 + EqualityComparer<Type[]>.Default.GetHashCode(this.ParameterTypes);
                return hash;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
