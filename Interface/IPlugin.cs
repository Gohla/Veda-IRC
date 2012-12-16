using System;
using System.Collections.Generic;
using Gohla.Shared;

namespace Veda.Interface
{
    public interface IPlugin : IDisposable, IEquatable<IPlugin>
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        String Name { get; }
        /// <summary>
        /// Gets the description, used to display help.
        /// </summary>
        String Description { get; }
        /// <summary>
        /// Gets the commands that should be added to the command manager initially.
        /// </summary>
        IEnumerable<ICommand> InitialCommands { get; }

        /// <summary>
        /// Gets the type of the plugin. Can be null.
        /// </summary>
        Type Type { get; }
        /// <summary>
        /// Gets the instance. Can be null.
        /// </summary>
        object Instance { get; }

        /// <summary>
        /// Gets the loaded action to invoke when the plugin is loaded. Can be null.
        /// </summary>
        Action<IPlugin, IBot> Loaded { get; }
    }
}
