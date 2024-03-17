
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IAccountServices
    {
        Task<ActionResponse> GetCurrentUser(bool skipCache = true);
    }
}