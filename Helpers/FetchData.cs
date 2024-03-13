
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Helpers
{
    public class FetchData
    {
        private string BaseUrl { get; set; }
        private string Token { get; set; }
        public FetchData(string baseUrl, string token = "")
        {
            BaseUrl = baseUrl;
            Token = token;
        }
        public async Task<APIResponse> FetchAsync(string url, object? queries = null, object? body = null, Method method = Method.Get)
        {
            try
            {
                var authenticator = new JwtAuthenticator(Token);
                var options = new RestClientOptions(BaseUrl)
                {
                    Authenticator = authenticator
                };
                var client = new RestClient(options);
                var request = new RestRequest(url, method);
                if (queries != null)
                {
                    request.AddObject(queries);
                }
                if (body != null)
                {
                    request.AddJsonBody(body);
                }
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new APIResponse
                    {
                        success = false,
                        message = "Error occurred while getting data from API",
                        status = response.StatusCode.ToString()
                    };
                }
                if (string.IsNullOrEmpty(response.Content))
                {
                    return new APIResponse
                    {
                        success = false,
                        message = "Cannot get data from API",
                        status = HttpStatusCode.NotFound.ToString()
                    };
                }
                var responseData = JsonSerializer.Deserialize<APIResponse>(response.Content);
                if (responseData?.success == false)
                {
                    return new APIResponse
                    {
                        success = false,
                        message = "Error occurred while getting data from API",
                        status = HttpStatusCode.InternalServerError.ToString()
                    };
                }
                return responseData ?? throw new Exception("Cannot get data from API");
            }
            catch (Exception e)
            {
                return new APIResponse
                {
                    success = false,
                    message = e.Message,
                    status = HttpStatusCode.InternalServerError.ToString()
                };
            }
        }
    }
}