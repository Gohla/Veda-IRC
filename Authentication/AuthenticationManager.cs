using System;
using System.Collections.Generic;
using NLog;
using ReactiveIRC.Interface;
using Veda.Authentication.Configuration;
using Veda.Interface;
using Veda.Storage;

namespace Veda.Authentication
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private static readonly Logger _logger = LogManager.GetLogger("AuthenticationManager");

        private readonly IStorageManager _storageManager;
        private readonly Dictionary<String, IBotUser> _users = new Dictionary<String, IBotUser>();
        private readonly Dictionary<String, IBotGroup> _groups = new Dictionary<String, IBotGroup>();
        private readonly Dictionary<IUser, IBotUser> _identifiedUsers = new Dictionary<IUser, IBotUser>();
        private readonly Dictionary<IUser, IBotUser> _guests = new Dictionary<IUser, IBotUser>();

        private Dictionary<String, UserData> _userData;

        public IBotGroup Guest { get; private set; }
        public IBotGroup Registered { get; private set; }
        public IBotGroup Administrator { get; private set; }
        public IBotGroup Owner { get; private set; }

        public AuthenticationManager(IStorageManager storageManager)
        {
            _storageManager = storageManager;

            Guest = new BotGroup(GroupNames.Guest);
            Registered = new BotGroup(GroupNames.Registered);
            Administrator = new BotGroup(GroupNames.Administrator);
            Owner = new BotGroup(GroupNames.Owner);

            _groups.Add(Guest.Name, Guest);
            _groups.Add(Registered.Name, Registered);
            _groups.Add(Administrator.Name, Administrator);
            _groups.Add(Owner.Name, Owner);

            _userData = _storageManager.Global().GetOrCreate<Dictionary<String, UserData>>("Users");
            foreach(UserData data in _userData.Values)
            {
                if(!_groups.ContainsKey(data.GroupName))
                {
                    _logger.Error("User with username " + data.Username + " has invalid group name " + data.GroupName
                        + ". User was not added.");
                }
                else
                {
                    _users.Add(data.Username, new BotUser(data.Username, data.Password, _groups[data.GroupName]));
                }
            }
        }

        public void Dispose()
        {

        }

        public IBotUser Register(IUser user, String username, String password)
        {
            if(IsIdentified(user))
                throw new InvalidOperationException("User is already identified.");

            if(_users.ContainsKey(username))
                throw new ArgumentException("Username is already registered.", "username");

            if(String.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null, empty or whitespace.", "password");

            IBotUser botUser = new BotUser(username, password, Registered);
            _users.Add(username, botUser);
            _userData.Add(username, new UserData { Username = username, Password = password, 
                GroupName = Registered.Name });
            _identifiedUsers.Add(user, botUser);
            return botUser;
        }

        public IBotUser Identify(IUser user, String username, String password)
        {
            if(IsIdentified(user))
                throw new InvalidOperationException("User is already identified.");

            if(!_users.ContainsKey(username))
                throw new InvalidOperationException("Incorrect username or password.");

            if(String.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Incorrect username or password.");

            IBotUser botUser = _users[username];
            if(!botUser.CheckPassword(password))
                throw new InvalidOperationException("Incorrect username or password.");

            _identifiedUsers.Add(user, botUser);

            return botUser;
        }

        public IBotUser GetUser(IUser user)
        {
            if(!IsIdentified(user))
            {
                if(!_guests.ContainsKey(user))
                {
                    IBotUser guest = new BotUser("Guest", String.Empty, Guest);
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
                return _identifiedUsers[user];
            }
        }

        public IBotGroup GetGroup(String name)
        {
            if(!_groups.ContainsKey(name))
                throw new ArgumentException("Group with name " + name + " does not exist.", "name");
            return _groups[name];
        }

        public bool IsIdentified(IUser user)
        {
            return _identifiedUsers.ContainsKey(user);
        }
    }
}
