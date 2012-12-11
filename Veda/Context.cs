﻿using ReactiveIRC.Interface;
using Veda.Interface;

namespace Veda
{
    internal class Context : IContext
    {
        public IBot Bot { get; set; }
        public IStorage Storage { get; set; }
        public IReceiveMessage Message { get; set; }
    }
}
