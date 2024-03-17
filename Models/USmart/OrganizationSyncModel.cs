
namespace VinhUni_Educator_API.Models.USmart
{
    public class OrganizationSyncModel
    {
        public int id { get; set; }
        public string code { get; set; } = null!;
        public int? order { get; set; }
        public string name { get; set; } = null!;
    }
}