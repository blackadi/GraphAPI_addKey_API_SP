using System;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Models.ExternalConnectors;

namespace SampleCertCall
{
    class Program
    {
        private static IConfiguration config;
        static void Main(string[] args)
        {
            //=============================
            // Global variables which will be used to store app registation info, you can use appsettings.json to store such data
            //=============================
            var builder = new ConfigurationBuilder()
               .SetBasePath(System.IO.Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            config = builder.Build();
            string clientId = config.GetValue<string>("ClientId");
            string tenantID = config.GetValue<string>("TenantId");
            string scopes = config.GetValue<string>("Scopes");
            string objectId = config.GetValue<string>("ObjectId");
            string api = config.GetValue<string>("ApiUrl");
            string aud_POP = config.GetValue<string>("Aud_POP"); // audience for Client Assertion must equal this val
            string aud_ClientAssertion = config.GetValue<string>("Aud_ClientAssertion"); // audience for PoP must equal this val

            // pfxFilePath -> Use an existing valid cert used/uploaded to the app or service principal to generate access token and PoP token.
            string pfxFilePath = config.GetValue<string>("CertificateDiskPath"); // Replace the file path with the location of your certificate.
            string password = config.GetValue<string>("CertificatePassword"); // If applicable, replace the password value with your certificate password.
            X509Certificate2 signingCert = null;
            try
            {
                if (!password.IsNullOrEmpty())
                    signingCert = new X509Certificate2(pfxFilePath, password);
                else
                    signingCert = new X509Certificate2(pfxFilePath);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                Console.WriteLine("Check the old/uploaded certificate {CertificateDiskPath}, you need to add a correct certificate path and/or password for this sample to work\n" + ex.Message);
                Environment.Exit(-1);
            }

            // newCerFilePath -> This is the new cert which will be uploaded. The cert can also be stored in Azure Key Vault.
            string newCerFilePath = config.GetValue<string>("NewCertificateDiskPath"); // Replace the file path with the location of your new certificate to be uploaded using the Graph API.
            string newCertPassword = config.GetValue<string>("NewCertificatePassword"); // If applicable, replace the password value with your new certificate password.
            X509Certificate2 newCert = null;
            try
            {
                if (newCertPassword != "")
                    newCert = new X509Certificate2(newCerFilePath, newCertPassword);
                else
                    newCert = new X509Certificate2(newCerFilePath);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                Console.WriteLine("Check the new certificate {NewCertificateDiskPath}, you need to add a correct certificate path and/or password for this sample to work\n" + ex.Message);
                Environment.Exit(-1);
            }

            //========================
            //Get acessToken via client assertion
            //========================
            var client_assertion = new Helper().GenerateClientAssertion(aud_ClientAssertion, clientId, signingCert, tenantID);
            var token = new Helper().GenerateAccessTokenWithClientAssertion(aud_ClientAssertion, client_assertion, clientId, signingCert, tenantID);

            //========================
            //Get PoP Token
            //========================
            var poP = new Helper().GeneratePoPToken(objectId, aud_POP, signingCert);

            // Get the new certificate info which will be uploaded via Microsoft Graph API call
            var key = new Helper().GetCertificateKey(newCert);
            var graphClient = new Helper().GetGraphClient(scopes, tenantID, clientId, signingCert);

            int choice = -1;
            while (choice != 0)
            {
                Console.WriteLine("\n=================================================");
                Console.WriteLine("Please choose one of the following options:");
                Console.WriteLine("=================================================");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Display access token");
                Console.WriteLine("2. Display client assertion");
                Console.WriteLine("3. Display PoP token");
                Console.WriteLine("4. Display certificate Info");
                Console.WriteLine("5. Upload certificate using Graph SDK");
                Console.WriteLine("6. Upload certificate using Graph API");
                Console.WriteLine("7. Delete certificate using Graph SDK");
                Console.WriteLine("8. Delete certificate using Graph API");
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
                        // Display client assertion
                        Console.WriteLine("\n\"Proof of Possession Token Value is\"\n__________________");
                        Console.WriteLine($"PoP token: {poP}");
                        Console.WriteLine("__________________\n");
                        break;
                    case 4:
                        // Display certificate key
                        new Helper().DisplayCertificateInfo(newCert);
                        break;
                    case 5:
                        // Call the addKey SDK using Graph SDK
                        if (newCertPassword != "")
                        {
                            var response = new GraphSDK().AddKeyWithPassword_GraphSDKAsync(poP, objectId, key, newCertPassword, graphClient).GetAwaiter().GetResult();
                        }
                        else
                        {
                            var response = new GraphSDK().AddKey_GraphSDKAsync(poP, objectId, key, graphClient).GetAwaiter().GetResult();
                        }
                        if (code != null)
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
                        // Call the addKey API directly without using SDK
                        if (!password.IsNullOrEmpty())
                        {
                            code = new GraphAPI().AddKeyWithPassword(poP, objectId, api, token, key, newCertPassword);
                        }
                        else
                        {
                            code = new GraphAPI().AddKey(poP, objectId, api, token, key);
                        }
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
                    case 7:
                        // Call the removeKey API using Graph SDK
                        Console.WriteLine("\nEnter certificate ID that you want to delete:");
                        certID = Console.ReadLine();
                        try
                        {
                            if (Guid.TryParse(certID, out val))
                            {
                                var response = new GraphSDK().RemoveKey_GraphSDKAsync(poP, objectId, certID, graphClient).GetAwaiter().GetResult();

                                if (response)
                                {
                                    Console.WriteLine("\n______________________");
                                    Console.WriteLine("Cert Deleted Successfully!");
                                    Console.WriteLine("_____________________\n");
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
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("\n______________________");
                            Console.WriteLine("ERROR: CertID Not Found");
                            Console.WriteLine("______________________\n");
                        }

                        break;
                    case 8:
                        // Call the removeKey API directly without using API
                        Console.WriteLine("\nEnter certificate ID that you want to delete:");
                        certID = Console.ReadLine();
                        try
                        {
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
                                Console.WriteLine("ERROR: Invalid Certificate ID");
                                Console.WriteLine("______________________\n");
                            }
                        }

                        catch (HttpRequestException ex)
                        {
                            Console.WriteLine(ex.InnerException.Message);
                            Console.WriteLine("\n______________________");
                            Console.WriteLine(ex.Message);
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