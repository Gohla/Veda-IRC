using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veda.Interface
{
    public interface IPermissionManager
    {
        T GetPermission<T>(IPlugin plugin, IBotUser user, String permission);
        void SetPermission<T>(IPlugin plugin, IBotUser user, String permission, T obj);
        void SetPermission<T>(IPlugin plugin, IBotGroup group, String permission, T obj);
    }
}
