using RESTAPI.Database;
using RESTAPI.Models;
using System;
using System.Threading.Tasks;

namespace RESTAPI.Cache
{
    public class CacheWrapper : ICacheWrapper
    {
        private const string WRAPPER_KEY_DOMAIN = "WRAPPER";
        private static readonly TimeSpan defaultTimeSpan = TimeSpan.FromMinutes(10);

        private readonly IDatabaseAccess database;
        private readonly ICacheProvider cache;

        public CacheWrapper(IDatabaseAccess _database, ICacheProvider _cache)
        {
            database = _database;
            cache = _cache;
        }

        public async Task<TagModel> GetTagByName(string name)
        {
            var key = GetWrappedKey<TagModel>("BYNAME", name);
            if (!cache.TryGet<TagModel>(key, out var tag))
            {
                tag = await database.GetTagByName(name);
                cache.Put(key, tag, defaultTimeSpan);
            }

            return tag;
        }

        public Task PutTag(TagModel tag)
        {
            cache.Put(GetWrappedKey<TagModel>("BYNAME", tag.Name), tag, defaultTimeSpan);
            cache.Put(GetWrappedKey<TagModel>("BYID", tag.Uid.ToString()), tag, defaultTimeSpan);
            return database.Put(tag);
        }

        private string GetWrappedKey(params string[] key) =>
            $"{WRAPPER_KEY_DOMAIN}:{string.Join(':', key)}";

        private string GetWrappedKey<T>(params string[] key) where T : EntityModel, new()
        {
            var param = new string[key.Length + 1];
            param[0] = new T().Index;
            key.CopyTo(param, 1);
            return GetWrappedKey(param);
        }
    }
}
