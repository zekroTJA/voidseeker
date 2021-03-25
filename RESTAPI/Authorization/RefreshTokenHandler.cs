using Microsoft.Extensions.Configuration;
using RESTAPI.Database;
using RESTAPI.Models;
using RESTAPI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTAPI.Authorization
{
    public class RefreshTokenHandler : IRefreshTokenHandler
    {
        private readonly IDatabaseAccess database;

        public RefreshTokenHandler(IDatabaseAccess _database)
        {
            database = _database;
        }

        public async Task<DeadlinedToken> GenerateAsync<T>(T ident, TimeSpan lifetime) where T : AuthClaims
        {
            var token = new DeadlinedToken()
            {
                Token = CryptoRandomUtil.GetBase64String(64),
                Deadline = DateTime.Now.Add(lifetime),
            };

            var dbToken = await database.GetRefreshTokenByUserUid(ident.UserUid);
            Func<RefreshTokenModel, Task> addOrUpdate = database.Update;

            if (dbToken == null)
            {
                dbToken = new RefreshTokenModel(token, ident.UserUid);
                addOrUpdate = database.Put;
            }

            await addOrUpdate(dbToken);

            return token;
        }

        public async Task<T> ValidateAndRestoreAsync<T>(string token) where T : AuthClaims
        {
            var dbToken = await database.GetRefreshTokenByToken(token);
            if (dbToken == null) return null;
            if (dbToken.ToDeadlinedToken().IsExpired()) return null;

            var user = await database.Get<UserModel>(dbToken.UserUid);
            if (user == null) return null;

            return new AuthClaims()
            {
                UserUid = dbToken.UserUid,
                UserName = user.UserName,
                User = user,
            } as T;
        }
    }
}
