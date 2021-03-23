using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTAPI.Extensions
{
    public static class HttpRequestExtension
    {
        private const string VALUE_PREFIX = "accesstoken ";

        public static bool ExtractRefreshToken(this HttpRequest request, out string token)
        {
            token = null;

            if (!request.Cookies.TryGetValue(Constants.REFRESH_TOKEN_COOKIE, out var cookieValue)
                || string.IsNullOrWhiteSpace(cookieValue))
                return false;

            token = cookieValue;
            return true;
        }

        public static bool ExtractAccessToken(this HttpRequest request, out string token)
        {
            token = null;

            if (!request.Headers.TryGetValue("Authorization", out var headerValue)
                || headerValue.ToString().Length <= VALUE_PREFIX.Length
                || !headerValue.ToString().ToLower().StartsWith(VALUE_PREFIX))
                return false;

            token = headerValue.ToString()[VALUE_PREFIX.Length..];
            return true;
        }
    }
}
