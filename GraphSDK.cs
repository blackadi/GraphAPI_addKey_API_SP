using System;
using System.Net;
using Microsoft.Graph;
using Azure.Identity;


namespace SampleCertCall
{
    class GraphSDK
    {
        // Using GraphSDK instead of calling the API directly

        public string AddKey_GraphSDK(string proof, string objectId, string key)
        {

            var graphClient = new Helper().GetGraphClient();

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
                .PostAsync().GetAwaiter().GetResult();

            return res.KeyId.Value.ToString();
        }

        public HttpStatusCode AddKeyWithPassword_GraphSDK(string proof, string objectId, string key, string password)
        {
            var graphClient = new Helper().GetGraphClient();

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

        public HttpStatusCode RemoveKey_GraphSDK(string proof, string objectId, string certID)
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

            var keyId = Guid.Parse(certID);

            var res = graphClient.Applications[objectId]
                .RemoveKey(keyId, proof)
                .Request()
                .PostResponseAsync().GetAwaiter().GetResult();

            return res.StatusCode;
        }
    }
}