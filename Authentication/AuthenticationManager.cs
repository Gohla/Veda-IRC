using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;
using Veda.Interface;
using Veda.Storage;

namespace Veda.Authentication
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IStorageManager _storageManager;
        private Dictionary<String, IBotUser> _users;
        private Dictionary<IUser, IBotUser> _authenticatedUsers = new Dictionary<IUser, IBotUser>();
        private Dictionary<IUser, IBotUser> _guests = new Dictionary<IUser, IBotUser>();

        public IBotGroup Guest { get; private set; }
        public IBotGroup Registered { get; private set; }
        public IBotGroup Administrator { get; private set; }
        public IBotGroup Owner { get; private set; }

        public AuthenticationManager(IStorageManager storageManager)
        {
            _storageManager = storageManager;

            _users = _storageManager.Global().GetOrCreate<Dictionary<String, IBotUser>>("Users");

            Guest = new BotGroup("Guest");
            Registered = new BotGroup("Registered");
            Administrator = new BotGroup("Administrator");
            Owner = new BotGroup("Owner");
        }

        public void Dispose()
        {

        }

        public IBotUser Register(IUser user, String username, String password)
        {
            if(_users.ContainsKey(username))
                throw new ArgumentException("Username is already registered.", "username");

            if(String.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null, empty or whitespace.", "password");

            IBotUser botUser = new BotUser(username, password, user, Registered);
            _users.Add(username, botUser);
            _authenticatedUsers.Add(user, botUser);
            return botUser;
        }

        public IBotUser Authenticate(IUser user, String username, String password)
        {
            if(_authenticatedUsers.ContainsKey(user))
                throw new InvalidOperationException("User is already authenticated.");

            if(!_users.ContainsKey(username))
                throw new InvalidOperationException("Incorrect username or password.");

            if(String.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Incorrect username or password.");

            IBotUser botUser = _users[username];
            if(!botUser.CheckPassword(password))
                throw new InvalidOperationException("Incorrect username or password.");

            _authenticatedUsers.Add(user, botUser);

            return botUser;
        }

        public IBotUser GetUser(IUser user)
        {
            if(!_authenticatedUsers.ContainsKey(user))
            {
                if(!_guests.ContainsKey(user))
                {
                    IBotUser guest = new BotUser("Guest", String.Empty, user, Guest);
                    _guests.Add(user, guest);
                    return guest;
                }
                else
                {
                    return _guests[user];
                }
            }
            else
            {
                return _authenticatedUsers[user];
            }
        }
    }
}
