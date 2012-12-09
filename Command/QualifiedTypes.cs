﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Veda.Command
{
    public class QualifiedTypes : IEquatable<QualifiedTypes>
    {
        public readonly String Name;
        public readonly Type[] ParameterTypes;

        public QualifiedTypes(String name, Type[] parameterTypes)
        {
            Name = name;
            ParameterTypes = parameterTypes;
        }

        public override bool Equals(object other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return Equals(other as QualifiedTypes);
        }

        public bool Equals(QualifiedTypes other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return
                StringComparer.OrdinalIgnoreCase.Equals(this.Name, other.Name)
             && this.ParameterTypes.SequenceEqual(other.ParameterTypes)
            ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name);
                hash = hash * 23 + EqualityComparer<Type[]>.Default.GetHashCode(this.ParameterTypes);
                return hash;
            }
        }

        public override String ToString()
        {
            return this.Name;
        }
    }
}