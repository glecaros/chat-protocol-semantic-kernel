
# Setting up dev certificates

When developing applications that require HTTPS, one effective method is to use `OpenSSL` for generating self-signed certificates. This approach is widely used due to its flexibility and compatibility across different platforms. The following sections will guide you through the process of generating and trusting these certificates using `OpenSSL`.

> [!CAUTION]
> **Important Note**: This process is intended for development purposes only. Using self-signed certificates in a production environment is not recommended as it can expose your application to security vulnerabilities.

## Generating the Certificate using OpenSSL

### Bash
```bash
echo "[req]" > ./openssl.conf;
echo "distinguished_name=req" >> ./openssl.conf;
echo "[EXT]" >> ./openssl.conf;
echo "subjectAltName=DNS:localhost" >> ./openssl.conf;
openssl req -x509 -out localhost.crt -keyout localhost.key -newkey rsa:2048 -nodes -sha256 -subj '/CN=localhost' -days 365 -extensions EXT -config ./openssl.conf
```

### Powershell

```powershell
$ReqConf = @"
[req]
distinguished_name=req
[EXT]
subjectAltName=DNS:localhost
"@

$ReqConf | Out-File -FilePath .\openssl.cnf -Encoding ascii

openssl req -x509 -out localhost.crt -keyout localhost.key -newkey rsa:2048 -nodes -sha256 -subj '/CN=localhost' -days 365 -extensions EXT -config .\openssl.cnf
```

## Generating the Certificate using Powershell

TODO

## Trusting the Certificate

Since the certificate you just generated is not signed by a trusted certificate authority, browsers will complain about the connection being unsafe. To resolve this we can either configure our browsers or our system to trust the certificate.

### System level

#### Windows

To add your certificate cd

```bash
certutil -addStore "root" <path-to-crt-file>
```
> [!NOTE]
> If you need to remove the certificate at any point, you can use certmgr.exe. Open it, navigate to Trusted Root Certification Authorities > Certificates, find your certificate, and delete it.

#### MacOS

To configure system level trust for your certificate you need to import it into your keychain:

1. Open the `Keychain Access` app.
1. Open your `login` keychain.
1. Import the certificate by going to `File->Import Items` and finding the `crt` file you created.
1. Once imported, double click it; expand the `Trust` section and set the `Secure Socket Layer (SSL)` option to `Always Trust`.
1. Once that is done, close the certificate config window and you will be asked to confirm by authenticating. Done that your certificate is trusted.


#### Linux

TODO


### Browser level config

### Edge/Chrome

Both Microsoft Edge and Google Chrome can be instructed to allow insecure certificates for `localhost`. You can enable this setting by navigating to the following URL in each browser:

- Edge -> `edge://flags/#allow-insecure-localhost`
- Chrome -> `chrome://flags/#allow-insecure-localhost`


### Firefox

TODO