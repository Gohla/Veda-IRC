using System;

namespace Veda.Interface
{
    public interface IStorage : IDisposable
    {
        /// <summary>
        /// Gets an object located at given identifier.
        /// </summary>
        ///
        /// <typeparam name="T">Object type to retrieve.</typeparam>
        /// <param name="id">The identifier.</param>
        ///
        /// <returns>
        /// Retrieved object or default(T) if it was not found.
        /// </returns>
        T Get<T>(params String[] id);

        /// <summary>
        /// Gets an object with given type located at given identifier.
        /// </summary>
        ///
        /// <param name="type">The type.</param>
        /// <param name="id">  The identifier.</param>
        ///
        /// <returns>
        /// Retrieved object or null if it was not found.
        /// </returns>
        object Get(Type type, params String[] id);

        /// <summary>
        /// Gets an object located at given identifier, or creates a new instance if it does not exist.
        /// </summary>
        ///
        /// <typeparam name="T">Object type to retrieve.</typeparam>
        /// <param name="id">The identifier.</param>
        ///
        /// <returns>
        /// Retrieved or created object.
        /// </returns>
        T GetOrCreate<T>(params String[] id) where T : new();

        /// <summary>
        /// Query if an object exists at given identifier.
        /// </summary>
        ///
        /// <param name="id">The identifier.</param>
        ///
        /// <returns>
        /// True if object exists, false otherwise.
        /// </returns>
        bool Exists(params String[] id);

        /// <summary>
        /// Sets an object located at given identifier.
        /// </summary>
        ///
        /// <param name="obj">The object to save.</param>
        /// <param name="id"> The identifier.</param>
        void Set(object obj, params String[] id);

        /// <summary>
        /// Remove the object at given identifier.
        /// </summary>
        ///
        /// <param name="id">The identifier.</param>
        ///
        /// <returns>
        /// True if object was removed, false otherwise.
        /// </returns>
        bool Remove(params String[] id);

        /// <summary>
        /// Tries to persist the storage to permanent storage.
        /// </summary>
        ///
        /// <returns>
        /// True if it succeeds, false otherwise.
        /// </returns>
        bool Persist();
    }
}
