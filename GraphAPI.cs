using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace SampleCertCall
{
    class GraphAPI
    {
        public static HttpStatusCode AddKeyWithPassword(string poP, string objectId, string api, string accessToken, string key, string password)
        {
            var client = new HttpClient();
            var url = $"{api}/{objectId}/addKey";

            var defaultRequestHeaders = client.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var payload = new
            {
                keyCredential = new
                {
                    type = "X509CertAndPassword",
                    usage = "Sign",
                    key,
                },
                passwordCredential = new
                {
                    secretText = password,
                },
                proof = poP
            };
            var stringPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var res = client.PostAsync(url, httpContent).Result;

            return res.StatusCode;
        }

        public static HttpStatusCode AddKey(string poP, string objectId, string api, string accessToken, string key)
        {
            var client = new HttpClient();
            var url = $"{api}/{objectId}/addKey";

            var defaultRequestHeaders = client.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            string pass = null;
            var payload = new
            {
                keyCredential = new
                {
                    type = "AsymmetricX509Cert",
                    usage = "Verify",
                    key,
                },
                passwordCredential = pass,
                proof = poP
            };
            var stringPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var res = client.PostAsync(url, httpContent).Result;

            return res.StatusCode;
        }

        public static HttpStatusCode RemoveKey(string poP, string objectId, string api, string keyId, string accessToken)
        {
            var client = new HttpClient();
            var url = $"{api}/{objectId}/removeKey";
            var defaultRequestHeaders = client.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var payload = new
            {
                keyId,
                proof = poP
            };
            var stringPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


            var res = client.PostAsync(url, httpContent).Result;
            var contents = res.Content.ReadAsStringAsync().Result;

            if (res.Content.ReadAsStringAsync().Result.Contains("No credentials found to be removed"))
            {
                throw new HttpRequestException("CertID Not Found", new HttpRequestException(contents, null, res.StatusCode));
            }

            if (res.Content.ReadAsStringAsync().Result.Contains("Access Token missing or malformed"))
            {
                throw new HttpRequestException("proof-of-possession (PoP) token is invalid", new HttpRequestException(contents, null, res.StatusCode));
            }

            return res.StatusCode;
        }
    }
}