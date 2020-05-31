using System;

namespace RESTAPI.Cache
{
    public interface ICacheProvider
    {
        void Put<T>(string key, T value, TimeSpan lifeTime);
        bool Contains(string key);
        bool TryGet<T>(string key, out T value);
        bool TryRefresh(string key, TimeSpan lifeSpanAdded);
        void Delete(string key);
    }
}
