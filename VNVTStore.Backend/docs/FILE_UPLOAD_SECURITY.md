# 🛡️ File Upload Security

## Overview
File uploads are a common attack vector. This document describes the security measures implemented to protect against malicious file uploads.

---

## ✅ **Implemented Security Measures**

### 1. **File Size Validation**
- **Max Image Size:** 5MB
- **Max General File Size:** 10MB
- Prevents DoS attacks via large file uploads

### 2. **Extension Whitelist**
- **Allowed Image Extensions:** `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`
- Blocks executable files (`.exe`, `.bat`, `.sh`, `.dll`, etc.)
- Case-insensitive matching

### 3. **MIME Type Validation**
- **Allowed MIME Types:** `image/jpeg`, `image/png`, `image/webp`, `image/gif`
- Validates `Content-Type` header
- Prevents MIME type spoofing

### 4. **Magic Number Verification** ✅
- **What:** Reads first bytes of file to verify actual file type
- **Why:** Attackers can rename `malware.exe` to `image.jpg` - extension/MIME checks won't catch this
- **How:** Compares file signature against known image signatures:
  - **JPEG:** `FF D8 FF`
  - **PNG:** `89 50 4E 47 0D 0A 1A 0A`
  - **GIF87a:** `47 49 46 38 37 61`
  - **GIF89a:** `47 49 46 38 39 61`
  - **WebP:** `52 49 46 46` (RIFF container)

**Example Attack Blocked:**
```
File: malware.exe → renamed to → photo.jpg
Extension check: ✅ PASS (.jpg is allowed)
MIME check: ⚠️ Could be spoofed
Magic number check: ❌ FAIL (reads EXE signature: 4D 5A instead of JPEG FF D8 FF)
Result: Upload REJECTED ✅
```

### 5. **Filename Sanitization**
- Removes path traversal patterns (`..`, `./`, `../`)
- Strips invalid filesystem characters
- Limits filename length to 100 characters
- Adds timestamp + hash to prevent collisions
- **Example:**
  - Input: `../../etc/passwd.jpg`
  - Output: `passwd_1705684523123_a3f8d2c1.jpg`

### 6. **Rate Limiting**
- `ExpensiveLimit` policy applied: **3 uploads per 30 seconds**
- Prevents bulk upload abuse

### 7. **Content-Length Validation**
- Rejects empty files (`Length == 0`)
- Validates declared size matches actual size

---

## 🔴 **Optional: Antivirus Scanning**

For production environments with high security requirements, integrate antivirus scanning.

### **Option 1: ClamAV (Open Source)**

**Install ClamAV:**
```bash
# Ubuntu/Debian
sudo apt-get install clamav clamav-daemon

# Windows (via Docker)
docker run -d -p 3310:3310 clamav/clamav:latest
```

**Add Package:**
```bash
dotnet add package nClam
```

**Implementation:**
```csharp
using nClam;

public interface IAntivirusService
{
    Task<Result> ScanFileAsync(Stream fileStream, string fileName);
}

public class ClamAVService : IAntivirusService
{
    private readonly ClamClient _clamClient;

    public ClamAVService()
    {
        _clamClient = new ClamClient("localhost", 3310);
    }

    public async Task<Result> ScanFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            // Ping ClamAV to check if it's running
            var pingResult = await _clamClient.PingAsync();
            if (!pingResult)
                return Result.Failure(Error.Failure("Antivirus service unavailable"));

            // Scan file
            fileStream.Position = 0;
            var scanResult = await _clamClient.SendAndScanFileAsync(fileStream);

            if (scanResult.Result == ClamScanResults.VirusDetected)
                return Result.Failure(Error.Validation($"Malware detected: {scanResult.InfectedFiles?.FirstOrDefault()?.VirusName}"));

            if (scanResult.Result == ClamScanResults.Error)
                return Result.Failure(Error.Failure("Antivirus scan error"));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure($"Antivirus scan failed: {ex.Message}"));
        }
    }
}
```

**Register Service:**
```csharp
services.AddScoped<IAntivirusService, ClamAVService>();
```

**Use in UploadController:**
```csharp
// After file validation
var antivirusScanResult = await _antivirusService.ScanFileAsync(stream, file.FileName);
if (antivirusScanResult.IsFailure)
{
    _logger.LogWarning("Malware detected in upload: {FileName}", file.FileName);
    return BadRequest(new { error = "File contains malicious content" });
}
```

---

### **Option 2: VirusTotal API**

**Add Package:**
```bash
dotnet add package VirusTotalNet
```

