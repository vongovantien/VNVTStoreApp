# 🔐 Deployment Secrets Configuration

## Overview
Security-sensitive configuration values (JWT secrets, connection strings, API keys) must **NEVER** be committed to source control. This document explains how to configure secrets for different environments.

---

## 🏠 **Development Environment**

### Using .NET User Secrets

User Secrets stores sensitive data outside your project directory, preventing accidental commits.

**Setup:**
```bash
cd VNVTStore.Backend/src/VNVTStore.API

# Set JWT Secret (minimum 32 characters)
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_STRONG_SECRET_KEY_HERE_MIN_32_CHARS"

# Set JWT Issuer & Audience
dotnet user-secrets set "JwtSettings:Issuer" "VNVTStore"
dotnet user-secrets set "JwtSettings:Audience" "VNVTStoreUsers"

# Optional: Database connection (if different from appsettings)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=shoppingdb;Username=postgres;Password=YOUR_PASSWORD"
```

**Verify:**
```bash
dotnet user-secrets list
```

**Location:**
- Windows: `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- Linux/Mac: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

**UserSecretsId** is defined in `VNVTStore.API.csproj`:
```xml
<UserSecretsId>526b9549-5cbc-4fb6-9535-1fdcf3c7e6e4</UserSecretsId>
```

---

## 🧪 **Testing Environment**

Test secrets are defined in `appsettings.Testing.json` and `VNVTStore.IntegrationTests/appsettings.json`. These use hardcoded values acceptable for automated testing.

**⚠️ Never use testing secrets in production!**

---

## ☁️ **Production Environment**

### **Option 1: Environment Variables (Recommended for Docker/VPS)**

Set environment variables on your hosting platform:

**Linux/Mac:**
```bash
export JwtSettings__SecretKey="YOUR_PRODUCTION_SECRET_KEY_MIN_32_CHARS"
export JwtSettings__Issuer="VNVTStore"
export JwtSettings__Audience="VNVTStoreUsers"
export ConnectionStrings__DefaultConnection="Host=prod-db.example.com;Database=vnvtstore;Username=app;Password=STRONG_PASSWORD"
```

**Windows (PowerShell):**
```powershell
$env:JwtSettings__SecretKey="YOUR_PRODUCTION_SECRET_KEY_MIN_32_CHARS"
$env:JwtSettings__Issuer="VNVTStore"
$env:JwtSettings__Audience="VNVTStoreUsers"
```

**Docker Compose:**
```yaml
services:
  api:
    image: vnvtstore-api:latest
    environment:
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
      - JwtSettings__Issuer=VNVTStore
      - JwtSettings__Audience=VNVTStoreUsers
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
    env_file:
      - .env.production  # NEVER commit this file!
```

**`.env.production` example (add to .gitignore):**
```env
JWT_SECRET_KEY=SuperStrongProductionSecretKeyHere123!@#
DB_CONNECTION_STRING=Host=prod-db;Database=vnvtstore;Username=app;Password=SecurePass
```

---

### **Option 2: Azure Key Vault (Recommended for Azure)**

**Install Package:**
```bash
dotnet add package Azure.Identity
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
```

**Update Program.cs:**
```csharp
if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["KeyVault:Endpoint"]!);
    builder.Configuration.AddAzureKeyVault(
        keyVaultEndpoint,
        new DefaultAzureCredential());
}
```

**Create Secrets in Azure Key Vault:**
```bash
az keyvault secret set --vault-name "vnvtstore-keyvault" --name "JwtSettings--SecretKey" --value "YOUR_SECRET"
az keyvault secret set --vault-name "vnvtstore-keyvault" --name "ConnectionStrings--DefaultConnection" --value "YOUR_CONNECTION"
```

**appsettings.Production.json:**
```json
{
  "KeyVault": {
    "Endpoint": "https://vnvtstore-keyvault.vault.azure.net/"
  }
}
```

---

### **Option 3: AWS Secrets Manager**

**Install Package:**
```bash
dotnet add package AWSSDK.SecretsManager
```

**Retrieve at Runtime:**
```csharp
var secretArn = builder.Configuration["AWS:SecretArn"];
var client = new AmazonSecretsManagerClient();
var response = await client.GetSecretValueAsync(new GetSecretValueRequest
{
    SecretId = secretArn
});
var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
builder.Configuration["JwtSettings:SecretKey"] = secrets["JwtSecretKey"];
```

---

## 🔑 **JWT Secret Requirements**

✅ **Minimum 32 characters** (256 bits for HS256)  
✅ **Use cryptographically random values**  
✅ **Unique per environment**  
✅ **Rotate regularly** (every 90 days recommended)

**Generate Strong Secret (PowerShell):**
```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

**Generate Strong Secret (Linux/Mac):**
```bash
openssl rand -base64 32
```

---

## 📋 **Configuration Priority**

.NET Core loads configuration in this order (later sources override earlier):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments

**Best Practice:** Use User Secrets (dev), Environment Variables (production), or Key Vault (cloud).

---

## ✅ **Verification Checklist**

- [ ] JWT secrets removed from `appsettings.json`
- [ ] Development secrets configured via User Secrets
- [ ] Production deployment uses environment variables or Key Vault
- [ ] `.env*` files added to `.gitignore`
- [ ] Secrets rotated before production deployment
- [ ] Team members instructed to run `dotnet user-secrets set` commands
- [ ] CI/CD pipeline configured with secret injection

---

## 🚨 **If Secrets Were Accidentally Committed**

1. **Rotate immediately** - generate new secrets
2. **Remove from Git history:**
   ```bash
   git filter-branch --force --index-filter \
     "git rm --cached --ignore-unmatch appsettings.Production.json" \
     --prune-empty --tag-name-filter cat -- --all
   ```
3. **Force push** (coordinate with team):
   ```bash
   git push origin --force --all
   ```

---

## 📚 **References**

- [Safe Storage of App Secrets in Development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault Configuration Provider](https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
