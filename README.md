# Sample Guide Explaning How Adds [servicePrincipal:addKey](https://docs.microsoft.com/en-us/graph/api/serviceprincipal-addkey?view=graph-rest-1.0&tabs=http) API works.

> :warning: **THIS GUIDE SAMPLE IS PROVIDED _"AS IS"_ WITHOUT WARRANTY OF ANY KIND**.
> This sample is not supported under any Microsoft standard support program or service. The code sample is provided AS IS without warranty of any kind **_Personal Effort_**.

## This tutorial will demonstrate how to add certificate to your application via Graph API

> :exclamation: [**Before you can call addKey API any existing valid certificates must be uploaded**](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-certificate-credentials#register-your-certificate-with-microsoft-identity-platform), Applications OR servicePrincipal that don’t have any existing valid certificates (no certificates have been added yet, or all certificates have expired), won’t be able to use this service action. You can use the Update application operation to perform an update instead. need to have exisiting cert follow below.

For this tutorial, I will do a `PATCH` request to upload my test certificate to one of my test servicePrincipal, but feel free to use anythose method to update your SP.

Using the C# code to fetch certficate info to be used with "_[keyCredentials](https://docs.microsoft.com/en-us/graph/api/resources/keycredential?view=graph-rest-1.0)_" Propertyi n the request body.

```json
{
  "keyCredentials": [
    {
      "key": "MIIDFDCCAfygAwIBAgIQIONixStLEqNCfOY/Hl1VcTANBgkqhkiG9w0BAQsFADAdMRswGQYDVQQDDBJ1cGxvYWRlZFRlc3QubG9jYWwwHhcNMjEwODIyMTEwMDMxWhcNMjIwODIyMTEyMDMxWjAdMRswGQYDVQQDDBJ1cGxvYWRlZFRlc3QubG9jYWwwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDuIEybadSNjJtghmk8LskRfR7aLg1Ld1WxB8aUQoWfq4YcRvedThATS4lyTa4Eh2pXnWSdO3idua9PlfCwIboEzKFz53oqlEylPB3YIxKtl1bPtH8hwonwD8Il5obNsHoHs1N1EThrsjBmTxTY5nO2kozwfmEvR60YX305rPhLeRlO5ikMZEsY1ybGQOKOr+g/OuNw52Q54H5ZvJgbcnVNNsb1AQvHESpvZkNiU9Z8svSN4G4E9kJIRgivuts6lLEUsl9r6SAcm33yIEMLi6xTHJNUZZAE9JOjugbdeSi0mPgnBB6+l1FKfx3kwnShgLgJ9wf9GLe5x00vEJIOPeo9AgMBAAGjUDBOMA4GA1UdDwEB/wQEAwIFoDAdBgNVHSUEFjAUBggrBgEFBQcDAgYIKwYBBQUHAwEwHQYDVR0OBBYEFGpgjxVGK2BQutJIQJVnbKlMZhshMA0GCSqGSIb3DQEBCwUAA4IBAQDhagw9f9rfRizSkfxec+sGP+W4jj89jz0YRlDHV+fa906yl9MaUbmPuzb33lPaf/n5FcNHGBkB71FFUAEy3pqMWHdyt/0lWRAfXWcOatEGx0T++qZabyiga7cwGCkYgeQnnEIwMsDd+G7BzmRdMqUULnZkbxKvhZrQp9ljKt/cRn2vdfTSAMxptzJ6ZIfQ/tukebKcvE6x8XocxTQ7eH1r0qFnhUl5+yHAVTh1MoaOBPLy7dCrvSdG+PjXpVROjQQFre4gNNHDjLSsXi2h6rPUTRHLjpu2xvzQaJweg6t0/AqET5WXUY6uWStKMa+7XR3Jv9VxqGn55cAGTSrORp+E",
      "keyId": "89357730-3f4f-49d6-90d2-c479bf925702",
      "type": "AsymmetricX509Cert",
      "usage": "Verify"
    }
  ]
}
```

![Package Structure](images\UpdateSP.PNG)

### 1. [Create a self-signed public certificate to authenticate your application](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate)

> :exclamation: **Caution**
> Using a self-signed certificate is only recommended for development, not production.

- [Option 1](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate#option-1--create-and-export-your-public-certificate-without-a-private-key): Create and export your public certificate without a private key

  ```powershell
  $cert = New-SelfSignedCertificate -Subject "CN={certificateName}" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256    ## Replace {certificateName}
  ```

  ```powershell
  Export-Certificate -Cert $cert -FilePath "C:\Users\admin\Desktop\{certificateName}.cer"   ## Specify your preferred location and replace {certificateName}
  ```

  Your certificate is now ready to upload to the Azure portal. Once uploaded, retrieve the certificate thumbprint for use to authenticate your application.

- [Option 2](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate#option-2-create-and-export-your-public-certificate-with-its-private-key): Create and export your public certificate with its private key

  ```powershell
  $cert = New-SelfSignedCertificate -Subject "CN={certificateName}" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256    ## Replace {certificateName}
  ```

  ```powershell
  Export-Certificate -Cert $cert -FilePath "C:\Users\admin\Desktop\{certificateName}.cer"   ## Specify your preferred location and replace {certificateName}
  ```

  ```powershell
  $mypwd = ConvertTo-SecureString -String "{myPassword}" -Force -AsPlainText  ## Replace {myPassword}
  ```

  ```powershell
  Export-PfxCertificate -Cert $cert -FilePath "C:\Users\admin\Desktop\{privateKeyName}.pfx" -Password $mypwd   ## Specify your preferred location and replace {privateKeyName}
  ```

  Your certificate (.cer file) is now ready to upload to the Azure portal. You also have a private key (.pfx file) that is encrypted and can't be read by other parties. Once uploaded, retrieve the certificate thumbprint for use to authenticate your application.

### 2. [Use the portal to create an Azure AD application and service principal that can access resources](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal)

[`This article`](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#option-1-upload-a-certificate) shows you how to Upload a certificate for authentication with service principals.

### 3. [Use JSON Web Token (JWT) assertion signed with a certificate that the application owns](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-certificate-credentials)

- Now we will create a signed jwt token (aka Client Assertion) using **`Postman`**.
- Then, get an Access Token Using [Client Credentials Grant Flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow#second-case-access-token-request-with-a-certificate)

  To compute the assertion, you can use one of the many JWT libraries in the language of your choice - [MSAL supports this using .WithCertificate()](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-client-assertions). The information is carried by the token in its Header, Claims, and Signature.

  For this demo [this modified sample C# code](https://docs.microsoft.com/en-us/graph/application-rollkey-prooftoken) will be used to generate this.

> **:information_source: You can `clone` the sample code or `Copy/Past` the code below.**

`git clone https://github.com/blackadi/GraphAPI_addKey_API_SP.git`

```csharp
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
```

Make sure the values for your own tenant is added as shown in the screenhost.

- to get the correct client assertion, uncomment everything from the code were it say for creating client_assertion.

  ![Package Structure](images\Generate_client_assertion.PNG)

  ![Package Structure](images\Generate_client_assertion_result.PNG)

  ![Package Structure](images\AccessTokenRequestwithCertificate.PNG)

- Generate PoP (proof of possession)

  > :information_source: Authentication_MissingOrMalformed error will be returned if PoP is not signed with the already uploaded certificate.

  Get new certificate info from the code, then to old certificate to get PoP from it.

  ![Package Structure](images\GetNewCertInfo.PNG)

  ![Package Structure](images\GetNewCertInfo2.PNG)

  ```json
  {
    "keyCredential": {
      "type": "X509CertAndPassword",
      "usage": "Sign",
      "key": "MIIDFjCCAf6gAwIBAgIQFk6OV+DB1pxCJQxk0bH6uTANBgkqhkiG9w0BAQsFADAeMRwwGgYDVQQDDBN1cGxvYWROZXdDZXJ0LmxvY2FsMB4XDTIxMDgyMjExMDMyMVoXDTIyMDgyMjExMjMyMVowHjEcMBoGA1UEAwwTdXBsb2FkTmV3Q2VydC5sb2NhbDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAOaYXVefPQIGko9aklP6YnklMV3Km+90D7Ikp6tk8/aQNptXSNf5dFJpmgWD8qbo5lLxHLpeO+HmnirKvwPCErCr6gHkmBwie7iP3qgh0xLsfGdpePaSwA7vaBOZlsOoadqXj+Rwlmktc7/J2MKC2HcEMN0OAUJyTO5YmYtkGi7ETnBxKWpTSmbL3M1EY4Gu+so0NXru5SO0cR3lJk49uX7ixIQBPNK1llnopncrMaTaD8pDYgZSWA0sEcCz9u8EsCx8rJmNDGOa7GfM7/fCAIWQWAvMb4BKPOh6gBBR+i1D1Lr3uVsNQ9pqqhpd6+z73jKUbCExcgp/iLLXFBSsxxECAwEAAaNQME4wDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDATAdBgNVHQ4EFgQUswUzw+7dO/95M0wg0at5/eHo6pgwDQYJKoZIhvcNAQELBQADggEBAG1x0VG6UPF0lLuZgUMCgmLMGh5iHXmqNA2DkHfhbKd+JFUowSA/Vd2NdN7zByNEWCpsNsiEgnzMen9zM53cuA9sQXmG2TxYFUYQ3fuFXpqrRvBqxP0UpeSZG6rZvr/nihoUfIY8JWC/iNIBoUbMjfUay2BDmCzRbLIKrmhuaHpIxHxSnHs1EUYcDejk7pzmdPPazrcKatmn1LK0o5o3kHXdKxoiYDoH9SVqhiQPx7Ge9oa9TebN9NzXHso3GIYd36YtlD12KRBF7wKbSl7X6oK1ka3WLCCdmMf76gU76ZFuEtgWPkzEckfH8fep0UqtLyPbCXkQv9KXhzkNVyx0poA="
    },
    "passwordCredential": {
      "secretText": "Test@123"
    },
    "proof": "To_BE_ADD_IN_NEXT_STEP"
  }
  ```

### 3. Now we can use that access token to add our new certificate via Graph API to either the [application](https://docs.microsoft.com/en-us/graph/api/application-addkey?view=graph-rest-1.0&tabs=http) or [servicePrincipal](https://docs.microsoft.com/en-us/graph/api/serviceprincipal-addkey?view=graph-rest-1.0&tabs=http).

- Now we will call addKey API using **`Postman`**

  Using the `C#` code we will generate the PoP token based on the valid certificate uploaded to our azure SP previously and use it to add the new certificate.
  ![Package Structure](images\GetPoP1.PNG)

  ![Package Structure](images\GetPoP2.PNG)

  ```json
  {
    "keyCredential": {
      "type": "X509CertAndPassword",
      "usage": "Sign",
      "key": "MIIDFjCCAf6gAwIBAgIQFk6OV+DB1pxCJQxk0bH6uTANBgkqhkiG9w0BAQsFADAeMRwwGgYDVQQDDBN1cGxvYWROZXdDZXJ0LmxvY2FsMB4XDTIxMDgyMjExMDMyMVoXDTIyMDgyMjExMjMyMVowHjEcMBoGA1UEAwwTdXBsb2FkTmV3Q2VydC5sb2NhbDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAOaYXVefPQIGko9aklP6YnklMV3Km+90D7Ikp6tk8/aQNptXSNf5dFJpmgWD8qbo5lLxHLpeO+HmnirKvwPCErCr6gHkmBwie7iP3qgh0xLsfGdpePaSwA7vaBOZlsOoadqXj+Rwlmktc7/J2MKC2HcEMN0OAUJyTO5YmYtkGi7ETnBxKWpTSmbL3M1EY4Gu+so0NXru5SO0cR3lJk49uX7ixIQBPNK1llnopncrMaTaD8pDYgZSWA0sEcCz9u8EsCx8rJmNDGOa7GfM7/fCAIWQWAvMb4BKPOh6gBBR+i1D1Lr3uVsNQ9pqqhpd6+z73jKUbCExcgp/iLLXFBSsxxECAwEAAaNQME4wDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDATAdBgNVHQ4EFgQUswUzw+7dO/95M0wg0at5/eHo6pgwDQYJKoZIhvcNAQELBQADggEBAG1x0VG6UPF0lLuZgUMCgmLMGh5iHXmqNA2DkHfhbKd+JFUowSA/Vd2NdN7zByNEWCpsNsiEgnzMen9zM53cuA9sQXmG2TxYFUYQ3fuFXpqrRvBqxP0UpeSZG6rZvr/nihoUfIY8JWC/iNIBoUbMjfUay2BDmCzRbLIKrmhuaHpIxHxSnHs1EUYcDejk7pzmdPPazrcKatmn1LK0o5o3kHXdKxoiYDoH9SVqhiQPx7Ge9oa9TebN9NzXHso3GIYd36YtlD12KRBF7wKbSl7X6oK1ka3WLCCdmMf76gU76ZFuEtgWPkzEckfH8fep0UqtLyPbCXkQv9KXhzkNVyx0poA="
    },
    "passwordCredential": {
      "secretText": "Test@123"
    },
    "proof": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjczMEQ2OUUwQzVFMDFCOTIyODc3MzU4QTg5NEQ0QjVDQTYwNTNGRDMiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJjdzFwNE1YZ0c1SW9keldLaVUxTFhLWUZQOU0ifQ.eyJhdWQiOiIwMDAwMDAwMi0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiIyMTI2YjA4Mi0xODBhLTQ1MjUtODMwMC1lYTY0NjU3NjljNDEiLCJleHAiOjE2Mjk2MzY5NDgsIm5iZiI6MTYyOTYzNjM0OCwiaWF0IjoxNjI5NjM2MzQ4fQ.OWfNEq1oHaxKzwipajhMDMNGihJtF8gYTlXqxy85TXrHx2HGrg3y5uJI-H5MZ5rgrXfuUpFpgmuKIjLT0RDhle2TdtgavLXALYRXBTHl8bgo17IkwIAZeP5oW3HeU7kph7i4pSdG31474tL1vKxqLDr6IaeJklzcC0B7BbL1Bybs147er98XACeJ9k74kEPEm7hP0kOEbzJwm5VfCN2A96wpBCsHRD96bdb5il9OOBbX9FFb536VHNfRSau32LjHPPar0oAgmNj4Zs9penDzvB-j5yBPGiMQguWxDL7L07iiUAnU-frbG2mNh_XjjjWLDKgOgP4mx0_FjsCwEATxRg"
  }
  ```

  After calling the API, the new certificate will uploaded.

  ![Package Structure](images\NewCertUploaded.PNG)

  Making a GET call to https://graph.microsoft.com/v1.0/servicePrincipals/2126b082-180a-4525-8300-ea6465769c41 we can see the result containing the new certficate and it's password.
  ![Package Structure](images\NewCertUploaded2.PNG)
