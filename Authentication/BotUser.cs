using System;
using System.Collections.Generic;
using Gohla.Shared;
using Veda.Interface;

namespace Veda.Authentication
{
    public class BotUser : IBotUser
    {
        public String Username { get; private set; }
        public String HashedPassword { get; private set; }
        public IBotGroup Group { get; private set; }

        public BotUser(String username, String password, IBotGroup group, bool hash = true)
        {
            Username = username;
            HashedPassword = hash ? PasswordHash.CreateHash(password) : password;
            Group = group;
        }

        public bool CheckPassword(String password)
        {
            return PasswordHash.ValidatePassword(password, HashedPassword);
        }

        public int CompareTo(IBotUser other)
        {
        	if(ReferenceEquals(other, null))
        		return 1;
        
        	int result = 0;
        	result = this.Username.CompareTo(other.Username);
        	return result;
        }
        
        public override bool Equals(object other)
        {
        	if(ReferenceEquals(other, null))
        		return false;
        
        	return Equals(other as IBotUser);
        }
        
        public bool Equals(IBotUser other)
        {
        	if(ReferenceEquals(other, null))
        		return false;
        
        	return
                EqualityComparer<String>.Default.Equals(this.Username, other.Username);
        	 ;
        }
        
        public override int GetHashCode()
        {
        	unchecked
        	{
        		int hash = 17;
                hash = hash * 23 + EqualityComparer<String>.Default.GetHashCode(this.Username);
        		return hash;
        	}
        }
        
        public override String ToString()
        {
        	return this.Username;
        }
    }
}
