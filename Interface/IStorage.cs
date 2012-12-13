﻿using System;

namespace Veda.Interface
{
    public interface IStorage : IDisposable
    {
        /// <summary>
        /// Opens the storage at given file. If file does not exist, it is created.
        /// Must be called before calling LoadObject, SaveObject or ClearObject.
        /// On dispose, saved objects will be stored in this file again.
        /// </summary>
        ///
        /// <param name="file">The file.</param>
        void Open(String file);

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
        T Get<T>(String id);

        /// <summary>
        /// Gets an object located at given identifier, or creates a default instance if it does not exist.
        /// </summary>
        ///
        /// <typeparam name="T">Object type to retrieve.</typeparam>
        /// <param name="id">The identifier.</param>
        ///
        /// <returns>
        /// Retrieved or created object.
        /// </returns>
        T GetOrCreate<T>(String id) where T : new();

        /// <summary>
        /// Query if an object exists at given identifier.
        /// </summary>
        ///
        /// <param name="id">   The identifier. </param>
        ///
        /// <returns>
        /// True if object exists, false otherwise.
        /// </returns>
        bool Exists(String id);

        /// <summary>
        /// Sets an object located at given identifier.
        /// </summary>
        ///
        /// <typeparam name="T">Object type to save.</typeparam>
        /// <param name="id"> The identifier.</param>
        /// <param name="obj">The object to save.</param>
        void Set<T>(String id, T obj);

        /// <summary>
        /// Remove the object at given identifier.
        /// </summary>
        ///
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="id">The identifier.</param>
        void Remove<T>(String id);
    }
}
