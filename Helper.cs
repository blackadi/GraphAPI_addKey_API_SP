using Azure.Identity;
using Microsoft.Graph;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SampleCertCall
{
    class Helper
    {
        public static string GenerateClientAssertion(string aud, string clientId, X509Certificate2 signingCert)
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
            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                NotBefore = now,
                Expires = now.AddMinutes(10),
                SigningCredentials = new X509SigningCredentials(signingCert)
            };

            var handler = new JsonWebTokenHandler();
            // Get Client Assertion
            var client_assertion = handler.CreateToken(securityTokenDescriptor);

            return client_assertion;
        }

        public static async Task<string> GenerateAccessTokenWithClientAssertionAsync(string client_assertion, string clientId, string tenantID)
        {
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
            var res = await client.PostAsync(url, new FormUrlEncodedContent(data));
            var content = await res.Content.ReadAsStringAsync();
            var token = "";

            if (content.Contains("AADSTS"))
            {
                throw new HttpRequestException(content, null, System.Net.HttpStatusCode.Unauthorized);
            }
            using (HttpResponseMessage response = res)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);
                token = (string)obj["access_token"];
            }

            return token;
        }

        public static string GeneratePoPToken(string objectId, string aud, X509Certificate2 signingCert)
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
            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                NotBefore = now,
                Expires = now.AddMinutes(10),
                SigningCredentials = new X509SigningCredentials(signingCert)
            };

            var handler = new JsonWebTokenHandler();
            var poP = handler.CreateToken(securityTokenDescriptor);
            // Console.WriteLine("\n\"Generate Proof of Possession Token:\"\n--------------------------------------------");
            // Console.WriteLine($"PoP: {poP}");

            return poP;
        }

        public static string GetCertificateKey(X509Certificate2 cert)
        {
            return Convert.ToBase64String(cert.GetRawCertData());
        }

        public static void DisplayCertificateInfo(X509Certificate2 cert)
        {

            Console.WriteLine("\n[Certificate info which will be used in the request body {keyCredential resource type}]");
            Console.WriteLine("__________________________________________________________________________________________\n");
            Console.WriteLine($"customKeyIdentifier (Thumbprint):  {cert.Thumbprint}");
            Console.WriteLine("");
            Console.WriteLine($"key: {Convert.ToBase64String(cert.GetRawCertData())}");
            Console.WriteLine("");
            Console.WriteLine($"displayName: {cert.Subject}");
            Console.WriteLine("");
            Console.WriteLine($"startDateTime: {Convert.ToDateTime(cert.GetEffectiveDateString()).ToString("yyyy-MM-ddTHH:mm:ssZ")}");
            Console.WriteLine("");
            Console.WriteLine($"endDateTime: {Convert.ToDateTime(cert.GetExpirationDateString()).ToString("yyyy-MM-ddTHH:mm:ssZ")}");
            Console.WriteLine("__________________________________________________________________________________________\n");
        }

        public static GraphServiceClient GetGraphClient(string scopes, string tenantId, string clientId, X509Certificate2 signingCert)
        {
            // using Azure.Identity;
            var options = new ClientCertificateCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientcertificatecredential
            var clientCertificateCredential = new ClientCertificateCredential(
                tenantId, clientId, signingCert, options);

            var graphClient = new GraphServiceClient(clientCertificateCredential, [scopes]);

            return graphClient;
        }

    }
}