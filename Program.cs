using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Linq;

namespace SampleCertCall
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure the following

            // pfxFilePath -> This must be the same valid cert used/uploaded to azure for generating access Token and PoP token  
            string pfxFilePath = "cert which is uploaded to azure\\uploadedCert.pfx";
            string password = "Test@123";
            X509Certificate2 signingCert = new X509Certificate2(pfxFilePath, password);

            //========================
            //Get acessToken via Client Assertion
            //========================
            string tenantID = "0213c7bf-21e1-4cb4-8529-e4eaff767ca4";
            string aud = $"https://login.microsoftonline.com/{tenantID}/v2.0"; // audience needs to be this 
            string clientId = "1ee1107a-4ceb-4c63-8a01-ac5d35230a99";
            var token = GenerateTokenviaClientAssertion(clientId, aud, signingCert, tenantID);

            //========================
            //Get PoP Token
            //========================
            aud = $"00000002-0000-0000-c000-000000000000"; // audience needs to be this

            string objectId = "9d97836c-be4a-4e70-a06d-d33c2467d11e";
            var poP = GeneratePoPToken(objectId, aud, signingCert);

            /// Upload Certificate
            // HttpStatusCode code = addKey(poP, objectId, token);
            // if (code == HttpStatusCode.OK)
            // {
            //     Console.WriteLine("Uploaded!");
            // }
            // else
            // {
            //     Console.WriteLine("Something went wrong!");
            // }

            /// Add Certificate ID to delete it
            // HttpStatusCode code = removeKey(poP, objectId, "121d48e4-633b-4a5e-b99a-9d1973ada9cd", token);
            // if (code == HttpStatusCode.NoContent)
            // {
            //     Console.WriteLine("Cert Deleted!");
            // }
            // else
            // {
            //     Console.WriteLine("Something went wrong!");
            // }

        }
        static HttpStatusCode addKey(string poP, string objectId, string accessToken)
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
            var key = GetCertificateKey(CurrentCertUsed);

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

        static HttpStatusCode removeKey(string poP, string objectId, string keyId, string accessToken)
        {
            var client = new HttpClient();
            var url = $"https://graph.microsoft.com/v1.0/applications/{objectId}/removeKey";
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
            var key = GetCertificateKey(CurrentCertUsed);

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

        static string GenerateTokenviaClientAssertion(string clientId, string aud, X509Certificate2 signingCert, string tenantID)
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
        static string GeneratePoPToken(string objectId, string aud, X509Certificate2 signingCert)
        {
            Guid guid = Guid.NewGuid();

            // aud and iss are the only required claims.
            var claims = new Dictionary<string, object>()
            {
                { "aud", aud },
                { "iss", objectId }
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
            var poP = handler.CreateToken(securityTokenDescriptor);
            Console.WriteLine("\n\"Generate Proof of Possession Token:\"\n--------------------------------------------");
            Console.WriteLine($"PoP: {poP}");

            return poP;
        }

        static string GetCertificateKey(X509Certificate2 cert)
        {
            Console.WriteLine("\n\n\n=================================================================================\n");
            Console.WriteLine("Certificate info to be used in request body {keyCredential resource type}\n");
            Console.WriteLine("=================================================================================\n");
            Console.WriteLine($"customKeyIdentifier (Thumbprint):  {cert.Thumbprint}"); // or signingCert.GetCertHashString()
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine($"key: {Convert.ToBase64String(cert.GetRawCertData())}");
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine($"displayName: {cert.GetName()}");
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine($"startDateTime: {Convert.ToDateTime(cert.GetEffectiveDateString()).ToString("yyyy-MM-ddTHH:mm:ssZ")}");
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine($"endDateTime: {Convert.ToDateTime(cert.GetExpirationDateString()).ToString("yyyy-MM-ddTHH:mm:ssZ")}");
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine($"keyId: {Guid.NewGuid()}");

            return Convert.ToBase64String(cert.GetRawCertData());
        }
    }
}
