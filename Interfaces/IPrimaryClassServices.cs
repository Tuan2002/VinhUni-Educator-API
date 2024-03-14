
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IPrimaryClassServices
    {
        Task<ActionResponse> SyncPrimaryClassesAsync();
    }
}