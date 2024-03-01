
using System.Dynamic;

namespace VinhUni_Educator_API.Helpers
{
    public class URLParser
    {
        public dynamic? ParseFragments(string? urlString)
        {
            if (string.IsNullOrEmpty(urlString))
            {
                return null;
            }
            dynamic urlFragments = new ExpandoObject();
            // Extracting the fragment part of the URL
            int fragmentIndex = urlString.IndexOf('#');
            if (fragmentIndex >= 0)
            {
                string fragment = urlString.Substring(fragmentIndex + 1);

                // Splitting the fragment into key-value pairs
                string[] keyValuePairs = fragment.Split('&');
                foreach (string pair in keyValuePairs)
                {
                    string[] keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0];
                        string value = keyValue[1];
                        // Adding key-value pairs to the dynamic object
                        ((IDictionary<string, object>)urlFragments)[key] = value;
                    }
                }
            }
            return urlFragments;
        }
    }
}