
namespace VinhUni_Educator_API.Utils
{
    public class APIResponse
    {
        public bool success { get; set; }
        public string? error { get; set; }
        public string? message { get; set; }
        public string? status { get; set; }
        public int? totalRecord { get; set; }
        public string? correlationId { get; set; }
        public string? errorCheckExist { get; set; }
        public string? errorControlId { get; set; }
        public string? errorFormId { get; set; }
        public string? errorInFile { get; set; }
        public string? debugMessage { get; set; } = null;
        public dynamic? data { get; set; }
    }
}