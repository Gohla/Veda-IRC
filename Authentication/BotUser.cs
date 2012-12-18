using System;
using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda.Authentication
{
    public class BotUser : IBotUser
    {
        private String _password;

        public String Username { get; private set; }
        public IUser User { get; private set; }
        public IBotGroup Group { get; private set; }

        public BotUser(String username, String password, IUser user, IBotGroup group)
        {
            Username = username;
            _password = password;
            User = user;
            Group = group;
        }

        public bool CheckPassword(String password)
        {
            return _password.Equals(password);
        }
    }
}
