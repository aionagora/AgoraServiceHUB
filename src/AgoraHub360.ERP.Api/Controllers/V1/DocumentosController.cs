namespace AgoraHub360.ERP.Api.Controllers.V1;

using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.DOC;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs;
using AgoraHub360.ERP.Shared.DTOs.DOC;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/documentos")]
[Authorize]
public class DocumentosController : ControllerBase
{
    private static readonly HashSet<string> _allowedMimes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/jpg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IRepository<Document> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;

    public DocumentosController(
        IRepository<Document> repo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IWebHostEnvironment env)
    {
        _repo        = repo;
        _unitOfWork  = unitOfWork;
        _currentUser = currentUser;
        _env         = env;
    }

    /// <summary>
    /// Sube un archivo y lo registra en doc.Documents.
    /// Devuelve el DocumentId para luego vincularlo a un comprobante.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Unauthorized(ApiResponse<string>.Fail("EmpresaId no encontrado en el token."));

        // Validar tamaño
        if (file.Length > MaxFileSizeBytes)
            return BadRequest(ApiResponse<string>.Fail($"El archivo supera el límite de 10 MB."));

        // Validar tipo MIME
        if (!_allowedMimes.Contains(file.ContentType))
            return BadRequest(ApiResponse<string>.Fail(
                "Tipo de archivo no permitido. Use PDF, JPG, PNG o XLSX."));

        // Guardar en wwwroot/uploads/{empresaId}/
        var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", empresaId.Value.ToString());
        Directory.CreateDirectory(uploadsDir);

        var safeFileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var filePath     = Path.Combine(uploadsDir, safeFileName);

        await using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream, ct);

        var relativeUrl = $"/uploads/{empresaId}/{safeFileName}";

        var doc = new Document
        {
            EmpresaId       = empresaId.Value,
            StorageProvider = 2, // FileSystem
            FileName        = file.FileName,
            MimeType        = file.ContentType,
            SizeBytes       = file.Length,
            Url             = relativeUrl,
            Activo          = true
        };

        await _repo.AddAsync(doc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = new DocumentUploadResultDto
        {
            DocumentId = doc.DocumentId,
            FileName   = doc.FileName,
            MimeType   = doc.MimeType,
            SizeBytes  = doc.SizeBytes,
            Url        = doc.Url
        };

        return Ok(ApiResponse<DocumentUploadResultDto>.Ok(result, "Archivo subido correctamente."));
    }
}
