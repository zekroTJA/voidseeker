using RESTAPI.Models;
using System.Threading.Tasks;

namespace RESTAPI.Cache
{
    public interface ICacheWrapper
    {
        Task<TagModel> GetTagByName(string name);

        Task PutTag(TagModel tag);
    }
}
