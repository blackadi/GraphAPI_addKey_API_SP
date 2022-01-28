using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Graph;
using Azure.Identity;

namespace SampleCertCall
{
    class Helper
    {
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
            Console.WriteLine("\n\"Generate Proof of Possession Token:\"\n--------------------------------------------");
            Console.WriteLine($"PoP: {poP}");

            return poP;
        }

        public string GetCertificateKey(X509Certificate2 cert)
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

        public GraphServiceClient GetGraphClient()
        {
            // The client credentials flow requires that you request the
            // /.default scope, and preconfigure your permissions on the
            // app registration in Azure. An administrator must grant consent
            // to those permissions beforehand.
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            // Multi-tenant apps can use "common",
            // single-tenant apps must use the tenant ID from the Azure portal
            var tenantId = "0213c7bf-21e1-4cb4-8529-e4eaff767ca4";

            // Values from app registration
            var clientId = "1ee1107a-4ceb-4c63-8a01-ac5d35230a99";
            var clientSecret = "3JW7Q~KetXAgQncz7umOt2pf1mVPOO3dgs5rV";

            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphClient;
        }
    }
}