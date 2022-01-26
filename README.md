# MS Graph API - {application: addKey}

## For this demo we will use addKey API for `application` instead of service principal for visual effect simplicity using `azure portal` to validate certificate upload.

- First, upload a valid certificate as it's needed to enable addAPI key.

  > Applications that don’t have any existing valid certificates (no certificates have been added yet, or all certificates have expired), won’t be able to use this service action. You can use the Update application operation to perform an update instead.

- Second, generate `accessToken` based on a vaild certificate (For the sake of this demo we will use `newCertToUpload.cer` which is uploaded to azure).

  1. Generate `client_assertion` via the C# code.
  1. Generate `access_token`

- Third, generate `proof of possession` token.

- Forth, get the `key` value for the new certificate.

- Finally, call the API.

[![MS Graph API - servicePrincipal:addKey](images/youtube.jpg)](https://youtu.be/MOS4L4mMZmc "MS Graph API - servicePrincipal:addKey")
