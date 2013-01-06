using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda.Command
{
    public abstract class AbstractCommand : ICommand
    {
        private String _description;

        public IPlugin Plugin { get; private set; }
        public String Name { get; private set; }
        public String Description
        {
            get
            {
                return _description + (Private ? " This command can only be sent in a private message." : String.Empty);
            }
            protected set
            {
                _description = value;
            }
        }
        public Type[] ParameterTypes { get; protected set; }
        public String[] ParameterNames { get; protected set; }
        public PermissionAttribute[] DefaultPermissions { get; protected set; }
        public bool Private { get; protected set; }

        public AbstractCommand(IPlugin plugin, String name)
        {
            Plugin = plugin;
            Name = name;
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

        public abstract object Call(IContext context, params object[] arguments);

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
             && ArrayEqualityComparer<Type>.Default.Equals(this.ParameterTypes, other.ParameterTypes)
             ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + EqualityComparer<IPlugin>.Default.GetHashCode(this.Plugin);
                hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name);
                hash = hash * 23 + ArrayEqualityComparer<Type>.Default.GetHashCode(this.ParameterTypes);
                return hash;
            }
        }

        public override String ToString()
        {
            return
                "(" 
              + ControlCodes.Bold
              (
                this.Name.ToLower()
              + (ParameterTypes.Length == 0 ? String.Empty : " ")
              + String.Join(", ", ParameterTypes.Zip(ParameterNames, (t, n) => "<" + n + ":" + t.Name + ">"))
              )
              + ")"
              ;
        }
    }
}
