using System;
using System.Collections.Concurrent;
using System.Timers;

namespace RESTAPI.Cache
{
    /// <summary>
    /// Wraps a map value with its type, value object
    /// and expiring timespamp.
    /// </summary>
    public class CacheValue
    {
        public Type Type { get; set; }
        public object Value { get; set; }
        public DateTime Expires { get; set; }

        public CacheValue(Type type, object value, TimeSpan lifeTime)
        {
            Type = type;
            Value = value;
            Expires = DateTime.Now.Add(lifeTime);
        }

        public bool IsExpired() =>
            DateTime.Now >= Expires;

        public void Refresh(TimeSpan lifeTimeAdded) =>
            Expires.Add(lifeTimeAdded);
    }

    /// <summary>
    /// Implementation of <see cref="ICacheProvider"/> using a
    /// ConcurrentDictionary i ncombination with a cleanup timer.
    /// </summary>
    public class InternalCacheProvider : ICacheProvider
    {
        private readonly Timer cleanupTimer;
        private readonly ConcurrentDictionary<string, CacheValue> container;

        /// <summary>
        /// Create new instance of <see cref="InternalCacheProvider"/> with the
        /// specified cleanup interval time span.
        /// </summary>
        /// <param name="cleanupInterval"></param>
        public InternalCacheProvider(TimeSpan cleanupInterval)
        {
            container = new ConcurrentDictionary<string, CacheValue>();
            cleanupTimer = new Timer(cleanupInterval.TotalMilliseconds);
            cleanupTimer.Elapsed += OnCleanup;
            cleanupTimer.Start();
        }

        public void Delete(string key)
        {
            if (Contains(key))
                while (!container.TryRemove(key, out _)) ;
        }

        public void Put<T>(string key, T value, TimeSpan lifeTime) =>
            container[key] = new CacheValue(typeof(T), value, lifeTime);

        public bool Contains(string key) =>
            container.ContainsKey(key);

        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            CacheValue val;

            if (!Contains(key))
                return false;

            while (!container.TryGetValue(key, out val)) ;

            if (val.IsExpired())
            {
                Delete(key);
                return false;
            }

            if (val.Type != typeof(T))
                throw new Exception("value type differs from parameter type");

            value = (T) val.Value;
            return true;
        }

        public bool TryRefresh(string key, TimeSpan lifeSpanAdded)
        {
            if (!Contains(key))
                return false;

            if (!container.TryGetValue(key, out var val))
                return false;

            val.Refresh(lifeSpanAdded);
            return true;
        }

        /// <summary>
        /// Cleanup function called by cleanup timer.
        /// Iterates through all map key-value paris,
        /// checks their expiration date and deletes
        /// all expired key-value pairs.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void OnCleanup(object o, ElapsedEventArgs args)
        {
            foreach (var kv in container)
            {
                if (kv.Value.IsExpired())
                    Delete(kv.Key);
            }
        }
    }
}
