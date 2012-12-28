using System;

namespace Veda.Interface
{
    public interface IStorage : IDisposable
    {
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

    public static class StorageExtensions
    {
        /// <summary>
        /// Gets an object located at given identifier.
        /// </summary>
        ///
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="storage">The storage to act on.</param>
        /// <param name="id">     The identifier.</param>
        ///
        /// <returns>
        /// Retrieved object or null if it was not found or
        /// </returns>
        public static T Get<T>(this IStorage storage, params String[] id)
            where T : class
        {
            return storage.Get(typeof(T), id) as T;
        }

        /// <summary>
        /// Gets an object located at given identifier.
        /// </summary>
        ///
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="storage">The storage to act on.</param>
        /// <param name="id">     The identifier.</param>
        ///
        /// <returns>
        /// Retrieved object or null if it was not found.
        /// </returns>
        public static T GetCast<T>(this IStorage storage, params String[] id)
        {
            return (T)storage.Get(typeof(T), id);
        }

        /// <summary>
        /// Gets an object located at given identifier, or creates a new instance if it does not exist.
        /// </summary>
        ///
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="storage">The storage to act on.</param>
        /// <param name="id">     The identifier.</param>
        ///
        /// <returns>
        /// Retrieved or created object.
        /// </returns>
        public static T GetOrCreate<T>(this IStorage storage, params String[] id) 
            where T : class, new()
        {
            T obj = storage.Get<T>(id);
            if(obj == null)
            {
                obj = new T();
                storage.Set(obj, id);
            }
            return obj;
        }

        /// <summary>
        /// Creates an object located at given identifier and returns it.
        /// </summary>
        ///
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="storage">The storage to act on.</param>
        /// <param name="obj">    The object to store.</param>
        /// <param name="id">     The identifier.</param>
        ///
        /// <returns>
        /// The given object.
        /// </returns>
        public static T Create<T>(this IStorage storage, T obj, params String[] id)
        {
            storage.Set(obj, id);
            return obj;
        }
    }
}
