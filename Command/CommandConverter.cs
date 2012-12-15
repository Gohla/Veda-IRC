using System;
using System.Collections.Generic;
using Veda.Interface;

namespace Veda.Command
{
    public abstract class CommandConverter<TFrom, TTo> : ICommandConverter
    {
        public Type FromType { get { return typeof(TFrom); } }
        public Type ToType { get { return typeof(TTo); } }
        public abstract Type ContextType { get; }

        public abstract object Convert(object obj, object context);

        public override bool Equals(object other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return Equals(other as ICommandConverter);
        }

        public bool Equals(ICommandConverter other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return
                EqualityComparer<Type>.Default.Equals(this.FromType, other.FromType)
             && EqualityComparer<Type>.Default.Equals(this.ToType, other.ToType)
             && EqualityComparer<Type>.Default.Equals(this.ContextType, other.ContextType)
             ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + EqualityComparer<Type>.Default.GetHashCode(this.FromType);
                hash = hash * 23 + EqualityComparer<Type>.Default.GetHashCode(this.ToType);
                hash = hash * 23 + EqualityComparer<Type>.Default.GetHashCode(this.ContextType);
                return hash;
            }
        }

        public override string ToString()
        {
            return 
                this.FromType.Name
              + " -> " 
              + this.ToType.Name
              + " (" + this.ContextType.Name + ")";
        }
    }
}
