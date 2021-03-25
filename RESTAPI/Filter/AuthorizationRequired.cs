using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESTAPI.Authorization;
using RESTAPI.Cache;
using RESTAPI.Controllers;
using RESTAPI.Database;
using RESTAPI.Extensions;
using RESTAPI.Models;
using System;
using System.Threading.Tasks;

namespace RESTAPI.Filter
{
    /// <summary>
    /// ActionFilter which tries to the a session key form the incomming
    /// request and tries to validate the reuqets with the injected
    /// Authorization module. The <see cref="AuthClaims"/> of the authorized
    /// user is then set to the <see cref="IAuthorizedController"/> instance.
    /// 
    /// If this filter is added and the authorization fails, a
    /// 401 Unauthorized exception is returned as response.
    /// </summary>
    public class AuthorizationRequired : ActionFilterAttribute
    {
        private static readonly TimeSpan USER_CACHE_TIME = TimeSpan.FromHours(1);

        private readonly IAccessTokenHandler accessTokenhandler; // Gets injected by DI
        private readonly IDatabaseAccess database;
        private readonly ICacheProvider cache;

        public AuthorizationRequired(
            IAccessTokenHandler _accessTokenHandler, 
            IDatabaseAccess _database,
            ICacheProvider _cache)
        {
            accessTokenhandler = _accessTokenHandler;
            database = _database;
            cache = _cache;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            var controller = ctx.Controller as IAuthorizedController;

            if (controller == null
                || !ctx.HttpContext.Request.ExtractAccessToken(out var accessToken)
                || !accessTokenhandler.ValidateAndRestore<AuthClaims>(accessToken, out var identity))
            {
                SetUnauthorized(ctx);
                return;
            }

            if (!cache.TryGet<UserModel>(identity.UserUid, out var user))
            {
                user = await database.Get<UserModel>(identity.UserUid);
                if (user == null)
                {
                    SetUnauthorized(ctx);
                    return;
                }
                cache.Put(user, USER_CACHE_TIME);
            }

            identity.User = user;
            controller.SetAuthClaims(identity);

            await base.OnActionExecutionAsync(ctx, next);
        }

        private static void SetUnauthorized(ActionExecutingContext ctx) =>
            ctx.Result = (ctx.Controller as ControllerBase)?.Unauthorized(Constants.INVALID_ACCESS_TOKEN);
    }

}
