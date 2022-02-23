using System;
using System.Net;
using System.Net.Http;
using Microsoft.Graph;


namespace SampleCertCall
{
    class GraphSDK
    {
        // Using GraphSDK instead of calling the API directly
        public HttpStatusCode AddKey_GraphSDK(string proof, string objectId, string key, GraphServiceClient graphClient)
        {
            var keyCredential = new KeyCredential
            {
                Type = "AsymmetricX509Cert",
                Usage = "Verify",
                Key = Convert.FromBase64String(key)
            };

            PasswordCredential passwordCredential = null;

            var res = graphClient.Applications[objectId]
                .AddKey(keyCredential, proof, passwordCredential)
                .Request()
                .PostResponseAsync().GetAwaiter().GetResult();

            return res.StatusCode;
        }

        public HttpStatusCode AddKeyWithPassword_GraphSDK(string proof, string objectId, string key, string password, GraphServiceClient graphClient)
        {
            var keyCredential = new KeyCredential
            {
                Type = "X509CertAndPassword",
                Usage = "Sign",
                Key = Convert.FromBase64String(key)
            };

            var passwordCredential = new PasswordCredential
            {
                SecretText = password
            };

            var res = graphClient.Applications[objectId]
                        .AddKey(keyCredential, proof, passwordCredential)
                        .Request()
                        .PostResponseAsync().GetAwaiter().GetResult();

            return res.StatusCode;
        }

        public HttpStatusCode RemoveKey_GraphSDK(string proof, string objectId, string certID, GraphServiceClient graphClient)
        {
            var keyId = Guid.Parse(certID);

            try
            {
                var res = graphClient.Applications[objectId]
                    .RemoveKey(keyId, proof)
                    .Request()
                    .PostResponseAsync().GetAwaiter().GetResult();

                return res.StatusCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}