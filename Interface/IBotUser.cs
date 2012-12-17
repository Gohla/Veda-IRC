using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veda.Interface
{
    public interface IBotUser
    {
        String Username { get; }
        IBotGroup Group { get; }
    }
}
