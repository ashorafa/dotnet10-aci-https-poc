# TLS Certificate Management

This directory contains information about managing TLS certificates for HTTPS in ACI.

## Certificate Generation

### Option 1: Self-Signed Certificate (for PoC/Testing)

Generate a self-signed certificate valid for 365 days:

```bash
openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem -days 365 -nodes \
  -subj "/C=US/ST=State/L=City/O=Organization/CN=localhost"
```

Convert to PKCS12 format (PFX) for .NET:

```bash
openssl pkcs12 -export -out cert.pfx -inkey key.pem -in cert.pem \
  -password pass:changeme
```

### Option 2: Using PowerShell (Windows)

```powershell
# Generate self-signed certificate
$cert = New-SelfSignedCertificate -CertStoreLocation "cert:\CurrentUser\My" `
  -DnsName "localhost" -FriendlyName "DotNetACI" -NotAfter (Get-Date).AddYears(1)

# Export to PFX
$password = ConvertTo-SecureString -String "changeme" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "cert.pfx" -Password $password
```

### Option 3: Using `generate-cert.sh` Script

Run the included script:

```bash
cd ../../scripts
./generate-cert.sh
# Output: cert.pfx in this directory
```

---

## Certificate Deployment to ACI

### Method 1: Include in Docker Image

**Pros:**
- No external dependencies
- Immutable configuration
- Faster startup

**Cons:**
- Certificate baked into image
- Certificate rotation requires new image

**Steps:**
1. Place `cert.pfx` in this directory
2. Docker build copies it (see `docker/Dockerfile`)
3. Environment variables point to `/app/certs/cert.pfx`

### Method 2: Mount from Azure File Share

**Pros:**
- Certificate shared across instances
- Easy rotation without rebuild

**Cons:**
- Additional storage dependency
- Slightly slower startup

**Steps:**
1. Upload `cert.pfx` to Azure Storage File Share
2. Mount in ACI via ARM template `volumes`
3. Set environment variable to mounted path

### Method 3: Azure Key Vault

**Pros:**
- Secure certificate storage
- Audit trail
- Rotation management

**Cons:**
- Requires managed identity
- More complex setup

**Steps:**
1. Upload certificate to Key Vault
2. Configure managed identity for ACI
3. Download at container startup via init script

---

## Using the Certificate

### Environment Variables

Set these in ACI:

```
Kestrel:Certificates:Default:Path=/app/certs/cert.pfx
Kestrel:Certificates:Default:Password=changeme
```

### Program.cs Configuration

The `Program.cs` reads these environment variables:

```csharp
var certificatePath = Environment.GetEnvironmentVariable("Kestrel:Certificates:Default:Path");
var certificatePassword = Environment.GetEnvironmentVariable("Kestrel:Certificates:Default:Password");

options.ListenAnyIP(443, listenOptions =>
{
    var certificate = new X509Certificate2(certificatePath, certificatePassword);
    listenOptions.UseHttps(certificate);
});
```

---

## Certificate Security Best Practices

### For Production

1. **Use Azure Key Vault** instead of embedding
   - Never commit certificates to git
   - Use managed identities for access
   - Enable audit logging

2. **Use Cert Authority Certificates**
   - Purchase or use Let's Encrypt
   - Avoid self-signed for production

3. **Rotate Regularly**
   - Set up automation for renewal
   - Monitor expiration dates

4. **Limit Permissions**
   - Only ACI container should access cert
   - Restrict Key Vault access

### For This PoC

- Self-signed certificate is acceptable
- Store in Docker image (simplicity)
- Password: `changeme` (for demo purposes)
- Valid for 365 days from generation

---

## Troubleshooting

### Certificate Not Found Error

```
Certificate not found at: /app/certs/cert.pfx
```

**Solution:** Ensure certificate file is in the directory and copied by Dockerfile

### Invalid Certificate Format

```
System.Security.Cryptography.CryptographicException: Invalid certificate format
```

**Solution:** Certificate must be PFX format, not PEM

### Port 443 Already in Use

```
System.Net.Sockets.SocketException: Address already in use
```

**Solution:** Check if another process is using port 443, or ensure ACI port mapping is correct

### Certificate Password Error

```
System.Security.Cryptography.CryptographicException: Mac verification failed
```

**Solution:** Ensure `Kestrel:Certificates:Default:Password` matches the PFX password

---

## Testing HTTPS

### From Local Machine

```bash
# Test with curl (ignore certificate verification for self-signed)
curl -k https://your-container-dns:443/health

# Test with PowerShell
$uri = "https://your-container-dns:443/health"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
Invoke-RestMethod -Uri $uri
```

### From Azure CLI

```bash
az container logs --resource-group my-rg --name dotnet-api
```

---

## References

- [ASP.NET Core Kestrel HTTPS](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/endpoints)
- [X.509 Certificate Configuration in .NET](https://learn.microsoft.com/dotnet/api/system.security.cryptography.x509certificates.x509certificate2)
- [Azure Container Instances Security](https://learn.microsoft.com/azure/container-instances/container-instances-environment-variables)
