using System;

namespace Veda.Interface
{
    public enum Group
    {
        Guest
      , Registered
      , Administrator
      , Owner
    }

    public static class GroupNames
    {
        public const String Guest = "Guest";
        public const String Registered = "Registered";
        public const String Administrator = "Administrator";
        public const String Owner = "Owner";

        public static String Name(this Group group)
        {
            switch(group)
            {
                case Group.Guest: return Guest;
                case Group.Registered: return Registered;
                case Group.Administrator: return Administrator;
                case Group.Owner: return Owner;
                default: return Guest;
            }
        }
    }
}
