using System;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;

namespace SampleCertCall
{
    class GraphAPI
    {
        public HttpStatusCode AddKey(string poP, string objectId, string accessToken)
        {
            var client = new HttpClient();
            var url = $"https://graph.microsoft.com/v1.0/applications/{objectId}/addKey";
            var defaultRequestHeaders = client.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Get the new certificate info which will be uploaded via the graph API 
            string pfxFilePath = "cert which will be added via API call\\newCertToUpload.pfx";
            string password = "Test@123";
            X509Certificate2 CurrentCertUsed = new X509Certificate2(pfxFilePath, password);
            var key = new Helper().GetCertificateKey(CurrentCertUsed);

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

            var res = client.PostAsync(url, httpContent).GetAwaiter().GetResult();

            return res.StatusCode;
        }

        public HttpStatusCode RemoveKey(string poP, string objectId, string keyId, string accessToken)
        {
            var client = new HttpClient();
            var url = $"https://graph.microsoft.com/v1.0/applications/{objectId}/removeKey";
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

            var res = client.PostAsync(url, httpContent).GetAwaiter().GetResult();

            return res.StatusCode;
        }

        public string GenerateTokenviaClientAssertion(string clientId, string aud, X509Certificate2 signingCert, string tenantID)
        {
            Guid guid = Guid.NewGuid();

            // aud and iss are the only required claims.
            var claims = new Dictionary<string, object>()
            {
                { "aud", aud },
                { "iss", clientId },
                { "sub", clientId },
                { "jti", guid}
            };

            // token validity should not be more than 10 minutes
            var now = DateTime.UtcNow;
            var securityTokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Claims = claims,
                NotBefore = now,
                Expires = now.AddMinutes(10),
                SigningCredentials = new X509SigningCredentials(signingCert)
            };

            var handler = new JsonWebTokenHandler();
            var client_assertion = handler.CreateToken(securityTokenDescriptor);
            Console.WriteLine("\n\"Generate Client Assertion Token:\"\n--------------------------------------------");
            Console.WriteLine($"client_assertion: {client_assertion}");

            // GET ACCESS TOKEN
            var data = new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                new KeyValuePair<string, string>("client_assertion", client_assertion),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
            };

            var client = new HttpClient();
            var url = $"https://login.microsoftonline.com/{tenantID}/oauth2/v2.0/token";
            var res = client.PostAsync(url, new FormUrlEncodedContent(data)).GetAwaiter().GetResult();
            var token = "";
            using (HttpResponseMessage response = res)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                JObject obj = JObject.Parse(responseBody);
                token = (string)obj["access_token"];
            }

            return token;
        }
    }
}