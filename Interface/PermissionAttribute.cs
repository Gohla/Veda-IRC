using System;

namespace Veda.Interface
{
    /// <summary>
    /// Attribute for setting default permissions on a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute : Attribute
    {
        /// <summary>
        /// Name of the group this permission is for. Get a group name using the GroupNames class or by calling the Name
        /// extension method on a Group enum.
        /// </summary>
        public String GroupName;
        /// <summary>
        /// If the group is allowed to invoke this command.
        /// </summary>
        public bool Allowed;
        /// <summary>
        /// The number of invocations that may be performed in the timespan.
        /// </summary>
        public ushort Limit;
        /// <summary>
        /// The limit timespan in milliseconds.
        /// </summary>
        public long Timespan;

        /// <summary>
        /// Constructor.
        /// </summary>
        ///
        /// <param name="groupName">
        ///     Name of the group this permission is for. Get a group name using the GroupNames class or by calling the Name
        ///     extension method on a Group enum.
        /// </param>
        public PermissionAttribute(String groupName)
        {
            GroupName = groupName;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        ///
        /// <param name="group">The group this permission is for.</param>
        public PermissionAttribute(Group group)
        {
            GroupName = group.Name();
        }
    }
}