**Implementation:**
```csharp
using VirusTotalNet;

public class VirusTotalService : IAntivirusService
{
    private readonly VirusTotal _virusTotal;

    public VirusTotalService(IConfiguration configuration)
    {
        var apiKey = configuration["VirusTotal:ApiKey"];
        _virusTotal = new VirusTotal(apiKey);
    }

    public async Task<Result> ScanFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            // Upload file to VirusTotal
            var report = await _virusTotal.ScanFileAsync(fileStream, fileName);

            // Wait for scan results (takes 15-30 seconds)
            await Task.Delay(20000);

            // Get report
            var scanReport = await _virusTotal.GetFileReportAsync(report.Resource);

            if (scanReport.Positives > 0)
                return Result.Failure(Error.Validation($"Malware detected by {scanReport.Positives} engines"));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure($"VirusTotal scan failed: {ex.Message}"));
        }
    }
}
```

**⚠️ Considerations:**
- **Rate Limits:** Free tier = 4 requests/minute
- **Latency:** Scan takes 15-30 seconds
- **Privacy:** File uploaded to third-party
- **Cost:** Premium API for high volume

---

### **Option 3: Azure Content Safety**

```bash
dotnet add package Azure.AI.ContentSafety
```

```csharp
using Azure;
using Azure.AI.ContentSafety;

public class AzureContentSafetyService : IAntivirusService
{
    private readonly ContentSafetyClient _client;

    public AzureContentSafetyService(IConfiguration configuration)
    {
        var endpoint = configuration["Azure:ContentSafety:Endpoint"];
        var apiKey = configuration["Azure:ContentSafety:ApiKey"];
        _client = new ContentSafetyClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
    }

    public async Task<Result> ScanFileAsync(Stream fileStream, string fileName)
    {
        // Azure Content Safety API for image analysis
        var imageData = BinaryData.FromStream(fileStream);
        var request = new AnalyzeImageOptions(imageData);

        var response = await _client.AnalyzeImageAsync(request);

        // Check for harmful content
        if (response.Value.HateSeverity > 2 || 
            response.Value.ViolenceSeverity > 2 ||
            response.Value.SelfHarmSeverity > 2 ||
            response.Value.SexualSeverity > 2)
        {
            return Result.Failure(Error.Validation("Image contains inappropriate content"));
        }

        return Result.Success();
    }
}
```

---

## 🧪 **Testing File Upload Security**

### **Test 1: Extension Bypass Attempt**
```bash
# Create malicious file with image extension
echo "<?php system(\$_GET['cmd']); ?>" > shell.jpg

# Upload
curl -X POST http://localhost:5178/api/v1/upload \
  -F "file=@shell.jpg" \
  -H "Content-Type: multipart/form-data"

# Expected: ❌ REJECTED (magic number mismatch)
```

### **Test 2: MIME Type Spoofing**
```bash
# Upload EXE with spoofed MIME type
curl -X POST http://localhost:5178/api/v1/upload \
  -F "file=@malware.exe;type=image/jpeg" \
  -H "Content-Type: multipart/form-data"

# Expected: ❌ REJECTED (magic number: 4D 5A instead of FF D8 FF)
```

### **Test 3: Directory Traversal**
```bash
# Upload with path traversal filename
curl -X POST http://localhost:5178/api/v1/upload \
  -F "file=@image.jpg;filename=../../etc/passwd.jpg" \
  -H "Content-Type: multipart/form-data"

# Expected: ✅ ACCEPTED (filename sanitized to: passwd_timestamp_hash.jpg)
```

### **Test 4: Oversized File**
```bash
# Create 10MB file
dd if=/dev/zero of=large.jpg bs=1M count=10

# Upload
curl -X POST http://localhost:5178/api/v1/upload \
  -F "file=@large.jpg" \
  -H "Content-Type: multipart/form-data"

# Expected: ❌ REJECTED (exceeds 5MB limit)
```

### **Test 5: Valid Image**
```bash
# Upload legitimate image
curl -X POST http://localhost:5178/api/v1/upload \
  -F "file=@photo.jpg" \
  -H "Content-Type: multipart/form-data"

# Expected: ✅ ACCEPTED
```

---

## 📋 **Security Checklist**

- [x] File size validation
- [x] Extension whitelist
- [x] MIME type validation
- [x] Magic number verification (file signature)
- [x] Filename sanitization
- [x] Rate limiting
- [x] Content-Length validation
- [ ] Antivirus scanning (optional, recommended for production)
- [ ] Image metadata stripping (EXIF removal)
- [ ] CDN integration for serving uploaded files
- [ ] Separate storage domain (prevents XSS via uploaded HTML)

---

## 🔗 **References**

- [OWASP File Upload Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html)
- [File Signatures (Magic Numbers)](https://en.wikipedia.org/wiki/List_of_file_signatures)
- [ClamAV Documentation](https://docs.clamav.net/)
- [Azure Content Safety](https://learn.microsoft.com/en-us/azure/ai-services/content-safety/)
