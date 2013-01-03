using System;
using System.Collections.Generic;
using System.Linq;
using Gohla.Shared;
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
        private readonly Dictionary<String, IBotGroup> _groups = new Dictionary<String, IBotGroup>(
            StringComparer.OrdinalIgnoreCase);

        private readonly MultiValueDictionary<IBotUser, IdentityMask> _masks =
            new MultiValueDictionary<IBotUser, IdentityMask>();
        private readonly HashSet<IdentityMask> _allMasks;

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

            Guest = new BotGroup(GroupNames.Guest, 0);
            Registered = new BotGroup(GroupNames.Registered, 10);
            Administrator = new BotGroup(GroupNames.Administrator, 20);
            Owner = new BotGroup(GroupNames.Owner, 30);

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
                    IBotUser user = new BotUser(data.Username, data.Password, _groups[data.GroupName], false);
                    _users.Add(data.Username, user);
                    _masks.Set(user, data.Masks);
                }
            }

            _allMasks = new HashSet<IdentityMask>(_masks.Values);
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
            _userData.Add(username, new UserData
            {
                Username = botUser.Username, 
                Password = botUser.HashedPassword,
                GroupName = botUser.Group.Name
            });
            _identifiedUsers.Add(user, botUser);
            return botUser;
        }

        public void Unregister(IUser user, IBotUser botUser, String username, String password)
        {
            if(!botUser.Username.Equals(username))
                throw new ArgumentException("Incorrect username.", "username");

            if(!botUser.CheckPassword(password))
                throw new ArgumentException("Incorrect password.", "password");

            _identifiedUsers.Remove(user);
            _users.Remove(botUser.Username);
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

        public void Unidentify(IUser user)
        {
            if(!IsIdentified(user))
                throw new InvalidOperationException("User is not identified.");

            _identifiedUsers.Remove(user);
        }

        public IBotUser GetUser(IUser user)
        {
            if(!IsIdentifiedSkipMasks(user))
            {
                IBotUser maskUser = TryIdentityMask(user);
                if(maskUser != null)
                {
                    return maskUser;
                }

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

        public IBotUser GetUser(String name)
        {
            if(!_users.ContainsKey(name))
                throw new ArgumentException("User with name " + name + " does not exist.", "user");

            return _users[name];
        }

        public IBotGroup GetGroup(String name)
        {
            if(!_groups.ContainsKey(name))
                throw new ArgumentException("Group with name " + name + " does not exist.", "name");
            return _groups[name];
        }

        public bool IsIdentified(IUser user)
        {
            IBotUser maskUser = TryIdentityMask(user);
            if(maskUser != null)
                return true;

            return IsIdentifiedSkipMasks(user);
        }

        public void AddMask(IBotUser user, IdentityMask mask)
        {
            // TODO: What about conflicting masks?
            if(_allMasks.Contains(mask))
                throw new ArgumentException("Identity mask " + mask + " already exists.", "mask");

            _masks.Add(user, mask);
            _userData[user.Username].Masks.Add(mask);
            _allMasks.Add(mask);
        }

        public void RemoveMask(IBotUser user, IdentityMask mask)
        {
            if(!_allMasks.Contains(mask))
                throw new ArgumentException("Identity mask " + mask + " does not exists.", "mask");

            _masks.Remove(user, mask);
            _userData[user.Username].Masks.Remove(mask);
            _allMasks.Remove(mask);
        }

        public IEnumerable<IdentityMask> Masks(IBotUser user)
        {
            return _masks[user];
        }

        private bool IsIdentifiedSkipMasks(IUser user)
        {
            return _identifiedUsers.ContainsKey(user);
        }

        private IBotUser TryIdentityMask(IUser user)
        {
            // TODO: Expensive!
            return _masks
                .Where(p => p.Value.Match(user.Identity))
                .Select(p => p.Key)
                .FirstOrDefault()
                ;
        }
    }
}
