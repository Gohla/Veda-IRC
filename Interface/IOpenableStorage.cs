using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veda.Interface
{
    public interface IOpenableStorage : IStorage
    {
        /// <summary>
        /// Opens the storage at given file. If file does not exist, it is created.
        /// Must be called before calling any IStorage methods.
        /// On dispose, saved objects will be stored in this file again.
        /// </summary>
        ///
        /// <param name="file">The file.</param>
        void Open(String file);
    }
}
