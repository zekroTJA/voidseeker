using System;
using System.Threading.Tasks;

namespace RESTAPI.Authorization
{
    public interface IRefreshTokenHandler
    {
        Task<DeadlinedToken> GenerateAsync<T>(T ident, TimeSpan lifetime) where T : AuthClaims;
        Task<T> ValidateAndRestoreAsync<T>(string token) where T : AuthClaims;
    }
}