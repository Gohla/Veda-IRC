using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;

namespace Veda.Authentication.Configuration
{
    public class UserData
    {
        public String Username { get; set; }
        public String Password { get; set; }
        public String GroupName { get; set; }
        public IList<IdentityMask> Masks { get; set; }

        public UserData()
        {
            Masks = new List<IdentityMask>();
        }
    }
}
