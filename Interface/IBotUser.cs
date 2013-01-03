﻿using System;
using System.Collections.Generic;
using ReactiveIRC.Interface;

namespace Veda.Interface
{
    public interface IBotUser
    {
        String Username { get; }
        String HashedPassword { get; }
        IBotGroup Group { get; }

        bool CheckPassword(String password);
    }
}
