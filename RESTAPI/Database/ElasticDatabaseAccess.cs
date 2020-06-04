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
            configuration.GetSection("Database:Elasticsearch:Nodes").Bind(nodeUrls);

            var settings = new ConnectionSettings(
                    new StaticConnectionPool(
                        nodeUrls.Select(x => new Uri(x))));

            client = new ElasticClient(settings);
                
        }

        public Task Put<T>(T obj) where T : UniqueModel => 
            client.IndexAsync(obj, idx => idx.Index(obj.Index).Id(obj.Uid));

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
            client.UpdateAsync<T, object>(obj.Uid, s => s
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

            return res?.Hits.DefaultIfEmpty(null).First()?.Source;
        }

        public async Task<TagModel?> GetTagByName(string name)
        {
            var res = await SearchOrNullAsync<TagModel>(s => s
                    .Index(new TagModel().Index)
                    .Size(1)
                    .Query(q =>
                        q.Term(t => t.Field(f => f.Name).Value(name))));

            return res?.Hits.DefaultIfEmpty(null).First()?.Source;
        }

        public async Task<List<UserModel>> SearchUsers(int offset, int size, string filter)
        {
            filter = filter.ToLower();

            ISearchResponse<UserModel> res;
            
            if (filter.NullOrEmpty())
            {
                res = await SearchMatchAll<UserModel>(offset, size);
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

        public async Task<List<ImageModel>> SearchImages(
            int offset, int size, string filter, string[] exclude, Guid ownerId, bool includePublic = false)
        {
            filter = filter.ToLower();
            
            ISearchResponse<ImageModel> res;

            if (filter.NullOrEmpty())
            {
                res = await SearchOrNullAsync<ImageModel>(s => s
                    .Index(new ImageModel().Index)
                    .Skip(offset)
                    .Size(size)
                    .Query(q => q
                        .Bool(b => b
                            .Should(m =>
                                   m.MatchPhrase(term => term.Field(f => f.OwnerUid).Query(ownerId.ToString()))
                                || (includePublic ? m.Term(term => term.Field(f => f.Public).Value(true)) : null)
                            )
                            .MustNot(m =>
                                   m.Fuzzy(match => match.Field(f => f.TagsCombined).Value(string.Join(" ", exclude).ToLower()))
                            )
                    )));
            }
            else
            {
                res = await SearchOrNullAsync<ImageModel>(s => s
                    .Index(new ImageModel().Index)
                    .Skip(offset)
                    .Size(size)
                    .Query(q => q
                        .Bool(b => b
                            .Must(m =>
                                   (
                                          m.Fuzzy(match => match.Field(f => f.TagsCombined).Value(filter).Boost(1.5))
                                       || m.Fuzzy(match => match.Field(f => f.Title).Value(filter).Boost(1.3))
                                       || m.Fuzzy(match => match.Field(f => f.Description).Value(filter).Boost(1))
                                   )
                                && (
                                          m.MatchPhrase(term => term.Field(f => f.OwnerUid).Query(ownerId.ToString()))
                                       || (includePublic ? m.Term(term => term.Field(f => f.Public).Value(true)) : null)
                                   )
                            )
                            .MustNot(m =>
                                   m.Fuzzy(match => match.Field(f => f.TagsCombined).Value(string.Join(" ", exclude).ToLower()))
                            )
                    )));
            }

            if (res == null)
                return new List<ImageModel>();

            return res.Hits.Select(x => x.Source).ToList();
        }

        public async Task<List<TagModel>> SearchTags(int offset, int size, string filter, int fuzziness = -1)
        {
            filter = filter.ToLower();

            ISearchResponse<TagModel> res;

            if (filter.NullOrEmpty())
            {
                res = await SearchMatchAll<TagModel>(offset, size);
            }
            else
            {
                var fuzz = fuzziness < 0 ? Fuzziness.Auto : Fuzziness.EditDistance(fuzziness);
                res = await SearchOrNullAsync<TagModel>(s => s
                    .Index(new TagModel().Index)
                    .Skip(offset)
                    .Size(size)
                    .Query(q =>
                           q.Fuzzy(m => m.Field(x => x.Name).Fuzziness(fuzz).Value(filter).Boost(1))
                    ));
            }

            if (res == null)
                return new List<TagModel>();

            return res.Hits.Select(x => x.Source).ToList();
        }

        // ------------------------------------------------------------------------------
        // --- HELPER FUNCTIONS ---

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

        private Task<ISearchResponse<T>?> SearchMatchAll<T>(int offset, int size) where T : UniqueModel, new() =>
            SearchOrNullAsync<T>(s => s
                .Index(new T().Index)
                .Skip(offset)
                .Size(size)
                .Query(q => q.MatchAll()));
    }
}
