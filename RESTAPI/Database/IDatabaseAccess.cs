using RESTAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RESTAPI.Database
{
    /// <summary>
    /// Provides access functionalities to a database connection.
    /// </summary>
    public interface IDatabaseAccess
    {
        /// <summary>
        /// Add an object to the database index.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="obj">object instance</param>
        /// <returns></returns>
        Task Put<T>(T obj) where T : UniqueModel;

        /// <summary>
        /// Fetch an object from database by its uid.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="uid">object uid</param>
        /// <returns></returns>
        Task<T> Get<T>(Guid uid) where T : UniqueModel, new();

        /// <summary>
        /// Removes an object from the database by its uid.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="uid">object uid</param>
        /// <returns></returns>
        Task Delete<T>(Guid uid) where T : UniqueModel, new();

        /// <summary>
        /// Fully updates the object in the database by
        /// the objects uid.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="obj">object instance</param>
        /// <returns></returns>
        Task Update<T>(T obj) where T : UniqueModel;

        /// <summary>
        /// Fetches the count of objects in the database
        /// of the specified object type.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <returns></returns>
        Task<long> Count<T>() where T : UniqueModel, new();

        /// <summary>
        /// Fetches the count of objects in the database
        /// of the specified object type where the specified
        /// field has the specified exact value.
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="field">field name</param>
        /// <param name="value">field value</param>
        /// <returns></returns>
        Task<long> Count<T>(string field, string value) where T : UniqueModel, new();


        /// <summary>
        /// Fetches an <see cref="UserModel"/> by <see cref="UserModel.UserName"/>.
        /// Returns null if no user was found.
        /// </summary>
        /// <param name="username">username</param>
        /// <returns></returns>
        Task<UserModel?> GetUserByUserName(string username);

        /// <summary>
        /// Fetches a <see cref="TagModel"/> by <see cref="TagModel.Name"/>.
        /// Returns null if no tag was found.
        /// </summary>
        /// <param name="name">tag name</param>
        /// <returns></returns>
        Task<TagModel?> GetTagByName(string name);

        /// <summary>
        /// Fetches an <see cref="ImageModel"/> by <see cref="ImageModel.Md5Hash"/>
        /// owned by the specified user by uid.
        /// Returns null if no image was found.
        /// </summary>
        /// <param name="hash">hash of the image</param>
        /// <param name="ownerUid">uid of the owner</param>
        /// <returns></returns>
        Task<ImageModel?> GetImageByHash(string hash, Guid ownerUid);


        /// <summary>
        /// Fetches a list of users matching the specified
        /// filter string.
        /// </summary>
        /// <param name="offset">results offset</param>
        /// <param name="size">results size</param>
        /// <param name="filter">filter string</param>
        /// <returns></returns>
        Task<List<UserModel>> SearchUsers(int offset, int size, string filter);

        /// <summary>
        /// Fetches a list of images matching the specified
        /// filter properties.
        /// 
        /// Only images which are either owned by the specified
        /// ownerUid or marked as public (when includePublic is
        /// true) are listed. When includeExplicit is false, images
        /// marked as explicit must not be included.
        /// 
        /// Image results are sorted by sortBy and ordered 
        /// ascending if ascending is true, else descending.
        /// </summary>
        /// <param name="offset">results offset</param>
        /// <param name="size">results size</param>
        /// <param name="filter">filter string</param>
        /// <param name="exclude">excluded tags</param>
        /// <param name="ownerUid">owner uid</param>
        /// <param name="includePublic">include public images</param>
        /// <param name="includeExplicit">include explicit images</param>
        /// <param name="sortBy">sort by field name</param>
        /// <param name="ascending">ascending or descending</param>
        /// <returns></returns>
        Task<List<ImageModel>> SearchImages(
            int offset, int size, string filter, string[] exclude, Guid ownerUid, bool includePublic = false,
            bool includeExplicit = false, string sortBy = "created", bool ascending = false);

        /// <summary>
        /// Fetch tags by specified filter string and
        /// with defined search fuzziness.
        /// </summary>
        /// <param name="offset">results offset</param>
        /// <param name="size">results size</param>
        /// <param name="filter">filter string</param>
        /// <param name="fuzziness">fuzziness value</param>
        /// <returns></returns>
        Task<List<TagModel>> SearchTags(int offset, int size, string filter, int fuzziness = -1);
    }
}
