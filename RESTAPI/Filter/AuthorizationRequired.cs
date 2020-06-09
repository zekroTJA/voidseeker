using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESTAPI.Authorization;
using RESTAPI.Controllers;
using RESTAPI.Database;
using RESTAPI.Models;
using System.Threading.Tasks;

namespace RESTAPI.Filter
{
    /// <summary>
    /// TypeFilter which tries to the a session key form the incomming
    /// request and tries to validate the reuqets with the injected
    /// Authorization module. The AuthClaims of the authorized
    /// user is then set to the controller instance.
    /// 
    /// If this filter is added and the authorization fails, a
    /// 401 Unauthorized exception is returned as response.
    /// </summary>
    public class AuthorizationRequired : ActionFilterAttribute
    {
        private readonly IAuthorization authorization; // Gets injected by DI
        private readonly IDatabaseAccess database;     // Gets injected by DI

        public AuthorizationRequired(IAuthorization _authorization, IDatabaseAccess _database)
        {
            authorization = _authorization;
            database = _database;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            var controller = ctx.Controller as IAuthorizedController;

            var ok = ctx.HttpContext.Request.Cookies.TryGetValue(Constants.SESSION_COOKIE_NAME, out var token);
            if (controller == null || !ok || token == null)
            {
                SetUnauthorized(ctx);
                return;
            }

            try
            {
                var claims = authorization.ValidateSessionKey(token);
                claims.User = await database.Get<UserModel>(claims.UserUid);
                controller.SetAuthClaims(claims);
            }
            catch
            {
                SetUnauthorized(ctx);
                return;
            }

            await base.OnActionExecutionAsync(ctx, next);
        }

        private void SetUnauthorized(ActionExecutingContext ctx)
        {
            var result = new ObjectResult(null);
            result.StatusCode = 401;
            ctx.Result = result;
        }
    }

}
