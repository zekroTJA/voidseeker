using RESTAPI.Models;
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
        Task<long> Count<T>() where T : UniqueModel, new();
        Task Update<T>(T obj) where T : UniqueModel;
        Task<UserModel?> GetUserByUserName(string username);
        Task<List<UserModel>> SearchUsers(int offset, int size, string filter);
    }
}
