using RESTAPI.Cache;
using RESTAPI.Models;
using System;

namespace RESTAPI.Extensions
{
    public static class CacheExtension
    {
        public static void Put<T>(this ICacheProvider cache, T value, TimeSpan lifeTime) where T : EntityModel =>
            cache.Put(GetKey(value), value, lifeTime);

        public static bool TryGet<T>(this ICacheProvider cache, Guid uid, out T value) where T : EntityModel, new() =>
            cache.TryGet(GetKey<T>(uid), out value);

        public static bool TryRefresh<T>(this ICacheProvider cache, Guid uid, TimeSpan lifeSpanAdded) where T : EntityModel, new() =>
            cache.TryRefresh(GetKey<T>(uid), lifeSpanAdded);

        public static void Delete<T>(this ICacheProvider cache, Guid uid) where T : EntityModel, new() =>
            cache.Delete(GetKey<T>(uid));

        public static bool Contains<T>(this ICacheProvider cache, Guid uid) where T : EntityModel, new() =>
            cache.Contains(GetKey<T>(uid));

        private static string GetKey<T>(T entity) where T : EntityModel =>
            $"{entity.Index}:{entity.Uid}";

        private static string GetKey<T>(Guid uid) where T : EntityModel, new() =>
            GetKey(new T() { Uid = uid });
    }
}
