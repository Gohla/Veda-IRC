using System;
using System.Collections.Generic;
using Veda.Interface;

namespace Veda.Authentication
{
    public class Permission : IPermission
    {
        private static readonly String ALLOWED_QUALIFIER = "Allowed";
        private static readonly String LIMIT_QUALIFIER = "Limit";
        private static readonly String TIMESPAN_QUALIFIER = "Timespan";

        private readonly IStorage _storage;
        private readonly String _permission;

        private readonly Dictionary<IBotUser, ushort> _currentItemsPerTimespan = new Dictionary<IBotUser, ushort>();
        private readonly Dictionary<IBotUser, DateTime> _nextTimeReset = new Dictionary<IBotUser, DateTime>();

        private readonly Dictionary<String, bool> _defaultAllowed = new Dictionary<String, bool>();
        private readonly Dictionary<String, ushort> _defaultLimit = new Dictionary<String, ushort>();
        private readonly Dictionary<String, TimeSpan> _defaultTimespan = new Dictionary<String, TimeSpan>();
        private Dictionary<String, bool> _allowed;
        private Dictionary<String, ushort> _limit;
        private Dictionary<String, TimeSpan> _timespan;

        public Permission(IStorage storage, String permission)
        {
            _storage = storage;
            _permission = permission;

            _allowed = _storage.Get<Dictionary<String, bool>>(_permission, ALLOWED_QUALIFIER);
            _limit = _storage.Get<Dictionary<String, ushort>>(_permission, LIMIT_QUALIFIER);
            _timespan = _storage.Get<Dictionary<String, TimeSpan>>(_permission, TIMESPAN_QUALIFIER);
        }

        public IPermission DefaultAllowed(IBotGroup group, bool allowed)
        {
            if(!_defaultAllowed.ContainsKey(group.Name))
                _defaultAllowed.Add(group.Name, allowed);

            return this;
        }

        public IPermission DefaultTimedLimit(IBotGroup group, ushort limit, TimeSpan timeSpan)
        {
            if(!_defaultLimit.ContainsKey(group.Name))
            {
                _defaultLimit.Add(group.Name, limit);
                _defaultTimespan.Add(group.Name, timeSpan);
            }

            return this;
        }

        public IPermission SetAllowed(IBotGroup group, bool allowed)
        {
            if(_allowed == null)
                _allowed = _storage.GetOrCreate<Dictionary<String, bool>>(_permission, ALLOWED_QUALIFIER);

            if(!_allowed.ContainsKey(group.Name))
                _allowed.Add(group.Name, allowed);

            return this;
        }

        public IPermission SetTimedLimit(IBotGroup group, ushort limit, TimeSpan timeSpan)
        {
            if(_limit == null)
            {
                _limit = _storage.GetOrCreate<Dictionary<String, ushort>>(_permission, LIMIT_QUALIFIER);
                _timespan = _storage.GetOrCreate<Dictionary<String, TimeSpan>>(_permission, TIMESPAN_QUALIFIER);
            }

            if(!_limit.ContainsKey(group.Name))
            {
                _limit.Add(group.Name, limit);
                _timespan.Add(group.Name, timeSpan);
            }

            return this;
        }

        public bool Check(IBotUser user, bool defaultAllowed = true)
        {
            IBotGroup group = user.Group;

            if(!HasAllowed(group))
                return defaultAllowed;

            if(!PeekAllowed(group))
                return false;

            if(!HasLimit(group))
                return defaultAllowed;

            CheckLimitReset(user);

            return PeekLimit(user);
        }

        public void CheckThrows(IBotUser user, bool defaultAllowed = true)
        {
            IBotGroup group = user.Group;

            if(!HasAllowed(group))
                if(defaultAllowed)
                    return;
                else
                    throw new InvalidOperationException("Not allowed.");

            if(!PeekAllowed(group))
                throw new InvalidOperationException("Not allowed.");

            if(!HasLimit(group))
                if(defaultAllowed)
                    return;
                else
                    throw new InvalidOperationException("Not allowed.");

            CheckLimitReset(user);

            if(!PeekLimit(user))
                throw new InvalidOperationException("Not allowed, too many attempts.");
        }

        private bool HasAllowed(IBotGroup group)
        {
            return (_allowed != null && _allowed.ContainsKey(group.Name)) || _defaultAllowed.ContainsKey(group.Name);
        }

        private bool PeekAllowed(IBotGroup group)
        {
            if(_allowed == null || !_allowed.ContainsKey(group.Name))
                return _defaultAllowed[group.Name];

            return _allowed[group.Name];
        }

        private bool HasLimit(IBotGroup group)
        {
            return (_limit != null && _limit.ContainsKey(group.Name)) || _defaultLimit.ContainsKey(group.Name);
        }

        private void CheckLimitReset(IBotUser user)
        {
            TimeSpan timespan;
            if(_timespan == null || !_timespan.ContainsKey(user.Group.Name))
                timespan = _defaultTimespan[user.Group.Name];
            else
                timespan = _timespan[user.Group.Name];

            if(DateTime.Now >= _nextTimeReset.GetOrCreate(user, () => DateTime.MinValue))
            {
                _nextTimeReset[user] = DateTime.Now + timespan;
                _currentItemsPerTimespan[user] = 0;
            }
        }

        private bool PeekLimit(IBotUser user)
        {
            ushort limit;
            if(_limit == null || !_limit.ContainsKey(user.Group.Name))
                limit = _defaultLimit[user.Group.Name];
            else
                limit = _limit[user.Group.Name];

            if(_currentItemsPerTimespan.GetOrCreate(user, () => (ushort)0) < limit)
            {
                ++_currentItemsPerTimespan[user];
                return true;
            }

            ++_currentItemsPerTimespan[user];
            return false;
        }
    }
}
