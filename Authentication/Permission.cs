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
        private readonly Dictionary<String, bool> _allowed;
        private readonly Dictionary<String, ushort> _limit;
        private readonly Dictionary<String, TimeSpan> _timespan;
        private ushort _currentItemsPerTimespan = 0;
        private DateTime _nextTimeReset = DateTime.MinValue;

        public Permission(IStorage storage, String permission)
        {
            _storage = storage;
            _permission = permission;

            _allowed = _storage.GetOrCreate<Dictionary<String, bool>>(_permission, ALLOWED_QUALIFIER);
            _limit = _storage.GetOrCreate<Dictionary<String, ushort>>(_permission,  LIMIT_QUALIFIER);
            _timespan = _storage.GetOrCreate<Dictionary<String, TimeSpan>>(_permission, TIMESPAN_QUALIFIER);
        }

        public IPermission DefaultAllowed(IBotGroup group, bool allowed)
        {
            if(!_allowed.ContainsKey(group.Name))
                _allowed.Add(group.Name, allowed);

            return this;
        }

        public IPermission DefaultTimedLimit(IBotGroup group, ushort limit, TimeSpan timeSpan)
        {
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
            if(!_allowed.ContainsKey(group.Name))
                return defaultAllowed;
            if(!_allowed[group.Name])
                return false;

            if(!_limit.ContainsKey(group.Name))
                return true;

            if(DateTime.Now >= _nextTimeReset)
            {
                _nextTimeReset = DateTime.Now + _timespan[group.Name];
                _currentItemsPerTimespan = 0;
            }

            if(_currentItemsPerTimespan < _limit[group.Name])
            {
                ++_currentItemsPerTimespan;
                return true;
            }

            return false;
        }

        public void CheckThrows(IBotUser user, bool defaultAllowed = true)
        {
            IBotGroup group = user.Group;
            if(!_allowed.ContainsKey(group.Name))
            {
                if(defaultAllowed)
                    return;
                else
                    throw new InvalidOperationException("Not allowed.");
            }
            if(!_allowed[group.Name])
                throw new InvalidOperationException("Not allowed.");

            if(!_limit.ContainsKey(group.Name))
                return;

            if(DateTime.Now >= _nextTimeReset)
            {
                _nextTimeReset = DateTime.Now + _timespan[group.Name];
                _currentItemsPerTimespan = 0;
            }

            if(_currentItemsPerTimespan < _limit[group.Name])
            {
                ++_currentItemsPerTimespan;
                return;
            }

            throw new InvalidOperationException("Not allowed, too many attempts.");
        }
    }
}
