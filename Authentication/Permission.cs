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
        private readonly Dictionary<IBotUser, ushort> _currentItemsPerTimespan = new Dictionary<IBotUser, ushort>();
        private readonly Dictionary<IBotUser, DateTime> _nextTimeReset = new Dictionary<IBotUser, DateTime>();
        private readonly NullableRef<bool> _defaultAllowed = new NullableRef<bool>();
        private readonly NullableRef<ushort> _defaultLimit = new NullableRef<ushort>();
        private readonly NullableRef<TimeSpan> _defaultTimespan = new NullableRef<TimeSpan>();
        private NullableRef<bool> _allowed;
        private NullableRef<ushort> _limit;
        private NullableRef<TimeSpan> _timespan;

        public String Name { get; private set; }
        public IBotGroup Group { get; private set; }

        public bool Allowed
        {
            get
            {
                if(_allowed == null || !_allowed.HasValue)
                    return _defaultAllowed.Value;

                return _allowed.Value;
            }
            set
            {
                if(_allowed == null)
                    _allowed = _storage.Create(new NullableRef<bool>(value), Group.Name, Name, ALLOWED_QUALIFIER);
                else
                    _allowed.Value = value;
            }
        }

        public ushort Limit
        {
            get
            {
                if(_limit == null || !_limit.HasValue)
                    return _defaultLimit.Value;
                else
                    return _limit.Value;
            }
            set
            {
                if(_limit == null)
                    _limit = _storage.Create(new NullableRef<ushort>(value), Group.Name, Name, LIMIT_QUALIFIER);
                else
                    _limit.Value = value;
            }
        }

        public TimeSpan Timespan
        {
            get
            {
                if(_timespan == null || !_timespan.HasValue)
                    return _defaultTimespan.Value;
                else
                    return _timespan.Value;
            }
            set
            {
                if(_timespan == null)
                    _timespan = _storage.Create(new NullableRef<TimeSpan>(value), Group.Name, Name, TIMESPAN_QUALIFIER);
                else
                    _timespan.Value = value;
            }
        }

        private bool HasAllowed
        {
            get
            {
                return (_allowed != null && _allowed.HasValue) || _defaultAllowed.HasValue;
            }
        }

        private bool HasLimit
        {
            get
            {
                return (_limit != null && _limit.HasValue) || _defaultLimit.HasValue;
            }
        }

        public Permission(IStorage storage, String name, IBotGroup group)
        {
            _storage = storage;
            Name = name;
            Group = group;

            _allowed = _storage.Get<NullableRef<bool>>(Group.Name, Name, ALLOWED_QUALIFIER);
            _limit = _storage.Get<NullableRef<ushort>>(Group.Name, Name, LIMIT_QUALIFIER);
            _timespan = _storage.Get<NullableRef<TimeSpan>>(Group.Name, Name, TIMESPAN_QUALIFIER);
        }

        public void DefaultAllowed(bool allowed)
        {
            _defaultAllowed.Value = allowed;
        }

        public void DefaultTimedLimit(ushort limit, TimeSpan timeSpan)
        {
            _defaultLimit.Value = limit;
            _defaultTimespan.Value = timeSpan;
        }

        public bool Check(IBotUser user, bool defaultAllowed = true)
        {
            if(!HasAllowed)
                return defaultAllowed;

            if(!Allowed)
                return false;

            if(!HasLimit)
                return defaultAllowed;

            CheckLimitReset(user);

            return PeekLimit(user);
        }

        public void CheckThrows(IBotUser user, bool defaultAllowed = true)
        {
            if(!HasAllowed)
                if(defaultAllowed)
                    return;
                else
                    throw new InvalidOperationException("Not allowed.");

            if(!Allowed)
                throw new InvalidOperationException("Not allowed.");

            if(!HasLimit)
                if(defaultAllowed)
                    return;
                else
                    throw new InvalidOperationException("Not allowed.");

            CheckLimitReset(user);

            if(!PeekLimit(user))
                throw new InvalidOperationException("Not allowed, too many attempts.");
        }

        private void CheckLimitReset(IBotUser user)
        {
            TimeSpan timespan;
            if(_timespan == null || !_timespan.HasValue)
                timespan = _defaultTimespan.Value;
            else
                timespan = _timespan.Value;

            if(DateTime.Now >= _nextTimeReset.GetOrCreate(user, () => DateTime.MinValue))
            {
                _nextTimeReset[user] = DateTime.Now + timespan;
                _currentItemsPerTimespan[user] = 0;
            }
        }

        private bool PeekLimit(IBotUser user)
        {
            ushort limit;
            if(_limit == null || !_limit.HasValue)
                limit = _defaultLimit.Value;
            else
                limit = _limit.Value;

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
