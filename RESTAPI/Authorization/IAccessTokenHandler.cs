namespace RESTAPI.Authorization
{
    public interface IAccessTokenHandler
    {
        DeadlinedToken Generate<T>(T identity) where T : AuthClaims;
        bool ValidateAndRestore<T>(string key, out T identity) where T : AuthClaims;
    }
}