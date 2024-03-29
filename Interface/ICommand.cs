﻿using System;
using System.Collections.Generic;

namespace Veda.Interface
{
    public interface ICommand : IEquatable<ICommand>
    {
        IPlugin Plugin { get; }
        String Name { get; }
        String Description { get; }
        Type[] ParameterTypes { get; }
        String[] ParameterNames { get; }
        PermissionAttribute[] DefaultPermissions { get; }
        bool Private { get; }

        bool IsCompatible(params Type[] argumentTypes);
        bool IsPartialCompatible(params Type[] argumentTypes);
        object Call(IContext context, params object[] arguments);
    }
}