using System;
using ReactiveIRC.Interface;
using Veda.Interface;
using Veda.Storage;

namespace Veda.Authentication
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private IStorageManager _storageManager;

        public AuthenticationManager(IStorageManager storageManager)
        {
            _storageManager = storageManager;
        }

        public IBotUser Register(IUser user, String username, String password)
        {
            throw new NotImplementedException();
        }

        public IBotUser Authenticate(IUser user, String username, String password)
        {
            throw new NotImplementedException();
        }

        public IBotUser GetUser(IUser user)
        {
            throw new NotImplementedException();
        }
    }
}
