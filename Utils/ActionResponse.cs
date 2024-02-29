
namespace VinhUni_Educator_API.Utils
{
    public class ActionResponse
    {
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}