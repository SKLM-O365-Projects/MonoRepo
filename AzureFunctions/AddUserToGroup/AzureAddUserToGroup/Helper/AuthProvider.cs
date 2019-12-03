using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureAddUserToGroup.Helpers
{
    public class AuthProvider 
    {

        private static HttpClient client = new HttpClient();


        // Gets an access token and its expiration date. First tries to get the token from the token cache.
        public static async Task<string> GetUserAccessTokenAsync()
        {
            var bodyData = BuildBodyData();
            var content = new StringContent(bodyData, Encoding.UTF8, "application/x-www-form-urlencoded");

            var postUrl = "https://login.microsoftonline.com/smkl.onmicrosoft.com/oauth2/v2.0/token";
            var response = await client.PostAsync(postUrl, content);

            var responseString = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseString);

            var token = GetJArrayValue(json, "access_token");
            return token;
        }

        private static string GetJArrayValue(JObject yourJArray, string key)
        {
            foreach (KeyValuePair<string, JToken> keyValuePair in yourJArray)
            {
                if (key == keyValuePair.Key)
                {
                    return keyValuePair.Value.ToString();
                }
            }
            return null;
        }

        private static string BuildBodyData()
        {
            var sb = new StringBuilder();
            sb.Append("client_id=" + HiddenConstants.ClientId);
            sb.Append("&");
            sb.Append("scope=https%3A%2F%2Fgraph.microsoft.com%2F.default");
            sb.Append("&");
            sb.Append("client_secret=" + HiddenConstants.ClientSecret);
            sb.Append("&");
            sb.Append("grant_type=client_credentials");
            return sb.ToString();
        }
    }
}