using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Nest;
using RESTAPI.Extensions;
using RESTAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTAPI.Database
{
    public class ElasticDatabaseAccess : IDatabaseAccess
    {
        private readonly ElasticClient client;

        public ElasticDatabaseAccess(IConfiguration configuration)
        {
            var nodeUrls = new List<string>();
            configuration.GetSection("Elasticsearch:Nodes").Bind(nodeUrls);

            var settings = new ConnectionSettings(
                    new StaticConnectionPool(
                        nodeUrls.Select(x => new Uri(x))));

            client = new ElasticClient(settings);
                
        }

        public Task Put<T>(T obj) where T : UniqueModel => 
            client.IndexAsync(obj, idx => idx.Index(obj.Index).Id(obj.UID));

        public async Task<T> Get<T>(Guid uid) where T : UniqueModel, new()
        {
            var res = await client.GetAsync<T>(uid, idx => idx.Index(new T().Index));
            return res.Source;
        }

        public Task Delete<T>(Guid uid) where T : UniqueModel, new() =>
            client.DeleteAsync<T>(uid, idx => idx.Index(new T().Index));

        public async Task<long> Count<T>() where T : UniqueModel, new()
        {
            var res = await client.CountAsync<T>(idx => idx.Index(new T().Index));
            return res.Count;
        }

        public Task Update<T>(T obj) where T : UniqueModel =>
            client.UpdateAsync<T, object>(obj.UID, s => s
                .Index(obj.Index)
                .Doc(obj)
                .RetryOnConflict(3));

        public async Task<UserModel?> GetUserByUserName(string username)
        {
            var res = await SearchOrNullAsync<UserModel>(s => s
                    .Index(new UserModel().Index)
                    .Size(1)
                    .Query(q =>
                        q.Term(t => t.Field(f => f.UserName).Value(username))));

            return res?.Hits.First()?.Source;
        }

        public async Task<List<UserModel>> SearchUsers(int offset, int size, string filter)
        {
            ISearchResponse<UserModel> res;
            
            if (filter.NullOrEmpty())
            {
                res = await SearchOrNullAsync<UserModel>(s => s
                    .Index(new UserModel().Index)
                    .Skip(offset)
                    .Size(size)
                    .Query(q => q.MatchAll()));
            }
            else
            {
                res = await SearchOrNullAsync<UserModel>(s => s
                    .Index(new UserModel().Index)
                    .Skip(offset)
                    .Size(size)
                    .Query(q =>
                           q.Fuzzy(m => m.Field(x => x.UserName).Value(filter).Boost(1.5))
                        || q.Fuzzy(m => m.Field(x => x.DisplayName).Value(filter).Boost(1.2))
                        || q.Fuzzy(m => m.Field(x => x.Description).Value(filter).Boost(1.0))
                    ));
            }

            if (res == null)
                return new List<UserModel>();

            return res.Hits.Select(x => x.Source).ToList();
        }

        private async Task<ISearchResponse<T>?> SearchOrNullAsync<T>(Func<SearchDescriptor<T>, ISearchRequest> selector)
            where T : class
        {
            try
            {
                return await client.SearchAsync(selector);
            } 
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
