using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IAuthenticationManager
    {
        IBotUser Register(IUser user, String username, String password);
        IBotUser Authenticate(IUser user, String username, String password);

        IBotUser GetUser(IUser user);
    }
}
