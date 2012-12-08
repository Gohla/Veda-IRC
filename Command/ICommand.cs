﻿using System;

namespace Veda.Command
{
    public interface ICommand : IEquatable<ICommand>
    {
        String Name { get; }
        String Description { get; }
        Type[] ParameterTypes { get; }

        bool IsCompatible(params Type[] argumentTypes);
        bool IsPartialCompatible(params Type[] argumentTypes);
        object Call(params object[] arguments);
    }
}