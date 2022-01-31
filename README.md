# A DOTNET Core 3.1 console application sample calling MS Graph API to add a key credential to an application

## For this demo we will use [addKey API](https://docs.microsoft.com/en-us/graph/api/application-addkey?view=graph-rest-1.0&tabs=http) for `application`, but the code can be changed to use `service principal` instead.

> This sample provides the ability to either call the `addKey` API using _graph SDK_ or _directly calling the graph API_.

- First, upload a valid certificate as it's needed to enable addAPI key.

  > Applications that don’t have any existing valid certificates (no certificates have been added yet, or all certificates have expired), won’t be able to use this service action. You can use the Update application operation to perform an update instead.

- Second, generate `accessToken` based on a vaild certificate (For the sake of this demo we will use `uploadedCert.pfx` which is uploaded to azure).

  1. Generate `client_assertion` via the C# code.
  1. Generate `access_token`

- Third, generate `proof of possession` token.

- Forth, get the `key` value of the new certificate which will be used by the [addKey API](https://docs.microsoft.com/en-us/graph/api/application-addkey?view=graph-rest-1.0&tabs=http) request body.

- Finally, call the API.

## Running the sample

Clone this repository `git clone https://github.com/blackadi/GraphAPI_addKey_API_SP.git`

> Clean the solution, rebuild the solution, and run it.

```console
    dotnet run
```
