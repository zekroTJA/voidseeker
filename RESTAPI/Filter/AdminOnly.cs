﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESTAPI.Controllers;

namespace RESTAPI.Filter
{
    public class AdminOnly : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            var controller = ctx.Controller as IAuthorizedController;

            var claims = controller.GetAuthClaims();
            if (claims == null || claims.User == null || !claims.User.IsAdmin)
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
