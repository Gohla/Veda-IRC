using System;
using System.Collections.Generic;
using Veda.Interface;

namespace Veda.Authentication
{
    public class BotGroup : IBotGroup
    {
        public String Name { get; private set; }
        public int PrivilegeLevel { get; private set; }

        public BotGroup(String name, int privilegeLevel)
        {
            Name = name;
            PrivilegeLevel = privilegeLevel;
        }

        public bool IsMorePrivileged(IBotGroup other)
        {
            return this.PrivilegeLevel > other.PrivilegeLevel;
        }

        public bool IsMoreOrSamePrivileged(IBotGroup other)
        {
            return this.PrivilegeLevel >= other.PrivilegeLevel;
        }

        public int CompareTo(IBotGroup other)
        {
        	if(ReferenceEquals(other, null))
        		return 1;
        
        	int result = 0;
            result = this.Name.CompareTo(other.Name);
        	return result;
        }
        
        public override bool Equals(object other)
        {
        	if(ReferenceEquals(other, null))
        		return false;
        
        	return Equals(other as IBotGroup);
        }
        
        public bool Equals(IBotGroup other)
        {
        	if(ReferenceEquals(other, null))
        		return false;
        
        	return
        		EqualityComparer<String>.Default.Equals(this.Name, other.Name)
        	 ;
        }
        
        public override int GetHashCode()
        {
        	unchecked
        	{
        		int hash = 17;
        		hash = hash * 23 + EqualityComparer<String>.Default.GetHashCode(this.Name);
        		return hash;
        	}
        }
        
        public override String ToString()
        {
        	return this.Name;
        }
    }
}
