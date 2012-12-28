using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda.Authentication
{
    public class BotUser : IBotUser
    {
        private String _password;

        public String Username { get; private set; }
        public IBotGroup Group { get; private set; }

        public BotUser(String username, String password, IBotGroup group)
        {
            Username = username;
            _password = password;
            Group = group;
        }

        public bool CheckPassword(String password)
        {
            return _password.Equals(password);
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
