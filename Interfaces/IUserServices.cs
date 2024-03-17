
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IUserServices
    {
        Task<ActionResponse> SyncUserFromSSO(string token);
    }
}