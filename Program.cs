using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace SampleCertCall
{
    class Program
    {
        private static IConfiguration config;
        static void Main(string[] args)
        {
            //=============================
            // Read app registration info
            //=============================
            config = new Helper().ReadFromJsonFile();
            var clientId = config.GetValue<string>("ClientId");
            string tenantID = config.GetValue<string>("TenantId");
            string clientSecret = config.GetValue<string>("ClientSecret");
            string scopes = config.GetValue<string>("Scopes");
            string objectId = config.GetValue<string>("ObjectId");
            string api = config.GetValue<string>("ApiUrl");
            string aud_POP = config.GetValue<string>("Aud_POP"); // audience for Client Assertion must equal this val
            string aud_ClientAssertion = config.GetValue<string>("Aud_ClientAssertion"); // audience for PoP must equal this val

            // pfxFilePath -> This must be the same valid cert used/uploaded to azure for generating access Token and PoP token
            string pfxFilePath = "cert which is uploaded to azure\\uploadedCert.pfx";
            string password = "Test@123";
            X509Certificate2 signingCert = new X509Certificate2(pfxFilePath, password);

            //========================
            //Get acessToken via Client Assertion
            //========================
            var client_assertion = new GraphAPI().GenerateClientAssertion(aud_ClientAssertion, clientId, signingCert, tenantID);
            var token = new GraphAPI().GenerateAccessTokenWithClientAssertion(aud_ClientAssertion, client_assertion, clientId, signingCert, tenantID);

            //========================
            //Get PoP Token
            //========================
            var poP = new Helper().GeneratePoPToken(objectId, aud_POP, signingCert);

            // Get the new certificate info which will be uploaded via the graph API 
            string newCerthPath = "cert which will be added via API call\\newCertToUpload.pfx";
            string pwd = "Test@123";
            X509Certificate2 CurrentCertUsed = new X509Certificate2(newCerthPath, pwd);
            var key = new Helper().GetCertificateKey(CurrentCertUsed);
            var graphClient = new Helper().GetGraphClient(scopes, tenantID, clientId, clientSecret);

            int choice = -1;
            while (choice != 0)
            {
                Console.WriteLine("\n=================================================");
                Console.WriteLine("Please choose one of the following options:");
                Console.WriteLine("=================================================");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Display access token");
                Console.WriteLine("2. Display client assertion");
                Console.WriteLine("3. Display certificate Info");
                Console.WriteLine("4. Upload certificate using Graph SDK");
                Console.WriteLine("5. Upload certificate using Graph API");
                Console.WriteLine("6. Delete certificate using Graph SDK");
                Console.WriteLine("7. Delete certificate using Graph API");
                Console.WriteLine("\nEnter the choose number here:");
                choice = Int32.TryParse(Console.ReadLine(), out choice) ? choice : -1;

                var code = new HttpStatusCode();
                string certID;
                Guid val;

                // Process user choice
                switch (choice)
                {
                    case 0:
                        // Exit the program
                        Console.WriteLine("\nGoodbye...\n");
                        break;
                    case 1:
                        // Display access token
                        Console.WriteLine("\n\"Access Token Value is:\"\n__________________");
                        Console.WriteLine($"Access Token: {token}");
                        Console.WriteLine("__________________\n");
                        break;
                    case 2:
                        // Display client assertion
                        Console.WriteLine("\n\"Client Assertion Token Value is\"\n__________________");
                        Console.WriteLine($"client_assertion: {client_assertion}");
                        Console.WriteLine("__________________\n");
                        break;
                    case 3:
                        // Display certificate key
                        new Helper().DisplayCertificateInfo(CurrentCertUsed);
                        break;
                    case 4:
                        // Call the addKey API using Graph SDK
                        code = new GraphSDK().AddKeyWithPassword_GraphSDK(poP, objectId, key, pwd, graphClient);
                        if (code == HttpStatusCode.OK)
                        {
                            Console.WriteLine("\n______________________");
                            Console.WriteLine("Uploaded Successfully!");
                            Console.WriteLine("______________________\n");
                        }
                        else
                        {
                            Console.WriteLine("\n______________________");
                            Console.WriteLine("Something went wrong!");
                            Console.WriteLine("HTTP Status code is " + code);
                            Console.WriteLine("______________________\n");
                        }

                        break;
                    case 5:
                        // Call the addKey API directly without using SDK
                        code = new GraphAPI().AddKeyWithPassword(poP, objectId, api, token);
                        if (code == HttpStatusCode.OK)
                        {
                            Console.WriteLine("\n______________________");
                            Console.WriteLine("Uploaded Successfully!");
                            Console.WriteLine("______________________\n");
                        }
                        else
                        {
                            Console.WriteLine("\n______________________");
                            Console.WriteLine("Something went wrong!");
                            Console.WriteLine("HTTP Status code is " + code);
                            Console.WriteLine("______________________\n");
                        }
                        break;
                    case 6:
                        // Call the removeKey API using Graph SDK
                        Console.WriteLine("\nEnter certificate ID that you want to delete:");
                        certID = Console.ReadLine();
                        if (Guid.TryParse(certID, out val))
                        {
                            code = new GraphSDK().RemoveKey_GraphSDK(poP, objectId, certID, graphClient);
                            if (code == HttpStatusCode.NoContent)
                            {
                                Console.WriteLine("\n______________________");
                                Console.WriteLine("Cert Deleted Successfully!");
                                Console.WriteLine("_____________________\n");
                            }
                            else
                            {
                                Console.WriteLine("\n______________________");
                                Console.WriteLine("\n\n\nSomething went wrong!");
                                Console.WriteLine("HTTP Status code is " + code);
                                Console.WriteLine("______________________\n");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\n______________________");
                            Console.WriteLine("Invalid Certificate ID");
                            Console.WriteLine("______________________\n");
                        }

                        break;
                    case 7:
                        // Call the removeKey API directly without using API
                        Console.WriteLine("\nEnter certificate ID that you want to delete:");
                        certID = Console.ReadLine();
                        if (Guid.TryParse(certID, out val))
                        {
                            code = new GraphAPI().RemoveKey(poP, objectId, api, certID, token);
                            if (code == HttpStatusCode.NoContent)
                            {
                                Console.WriteLine("\n______________________");
                                Console.WriteLine("Cert Deleted Successfully!");
                                Console.WriteLine("______________________\n");
                            }
                            else
                            {
                                Console.WriteLine("\n______________________");
                                Console.WriteLine("Something went wrong!");
                                Console.WriteLine("HTTP Status code is " + code);
                                Console.WriteLine("______________________\n");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\n______________________");
                            Console.WriteLine("Invalid Certificate ID");
                            Console.WriteLine("______________________\n");
                        }

                        break;
                    default:
                        Console.WriteLine("\n______________________");
                        Console.WriteLine("Invalid choice");
                        Console.WriteLine("______________________\n");
                        break;
                }

            }
        }
    }
}
