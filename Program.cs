using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;

namespace SampleCertCall
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure the following
            string pfxFilePath = "cert which is uploaded to azure\\uploadedCert.pfx";
            string password = "Test@123";
            string objectId = "2126b082-180a-4525-8300-ea6465769c41"; // use {CLIENT_ID} for creating client_assertion and use {ObjectID} for PoP token creation
            Guid guid = Guid.NewGuid();

            // Get signing certificate
            X509Certificate2 signingCert = new X509Certificate2(pfxFilePath, password);

            // audience
            // string aud = "https://login.microsoftonline.com/0213c7bf-21e1-4cb4-8529-e4eaff767ca4/v2.0"; // uncomment this for {client_assertion}
            string aud = $"00000002-0000-0000-c000-000000000000"; // uncomment for proof of possession token

            // aud and iss are the only required claims.
            var claims = new Dictionary<string, object>()
            {
                { "aud", aud },
                { "iss", objectId }
                // { "sub", objectId }, // uncomment this when creating {client_assertion}
                // { "jti", guid} // uncomment this when creating {client_assertion}
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
            var x = handler.CreateToken(securityTokenDescriptor);
            Console.WriteLine($"client_assertion: {x}");
            Console.WriteLine("=================================================================================\n");
            Console.WriteLine("certificate Info to be used in request body {keyCredential resource type}\n");
            Console.WriteLine("=================================================================================\n");
            Console.WriteLine($"customKeyIdentifier (Thumbprint):  {signingCert.Thumbprint}"); // or signingCert.GetCertHashString()
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine($"key: {Convert.ToBase64String(signingCert.GetRawCertData())}");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine($"displayName: {signingCert.GetName()}");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine($"startDateTime: {Convert.ToDateTime(signingCert.GetEffectiveDateString()).ToString("yyyy-MM-ddTHH:mm:ssZ")}");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine($"endDateTime: {Convert.ToDateTime(signingCert.GetExpirationDateString()).ToString("yyyy-MM-ddTHH:mm:ssZ")}");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine($"keyId: {Guid.NewGuid()}");
        }
    }
}
