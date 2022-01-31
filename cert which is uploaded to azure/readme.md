# [Option 1: Create and export your public certificate without a private key](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate#option-1--create-and-export-your-public-certificate-without-a-private-key)

Use the certificate you create using this method to authenticate from an application running from your machine. For example, authenticate from Windows PowerShell.

In an elevated PowerShell prompt, run the following command and leave the PowerShell console session open. Replace `{certificateName}` with the name that you wish to give to your certificate.

```ps
$cert = New-SelfSignedCertificate -Subject "CN={certificateName}" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256    ## Replace {certificateName}
```

The **$cert** variable in the previous command stores your certificate in the current session and allows you to export it. The command below exports the certificate in `.cer` format. You can also export it in other formats supported on the Azure portal including `.pem` and `.crt`.

```ps
Export-Certificate -Cert $cert -FilePath "C:\Users\admin\Desktop\{certificateName}.cer"   ## Specify your preferred location and replace {certificateName}
```

Your certificate is now ready to upload to the Azure portal. Once uploaded, retrieve the certificate thumbprint for use to authenticate your application.

# [Option 2: Create and export your public certificate with its private key](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate#option-2-create-and-export-your-public-certificate-with-its-private-key)

Use this option to create a certificate and its private key if your application will be running from another machine or cloud, such as Azure Automation.

In an elevated PowerShell prompt, run the following command and leave the PowerShell console session open. Replace `{certificateName}` with name that you wish to give your certificate.

```ps
$cert = New-SelfSignedCertificate -Subject "CN={certificateName}" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256    ## Replace {certificateName}
```

The $cert variable in the previous command stores your certificate in the current session and allows you to export it. The command below exports the certificate in `.cer` format. You can also export it in other formats supported on the Azure portal including `.pem` and `.crt`.

```ps
Export-Certificate -Cert $cert -FilePath "C:\Users\admin\Desktop\{certificateName}.cer"   ## Specify your preferred location and replace {certificateName}
```

Still in the same session, create a password for your certificate private key and save it in a variable. In the following command, replace `{myPassword}` with the password that you wish to use to protect your certificate private key.

```ps
$mypwd = ConvertTo-SecureString -String "{myPassword}" -Force -AsPlainText  ## Replace {myPassword}
```

Now, using the password you stored in the `$mypwd` variable, secure, and export your private key.

```ps
Export-PfxCertificate -Cert $cert -FilePath "C:\Users\admin\Desktop\{privateKeyName}.pfx" -Password $mypwd   ## Specify your preferred location and replace {privateKeyName}
```

Your certificate (`.cer` file) is now ready to upload to the Azure portal. You also have a private key (`.pfx` file) that is encrypted and can't be read by other parties. Once uploaded, retrieve the certificate thumbprint for use to authenticate your application.

# [Optional task: Delete the certificate from the keystore.](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate#optional-task-delete-the-certificate-from-the-keystore)

If you created the certificate using Option 2, you can delete the key pair from your personal store. First, run the following command to retrieve the certificate thumbprint.

```ps
Get-ChildItem -Path "Cert:\CurrentUser\My" | Where-Object {$_.Subject -Match "{certificateName}"} | Select-Object Thumbprint, FriendlyName    ## Replace {privateKeyName} with the name you gave your certificate
```

Then, copy the thumbprint that is displayed and use it to delete the certificate and its private key.

```ps
Remove-Item -Path Cert:\CurrentUser\My\{pasteTheCertificateThumbprintHere} -DeleteKey
```

# [Know your certificate expiry date](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-self-signed-certificate#know-your-certificate-expiry-date)

The self-signed certificate you created following the steps above has a limited lifetime before it expires. In the **App registrations** section of the Azure portal, the **Certificates & secrets** screen displays the expiration date of the certificate. If you're using Azure Automation, the Certificates screen on the Automation account displays the expiration date of the **certificate**. Follow the previous steps to create a new self-signed certificate.
