# GarmentsERP — File Storage Strategy

**Version:** 1.0.0
**Date:** 2026-09-24
**Status:** Approved

---

## 1. Overview

GarmentsERP stores multiple types of user-uploaded and system-generated files.
This document defines the storage strategy, folder structure, access pattern, 
security rules, and backup approach for all file assets.

---

## 2. File Categories

| Category               | Examples                                        | Max Size  |
|------------------------|-------------------------------------------------|-----------|
| Company Assets         | Logo, letterhead template                       | 2 MB      |
| Employee Documents     | NID scan, passport, photo, contract             | 5 MB      |
| Procurement Docs       | PO scan, GRN document, supplier invoice scan    | 10 MB     |
| Style / Product Images | Garment photo, tech pack image, sketch          | 10 MB     |
| Compliance Documents   | Audit report, buyer compliance cert, COC        | 20 MB     |
| Attendance Imports     | Biometric CSV/Excel export files                | 5 MB      |
| Cutting Marker Files   | Marker plan image/file                          | 10 MB     |
| Generated PDFs         | QuestPDF-generated reports (PO, payslip, etc.) | 5 MB      |
| Temp/Exports           | Payroll bank CSV, bulk export files             | 10 MB     |

---

## 3. Allowed File Types

| Category             | Allowed Extensions                              |
|----------------------|-------------------------------------------------|
| Images               | .jpg, .jpeg, .png, .webp                        |
| Documents            | .pdf, .doc, .docx, .xls, .xlsx                  |
| Data Import          | .csv, .xlsx                                     |
| Generated Reports    | .pdf only (server-generated, not user-uploaded) |

**Blocked file types:** .exe, .bat, .sh, .ps1, .js, .php, .asp, .dll — all script/executable types are rejected.

---

## 4. Storage Location (Phase 1 — Local Server)

### Root Storage Path (configurable in appsettings.json)
```
D:\GarmentsERP\Storage\
```

### Folder Structure
```
D:\GarmentsERP\Storage\
|
+-- company\
|   +-- {companyId}\
|       +-- logo\                    <- company logo
|       +-- letterhead\              <- report letterhead image
|
+-- employees\
|   +-- {companyId}\
|       +-- {employeeId}\
|           +-- photo\               <- profile photo
|           +-- nid\                 <- NID scan
|           +-- passport\            <- passport scan
|           +-- contracts\           <- employment contract PDF
|           +-- other\               <- other documents
|
+-- procurement\
|   +-- {companyId}\
|       +-- purchase-orders\
|           +-- {poId}\              <- PO attachment scan
|       +-- grn\
|           +-- {grnId}\             <- GRN attachment
|       +-- invoices\
|           +-- {invoiceId}\         <- Supplier invoice scan
|
+-- production\
|   +-- {companyId}\
|       +-- styles\
|           +-- {styleId}\           <- Style/garment images, tech pack
|       +-- cutting\
|           +-- {cuttingOrderId}\    <- Marker plan files
|       +-- compliance\
|           +-- {orderId}\           <- Buyer compliance docs
|
+-- hr\
|   +-- {companyId}\
|       +-- attendance\
|           +-- imports\             <- Biometric CSV import files
|       +-- payroll\
|           +-- {period}\            <- Payroll-related docs
|
+-- reports\
|   +-- {companyId}\
|       +-- generated\               <- QuestPDF auto-generated reports (temp cache)
|       +-- exports\                 <- Exported CSV/Excel files
|
+-- temp\                            <- Temporary upload staging (auto-cleaned daily)
```

---

## 5. Storage Configuration (appsettings.json)

```json
{
  "FileStorage": {
    "Provider": "LocalDisk",
    "LocalDisk": {
      "RootPath": "D:\\GarmentsERP\\Storage",
      "PublicBaseUrl": "https://yourdomain.com/files"
    },
    "AzureBlob": {
      "ConnectionString": "",
      "ContainerName": "garments-erp-files"
    },
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv"],
    "MaxFileSizeBytes": 20971520,
    "GeneratedReportsTempTtlMinutes": 60
  }
}
```

---

## 6. Backend Implementation

### IFileStorageService Interface (Abstraction Layer)

```csharp
// GarmentsERP.Shared/Interfaces/IFileStorageService.cs

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string folder,
        CancellationToken ct = default);

    Task<Stream> DownloadAsync(
        string filePath,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(
        string filePath,
        CancellationToken ct = default);

    string GetPublicUrl(string filePath);
}

public record FileUploadResult(
    bool Success,
    string FilePath,        // relative path stored in DB
    string PublicUrl,
    string FileName,
    long FileSizeBytes,
    string ContentType);
```

### LocalDiskFileStorageService (Phase 1)

```csharp
// GarmentsERP.Infrastructure/Storage/LocalDiskFileStorageService.cs

public class LocalDiskFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly string _publicBaseUrl;

    public async Task<FileUploadResult> UploadAsync(
        Stream fileStream, string fileName, string folder, CancellationToken ct)
    {
        var safeFileName   = $"{Guid.NewGuid():N}_{SanitizeFileName(fileName)}";
        var relativePath   = Path.Combine(folder, safeFileName);
        var absolutePath   = Path.Combine(_rootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var fs = File.Create(absolutePath);
        await fileStream.CopyToAsync(fs, ct);

        return new FileUploadResult(
            Success       : true,
            FilePath      : relativePath,          // stored in DB
            PublicUrl     : GetPublicUrl(relativePath),
            FileName      : safeFileName,
            FileSizeBytes : fs.Length,
            ContentType   : GetContentType(fileName));
    }

    public string GetPublicUrl(string filePath)
        => $"{_publicBaseUrl}/{filePath.Replace('\\', '/')}";

    private static string SanitizeFileName(string name)
        => Regex.Replace(Path.GetFileName(name), @"[^a-zA-Z0-9._-]", "_");
}
```

