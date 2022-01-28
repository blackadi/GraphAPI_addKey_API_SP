using System;
using System.Security.Cryptography.X509Certificates;
using System.Net;

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

            var token = new GraphAPI().GenerateTokenviaClientAssertion(clientId, aud, signingCert, tenantID);

            //========================
            //Get PoP Token
            //========================
            aud = $"00000002-0000-0000-c000-000000000000"; // audience needs to be this

            string objectId = "9d97836c-be4a-4e70-a06d-d33c2467d11e";
            var poP = new Helper().GeneratePoPToken(objectId, aud, signingCert);

            // Get the new certificate info which will be uploaded via the graph API 
            string newCerthPath = "cert which will be added via API call\\newCertToUpload.pfx";
            string pwd = "Test@123";
            X509Certificate2 CurrentCertUsed = new X509Certificate2(newCerthPath, pwd);
            var key = new Helper().GetCertificateKey(CurrentCertUsed);

            /**
            * Call the addKey API using Graph SDK
            **/
            // var graphClient = new GraphSDK();
            // var code = graphClient.AddKeyWithPassword_GraphSDK(poP, objectId, key, pwd);
            // if (code == HttpStatusCode.OK)
            // {
            //     Console.WriteLine("Uploaded!");
            // }
            // else
            // {
            //     Console.WriteLine("Something went wrong!");
            // }
            // var code = graphClient.RemoveKey_GraphSDK(poP, objectId, "0bdee6ff-c762-461c-b114-42781972938a");
            // if (code == HttpStatusCode.NoContent)
            // {
            //     Console.WriteLine("Cert Deleted!");
            // }
            // else
            // {
            //     Console.WriteLine("Something went wrong!");
            // }

            /**
            * Call the addKey API directly without using SDK
            **/
            /// Upload Certificate
            // HttpStatusCode code = new GraphAPI().AddKey(poP, objectId, token);
            // if (code == HttpStatusCode.OK)
            // {
            //     Console.WriteLine("Uploaded!");
            // }
            // else
            // {
            //     Console.WriteLine("Something went wrong!");
            // }

            /// Add Certificate ID to delete it
            // HttpStatusCode code = new GraphAPI().RemoveKey(poP, objectId, "5582c95d-6141-4527-94b8-99fd0229e7c4", token);
            // if (code == HttpStatusCode.NoContent)
            // {
            //     Console.WriteLine("Cert Deleted!");
            // }
            // else
            // {
            //     Console.WriteLine("Something went wrong!");
            // }

        }
    }
}
