using System;

namespace RESTAPI.Cache
{
    /// <summary>
    /// Provides simple key-value caching
    /// with value expiration.
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Add or set a value by key in the
        /// cache with a defined lifetime.
        /// </summary>
        /// <typeparam name="T">value type</typeparam>
        /// <param name="key">key string</param>
        /// <param name="value">associated value</param>
        /// <param name="lifeTime">value life time</param>
        void Put<T>(string key, T value, TimeSpan lifeTime);

        /// <summary>
        /// Returns true when the cache contains
        /// the specified key, regardless of
        /// life time.
        /// </summary>
        /// <param name="key">key string</param>
        /// <returns></returns>
        bool Contains(string key);

        /// <summary>
        /// Tries to get a value from the cache by key.
        /// Returns false, when key was not found or
        /// value has been expired. Otherwise, the value
        /// will be set to the out reference of value.
        /// </summary>
        /// <typeparam name="T">value type</typeparam>
        /// <param name="key">key string</param>
        /// <param name="value">value reference</param>
        /// <returns></returns>
        bool TryGet<T>(string key, out T value);

        /// <summary>
        /// Tries to extend the lifetime of the desired
        /// key-value pair, regardeless of expiration.
        /// The specified TimeSpan will be added to the
        /// date of expiration.
        /// Returns false if no key was found.
        /// </summary>
        /// <param name="key">key string</param>
        /// <param name="lifeSpanAdded">duration to extend life span</param>
        /// <returns></returns>
        bool TryRefresh(string key, TimeSpan lifeSpanAdded);

        /// <summary>
        /// Deletes the desired key-value pair, regardeless
        /// if the key exists. If the key exists, it will be
        /// definetly removed after.
        /// </summary>
        /// <param name="key">key string</param>
        void Delete(string key);
    }
}
