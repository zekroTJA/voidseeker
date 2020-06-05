﻿using RESTAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RESTAPI.Database
{
    public interface IDatabaseAccess
    {
        Task Put<T>(T obj) where T : UniqueModel;
        Task<T> Get<T>(Guid uid) where T : UniqueModel, new();
        Task Delete<T>(Guid uid) where T : UniqueModel, new();
        Task Update<T>(T obj) where T : UniqueModel;
        Task<long> Count<T>() where T : UniqueModel, new();
        Task<long> Count<T>(string field, string value) where T : UniqueModel, new();

        Task<UserModel?> GetUserByUserName(string username);
        Task<TagModel?> GetTagByName(string name);
        Task<ImageModel?> GetImageByHash(string hash, Guid ownerId);

        Task<List<UserModel>> SearchUsers(int offset, int size, string filter);
        Task<List<ImageModel>> SearchImages(
            int offset, int size, string filter, string[] exclude, Guid ownerId, bool includePublic = false,
            bool includeExplicit = false, string sortBy = "created", bool ascending = false);
        Task<List<TagModel>> SearchTags(int offset, int size, string filter, int fuzziness = -1);
    }
}
