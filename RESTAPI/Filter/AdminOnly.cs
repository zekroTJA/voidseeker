using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESTAPI.Controllers;

namespace RESTAPI.Filter
{
    /// <summary>
    /// ActiolFilter for an <see cref="IAuthorizedController"/> which
    /// tries to retrieve the <see cref="Authorization.AuthClaims"/>
    /// from the controller instance. If the hydraded user is an admin,
    /// the reuqest will action will be executed. Otherwise, the request
    /// is canceled and a 401 Unauthorized response if returned.
    /// </summary>
    public class AdminOnly : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            var controller = ctx.Controller as IAuthorizedController;

            var claims = controller.GetAuthClaims();
            if (claims == null || claims.User == null || !claims.User.IsAdmin.Equals(true))
            {
                SetUnauthorized(ctx);
                return;
            }

            base.OnActionExecuting(ctx);
        }

        private void SetUnauthorized(ActionExecutingContext ctx)
        {
            var result = new ObjectResult(null);
            result.StatusCode = 401;
            ctx.Result = result;
        }
    }
}
