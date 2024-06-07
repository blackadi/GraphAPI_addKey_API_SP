# A DOTNET Core 3.1 console application sample calling MS Graph API to add a key credential to an application

## For this demo we will use [addKey API](https://docs.microsoft.com/en-us/graph/api/application-addkey?view=graph-rest-1.0&tabs=http) for `application`, but the code can be changed to use `service principal` instead.

> This sample provides the ability to either call the `addKey` API using _graph SDK_ or _directly calling the graph API_.
> [More info here](https://github.com/blackadi/GraphAPI_addKey_API_SP/wiki)

## Running the sample

### Step 1: Clone this repository

From your shell or command line:

```Shell
git clone https://github.com/blackadi/GraphAPI_addKey_API_SP.git
```

### Step 2: Register the sample with your Azure Active Directory tenant

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app.
   - In the **Supported account types** section, select **Accounts in this organizational directory only ({tenant name})**.
   - Click **Register** button at the bottom to create the application.
1. On the application **Overview** page, find the **Application (client) ID** and **Directory (tenant) ID** values and record it for later. You'll need it to configure the configuration file(s) later in your code.
1. From the **Certificates & secrets** page, in the **Certificates** section, choose **Upload certificate**:

   - Select the certificate file you want to upload. It must be one of the following file types: _.cer, .pem, .crt_.
   
      >**_Please be advised that this certificate will be utilized for authentication purposes with Microsoft Entra ID in place of client secrets_**.
   - Select **Add**.

1. In the Application menu blade, click on the **API permissions** in the left to open the page where we add access to the Apis that your application needs.

   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the _Commonly used Microsoft APIs_ section, click on **Microsoft Graph**
   - In the **Application permissions** section, ensure that the right permissions are checked: **Application.ReadWrite.OwnedBy**
   - Select the **Add permissions** button at the bottom.

1. At this stage, the permissions are assigned correctly but since the client app does not allow users to interact, the user's themselves cannot consent to these permissions.
   To get around this problem, we'd let the [tenant administrator consent on behalf of all users in the tenant](https://docs.microsoft.com/azure/active-directory/develop/v2-admin-consent).
   Click the **Grant admin consent for {tenant}** button, and then select **Yes** when you are asked if you want to grant consent for the requested permissions for all account in the tenant.
   You need to be the tenant admin to be able to carry out this operation.

### Step 3: Create a private key and certificate

- You can follow the instruction [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate#option-2-create-and-export-your-public-certificate-with-its-private-key), upload a valid certificate as it's needed when calling addKey API.

   > Applications without any existing valid certificates (either no certificates have been added or all certificates have expired) will not be able to utilize this code sample. Instead, you can use the Update application operation via the Graph API to modify the `keyCredentials` property, or you can upload your valid certificates through the Azure portal as detailed [here](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app#add-a-certificate).

Finally, go back to the Azure portal. In the Application menu blade, click on the **Certificates & secrets**, in the **Certificates** section, upload the certificate you created.

### Step 4: Configure the sample app to use your app registration

Open the project in your IDE (like Visual Studio) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `appsettings.json` file
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) value you recorded earlier from the Azure portal.
1. Find the app key `TenantId` and replace the existing value with the directory (tenant) ID value you recorded earlier from the Azure portal.
1. Find the app key `ObjectId` and replace the existing value with your app registration (Object ID) value which can be found from the Azure portal.
1. Find the app key `Aud_ClientAssertion` and replace `{YOUR_TENANT_ID_HERE}` with the directory (tenant) ID value you recorded earlier from the Azure portal.
1. Find the app key `CertificateDiskPath` and replace the existing value with your exising self-signed certificate, for more info see [this](https://github.com/blackadi/GraphAPI_addKey_API_SP/blob/main/cert%20which%20is%20uploaded%20to%20azure/readme.md).
1. Find the app key `CertificatePassword` and replace the existing value with your exising self-signed certificate password, for more info see [this](https://github.com/blackadi/GraphAPI_addKey_API_SP/blob/main/cert%20which%20is%20uploaded%20to%20azure/readme.md).
1. Find the app key `NewCertificateDiskPath` and replace the existing value with your new self-signed certificate, for more info see [this](https://github.com/blackadi/GraphAPI_addKey_API_SP/blob/main/cert%20which%20is%20uploaded%20to%20azure/readme.md).
1. Find the app key `NewCertificatePassword` and replace the existing value with your new self-signed certificate password, for more info see [this](https://github.com/blackadi/GraphAPI_addKey_API_SP/blob/main/cert%20which%20is%20uploaded%20to%20azure/readme.md).

### Step 5: Run the sample

> Clean the solution, rebuild the solution, and run it.

```console
    dotnet run
```

## About this sample

- The code will generate `client_assertion` first, then will get `access_token` using [client credentials flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow#second-case-access-token-request-with-a-certificate)

- a `proof of possession` token will be generated and this JWT token must be signed using the private key of the application existing valid certificates.

- Extract the `key` value of the new certificate which will be uploaded via [addKey API](https://docs.microsoft.com/en-us/graph/api/application-addkey?view=graph-rest-1.0&tabs=http) request body.

- Finally, call the API.

> :warning: The certificates used in this sample are for testing purposes only.