### How to Register (Module DI)

```csharp
// Register in each module's DI based on Provider config
builder.Services.AddScoped<IFileStorageService, LocalDiskFileStorageService>();
// Future: builder.Services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();
```

---

## 7. API Endpoints for File Upload/Download

```
POST   /api/v1/files/upload                   <- Upload a file
GET    /api/v1/files/{filePath}               <- Download a file (streamed)
DELETE /api/v1/files/{filePath}               <- Delete a file
GET    /api/v1/reports/{reportType}/{id}      <- Download generated PDF (QuestPDF)
```

### Upload Response
```json
{
  "success": true,
  "data": {
    "filePath": "employees/comp-001/emp-abc/photo/abc123_profile.jpg",
    "publicUrl": "https://yourdomain.com/files/employees/comp-001/emp-abc/photo/abc123_profile.jpg",
    "fileName": "abc123_profile.jpg",
    "fileSizeBytes": 204800,
    "contentType": "image/jpeg"
  }
}
```

---

## 8. Database — File Reference Pattern

Files are NOT stored in the database. Only the **relative file path** is stored.

```sql
-- Example: employees table
ALTER TABLE hr.Employees ADD
    PhotoPath        NVARCHAR(500) NULL,   -- relative path
    NidScanPath      NVARCHAR(500) NULL,
    PassportScanPath NVARCHAR(500) NULL,
    ContractPath     NVARCHAR(500) NULL;

-- Example: procurement.PurchaseOrders
ALTER TABLE procurement.PurchaseOrders ADD
    AttachmentPath   NVARCHAR(500) NULL;
```

The backend resolves the path to a full URL using `IFileStorageService.GetPublicUrl(filePath)`.

---

## 9. Security Rules

| Rule                                       | Implementation                                      |
|--------------------------------------------|-----------------------------------------------------|
| Files served only to authenticated users   | Download endpoint requires valid JWT                |
| Files served only to permitted users       | Permission check: user must have Read on that module|
| No direct disk path access via URL         | Files served through API controller, never direct   |
| File type whitelist enforced server-side   | Extension + MIME type checked on upload             |
| File size limit enforced                   | Max 20 MB per file, configured in middleware        |
| File names sanitized                       | GUID prefix + regex sanitization                    |
| Executable files rejected                  | .exe, .bat, .sh, .ps1, etc. always rejected        |
| Virus scan (recommended)                   | Windows Defender or ClamAV integration (future)     |

---

## 10. Serving Static Files (IIS Setup)

Files are NOT served as static files directly by IIS.
They are served through the ASP.NET Core API controller:

```csharp
// FileController.cs
[Authorize]
[ApiController]
[Route("api/v1/files")]
public class FileController : ControllerBase
{
    [HttpGet("{**filePath}")]
    [HasPermission("Files.Read")]
    public async Task<IActionResult> Download(string filePath)
    {
        var stream = await _fileStorage.DownloadAsync(filePath);
        var contentType = GetContentType(filePath);
        return File(stream, contentType, enableRangeProcessing: true);
    }
}
```

---

## 11. Generated PDF Reports (QuestPDF)

QuestPDF reports are generated **on-demand** and streamed directly to the client.
They are NOT permanently stored on disk (no accumulation of old PDFs).

**Exception:** If the same report is requested multiple times within 60 minutes,
it is cached in `storage\reports\generated\` folder and served from cache.
Cache is auto-purged by a background job (Hosted Service).

---

## 12. Backup Strategy for Files

| Backup Type    | Frequency         | Destination                          |
|----------------|-------------------|--------------------------------------|
| Full backup    | Every Sunday 2 AM | External drive / NAS                 |
| Incremental    | Daily 11 PM       | External drive / NAS                 |
| Offsite backup | Weekly            | Azure Blob / Google Drive (future)   |
| Retention      | 90 days           | Older backups auto-purged            |

Backup script: `scripts\backup-files.ps1` (to be created in Phase 6)

---

## 13. Disk Space Planning

| Category              | Estimated Storage (Year 1) |
|-----------------------|----------------------------|
| Employee documents     | ~5 GB                      |
| Procurement docs       | ~10 GB                     |
| Style/product images   | ~15 GB                     |
| Compliance documents   | ~5 GB                      |
| Generated PDFs (cache) | ~2 GB                      |
| Attendance imports     | ~1 GB                      |
| **Total (Year 1)**     | **~40 GB**                 |
| **Recommended disk**   | **200 GB dedicated drive** |

---

## 14. Phase 2 — Azure Blob Storage Migration

When scaling up, the ONLY change needed is:
1. Add `Azure.Storage.Blobs` NuGet package
2. Implement `AzureBlobFileStorageService : IFileStorageService`
3. Change `"Provider": "AzureBlob"` in `appsettings.json`
4. Run migration script to copy existing files to blob

Zero changes to any business logic, commands, or queries.

---

*Document: FILE_STORAGE.md | Version 1.0.0 | Updated: 2026-09-24*
