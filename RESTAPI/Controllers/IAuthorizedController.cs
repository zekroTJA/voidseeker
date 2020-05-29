using RESTAPI.Authorization;

namespace RESTAPI.Controllers
{
    /// <summary>
    /// Provides functionalities to get and set the 
    /// AuthClaims of an authenticated user.
    /// </summary>
    public interface IAuthorizedController
    {
        /// <summary>
        /// Set an AuthClaims instance to the controller
        /// instance.
        /// </summary>
        /// <param name="claims">AuthClaims instance</param>
        void SetAuthClaims(AuthClaims claims);

        /// <summary>
        /// Returns the currently set AuthClaims of the
        /// controller instance.
        /// </summary>
        /// <returns>AuthClaoms of an authenticated user</returns>
        AuthClaims GetAuthClaims();
    }

}
