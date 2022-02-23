using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace SampleCertCall
{
    class Helper
    {
        private static IConfiguration configuration;
        public string GeneratePoPToken(string objectId, string aud, X509Certificate2 signingCert)
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
            // Console.WriteLine("\n\"Generate Proof of Possession Token:\"\n--------------------------------------------");
            // Console.WriteLine($"PoP: {poP}");

            return poP;
        }

        public string GetCertificateKey(X509Certificate2 cert)
        {
            return Convert.ToBase64String(cert.GetRawCertData());
        }

        public void DisplayCertificateInfo(X509Certificate2 cert)
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

        public GraphServiceClient GetGraphClient(string scopes, string tenantId, string clientId, X509Certificate2 signingCert)
        {
            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientcertificatecredential
            var clientCertificateCredential = new ClientCertificateCredential(
                tenantId, clientId, signingCert, options);

            var graphClient = new GraphServiceClient(clientCertificateCredential, new[] { scopes });

            return graphClient;
        }

        public IConfiguration ReadFromJsonFile()
        {
            // Using appsettings.json to load the configuration settings
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            configuration = builder.Build();

            return configuration;
        }

        public void IsConfigSetToDefault(string clientId, string tenantID, string scopes, string objectId, string aud_ClientAssertion)
        {
            if (clientId.Contains("YOUR_CLIENT_ID_HERE") || tenantID.Contains("YOUR_TENANT_ID_HERE") || objectId.Contains("YOUR_OBJECT_ID_HERE") || aud_ClientAssertion.Contains("{YOUR_TENANT_ID_HERE}"))
            {
                Console.WriteLine("Please configure the sample to use your Azure AD tenant using appsettings.json file");
            }
        }
    }
}