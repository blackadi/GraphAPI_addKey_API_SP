using Microsoft.Graph;
// using Microsoft.Graph.ServicePrincipals.Item.AddKey; // Dependencies for the AddKey service principal method
// using Microsoft.Graph.ServicePrincipals.Item.RemoveKey; // Dependencies for the RemoveKey service principal method
using Microsoft.Graph.Applications.Item.AddKey;
using Microsoft.Graph.Applications.Item.RemoveKey;
using Microsoft.Graph.Models;
using System;
using System.Threading.Tasks;


namespace SampleCertCall
{
    class GraphSDK
    {
        // Using GraphSDK instead of calling the API directly
        public static async Task<KeyCredential> AddKey_GraphSDKAsync(string proof, string objectId, string key, GraphServiceClient graphClient)
        {
            var requestBody = new AddKeyPostRequestBody
            {
                KeyCredential = new KeyCredential
                {
                    Type = "AsymmetricX509Cert",
                    Usage = "Verify",
                    Key = Convert.FromBase64String(key)
                },
                PasswordCredential = null,
                Proof = proof
            };

            // var res = await graphClient.ServicePrincipals[objectId].AddKey.PostAsync(requestBody); // Uncomment this to upload a certificate to a service principal and the using statement at the top
            var res = await graphClient.Applications[objectId].AddKey.PostAsync(requestBody);

            return res;
        }

        public static async Task<KeyCredential> AddKeyWithPassword_GraphSDKAsync(string proof, string objectId, string key, string password, GraphServiceClient graphClient)
        {
            var requestBody = new AddKeyPostRequestBody
            {
                KeyCredential = new KeyCredential
                {
                    Type = "X509CertAndPassword",
                    Usage = "Sign",
                    Key = Convert.FromBase64String(key)
                },
                PasswordCredential = new PasswordCredential
                {
                    SecretText = password
                },
                Proof = proof
            };

            // var res = await graphClient.ServicePrincipals[objectId] // Uncomment this to upload a certificate to a service principal and the using statement at the top
            var res = await graphClient.Applications[objectId] // Upload a certificate to the application
                        .AddKey
                        .PostAsync(requestBody);

            return res;
        }

        public static async Task<bool> RemoveKey_GraphSDKAsync(string proof, string objectId, string certID, GraphServiceClient graphClient)
        {
            var keyId = Guid.Parse(certID);

            try
            {
                var requestBody = new RemoveKeyPostRequestBody
                {
                    KeyId = keyId,
                    Proof = proof
                };

                // await graphClient.ServicePrincipals[objectId] // Uncomment this to remove a certificate from a service principal and the using statement at the top
                await graphClient.Applications[objectId] // Remove a certificate from the application
                    .RemoveKey
                    .PostAsync(requestBody);

                return true;
            }
            catch (Exception ex)
            {
                Console.Write($"Exception RemoveKey_GraphSDKAsync: {Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");

                return false;
            }
        }
    }
}