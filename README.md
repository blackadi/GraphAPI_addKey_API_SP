# MS Graph API: Add/Renew key cerdentional (certificate) _API [servicePrincipal:addKey](https://docs.microsoft.com/en-us/graph/api/serviceprincipal-addkey?view=graph-rest-1.0&tabs=http)_.

By using this API, we can add a new certficate to any servicePrincipal. The idea is to automate rolling servicePrincipal expiring keys via [addKey](https://docs.microsoft.com/en-us/graph/api/serviceprincipal-addkey?view=graph-rest-1.0&tabs=http) or [removeKey](https://docs.microsoft.com/en-us/graph/api/serviceprincipal-removekey?view=graph-rest-1.0&tabs=http).

## Add a certificate to an existing servicePrincipal

> **:no_entry: ServicePrincipals that don’t have any existing valid certificates (i.e.: no certificates have been added yet, or all certificates have expired), won’t be able to use this service action. [Update servicePrincipal](https://docs.microsoft.com/en-us/graph/api/serviceprincipal-update?view=graph-rest-1.0&tabs=http) can be used to perform an update instead.**

### Prerequisite
- Valid certificate. (_For the sake of this example, we will use a self-signed certificate_)
- Consent to the needed [permissions](https://docs.microsoft.com/en-us/graph/api/serviceprincipal-addkey?view=graph-rest-1.0&tabs=http#permissions).
- [Registered application](https://docs.microsoft.com/en-us/graph/auth-register-app-v2).
- Rest API Client tool.   

### [Create a new self-signed public certificate to authenticate your application](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate)

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

To enable certificate credentials instead of client secret for authentication, we need to generate a JSON Web Token (JWT) assertion signed with a certificate that the application/servicePrincipal owns.

### [Use JSON Web Token (JWT) assertion signed with a certificate that the application owns](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-certificate-credentials)

- We need to create a signed jwt token (aka Client Assertion).
- Then, get an Access Token Using [Client Credentials Grant Flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow#second-case-access-token-request-with-a-certificate).

  To compute the assertion, you can use one of the many JWT libraries in the language of your choice - [MSAL supports this using .WithCertificate()](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-client-assertions). The information is carried by the token in its Header, Claims, and Signature.

  Please refer back to [the official code sample](https://docs.microsoft.com/en-us/graph/application-rollkey-prooftoken).


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
            string pfxFilePath = "<Path to your certificate file>";
            string password = "<Certificate password>";
            string objectId = "<id of the application or servicePrincipal object>"; // use {CLIENT_ID} for creating client_assertion and use {ObjectID} for PoP token creation
            Guid guid = Guid.NewGuid();

            // Get signing certificate
            X509Certificate2 signingCert = new X509Certificate2(pfxFilePath, password);

            // audience
            // string aud = "https://login.microsoftonline.com/{tenantId}/v2.0"; // uncomment this for {client_assertion}
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

Make sure to change the values to match to your configurations, see below screenhost.

- To get the correct client assertion, uncomment everything from the code where it says for creating client assertion.

  ![Package Structure](images/Generate_client_assertion.PNG)

  ![Package Structure](images/Generate_client_assertion_result.PNG)

  ![Package Structure](images/AccessTokenRequestwithCertificate.PNG)

  Upon a successful HTTP request, the `access_token` will be returned, as shown on the above screenshot.

- Generating proof of possession tokens for rolling keys.

  > :information_source: Authentication_MissingOrMalformed error will be returned if PoP is not signed with the already uploaded certificate.

  **_The provided code in this tutorial can be used to extract certificate info as follow:_**

  - Add your certificate path to `pfxFilePath` variable as follow :

    ![Package Structure](images/GetNewCertInfo.PNG)
  
  - On successful code execution, the below result will be returned:

    ![Package Structure](images/GetNewCertInfo2.PNG)

  - Generate the request body as documented [here](https://docs.microsoft.com/en-us/graph/api/serviceprincipal-addkey?view=graph-rest-1.0&tabs=http#request-body)

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

### Generateing the proof of possession tokens.

- To enable the provided code in this tutorial to return a valid PoP token, We need to re-configure the below variables as follow:

  ![Package Structure](images/GetPoP1.PNG)

- On successful code execution, the below result will be returned:

  ![Package Structure](images/GetPoP2.PNG)

- Add the PoP token in the request body (generated from the previous step) under the `proof` section, as follow:

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
    -------------------------------------------------------------
                        THIS SECTION
    -------------------------------------------------------------
    "proof": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjczMEQ2OUUwQzVFMDFCOTIyODc3MzU4QTg5NEQ0QjVDQTYwNTNGRDMiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJjdzFwNE1YZ0c1SW9keldLaVUxTFhLWUZQOU0ifQ.eyJhdWQiOiIwMDAwMDAwMi0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiIyMTI2YjA4Mi0xODBhLTQ1MjUtODMwMC1lYTY0NjU3NjljNDEiLCJleHAiOjE2Mjk2MzY5NDgsIm5iZiI6MTYyOTYzNjM0OCwiaWF0IjoxNjI5NjM2MzQ4fQ.OWfNEq1oHaxKzwipajhMDMNGihJtF8gYTlXqxy85TXrHx2HGrg3y5uJI-H5MZ5rgrXfuUpFpgmuKIjLT0RDhle2TdtgavLXALYRXBTHl8bgo17IkwIAZeP5oW3HeU7kph7i4pSdG31474tL1vKxqLDr6IaeJklzcC0B7BbL1Bybs147er98XACeJ9k74kEPEm7hP0kOEbzJwm5VfCN2A96wpBCsHRD96bdb5il9OOBbX9FFb536VHNfRSau32LjHPPar0oAgmNj4Zs9penDzvB-j5yBPGiMQguWxDL7L07iiUAnU-frbG2mNh_XjjjWLDKgOgP4mx0_FjsCwEATxRg" 
    -------------------------------------------------------------
                        THIS SECTION
    -------------------------------------------------------------
  }
  ```


### Finally, call the API.

**All the required properties have been added to the request body and a successful `200 OK` response code should be returned.**

- The result of adding a new key credential (certificate) to a servicePrincipal is shown below:

  ![Package Structure](images/NewCertUploaded.PNG)

- To show the newly added certificate, we can do a `GET` request to the following Graph API, see below screenshots.
  
  ```http
  https://graph.microsoft.com/v1.0/servicePrincipals/2126b082-180a-4525-8300-ea6465769c41
  ```

  ![Package Structure](images/NewCertUploaded2.PNG)

> **:warning: We can use the [removeKey](https://docs.microsoft.com/en-us/graph/api/serviceprincipal-removekey?view=graph-rest-1.0&tabs=http) API to remove expired/unwanted certificate from a servicePrincipal.**