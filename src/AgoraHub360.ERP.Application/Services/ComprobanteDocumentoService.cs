namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.ACC;
using AgoraHub360.ERP.Domain.Entities.DOC;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public class ComprobanteDocumentoService : IComprobanteDocumentoService
{
    private readonly IRepository<ComprobanteDocumento> _repo;
    private readonly IRepository<AsientoContable> _asientoRepo;
    private readonly IRepository<Document> _documentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ComprobanteDocumentoService(
        IRepository<ComprobanteDocumento> repo,
        IRepository<AsientoContable> asientoRepo,
        IRepository<Document> documentRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repo          = repo;
        _asientoRepo   = asientoRepo;
        _documentRepo  = documentRepo;
        _unitOfWork    = unitOfWork;
        _currentUser   = currentUser;
    }

    // ?? GetByComprobante ??????????????????????????????????????????????????????

    public async Task<Result<IReadOnlyList<ComprobanteDocumentoDto>>> GetByComprobanteAsync(
        long comprobanteId, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<IReadOnlyList<ComprobanteDocumentoDto>>.Failure("No active company.");

        // Verificar que el comprobante pertenece a la empresa
        var asiento = await _asientoRepo.GetByIdAsync(comprobanteId, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value)
            return Result<IReadOnlyList<ComprobanteDocumentoDto>>.Failure("Comprobante no encontrado.");

        var adjuntos = await _repo.FindAsync(
            cd => cd.ComprobanteId == comprobanteId && cd.EmpresaId == empresaId.Value, ct);

        // Enriquecer con datos del documento
        var docIds  = adjuntos.Select(a => a.DocumentId).Distinct().ToList();
        var documentos = await _documentRepo.FindAsync(
            d => docIds.Contains(d.DocumentId), ct);
        var docMap = documentos.ToDictionary(d => d.DocumentId);

        var dtos = adjuntos
            .Select(cd => MapToDto(cd, docMap.GetValueOrDefault(cd.DocumentId)))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ComprobanteDocumentoDto>>.Success(dtos);
    }

    // ?? Adjuntar ??????????????????????????????????????????????????????????????

    public async Task<Result<ComprobanteDocumentoDto>> AdjuntarAsync(
        long comprobanteId, AdjuntarDocumentoDto dto, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<ComprobanteDocumentoDto>.Failure("No active company.");

        // Validar comprobante
        var asiento = await _asientoRepo.GetByIdAsync(comprobanteId, ct);
        if (asiento is null || asiento.EmpresaId != empresaId.Value || !asiento.Activo)
            return Result<ComprobanteDocumentoDto>.Failure("Comprobante no encontrado.");

        // Validar documento
        var documento = await _documentRepo.GetByIdAsync(dto.DocumentId, ct);
        if (documento is null || documento.EmpresaId != empresaId.Value)
            return Result<ComprobanteDocumentoDto>.Failure("Documento no encontrado o no pertenece a la empresa.");

        // Evitar duplicados
        var yaExiste = await _repo.FindAsync(
            cd => cd.ComprobanteId == comprobanteId
               && cd.DocumentId == dto.DocumentId
               && cd.EmpresaId  == empresaId.Value, ct);
        if (yaExiste.Count > 0)
            return Result<ComprobanteDocumentoDto>.Failure("El documento ya está adjunto a este comprobante.");

        var adjunto = new ComprobanteDocumento
        {
            ComprobanteId = comprobanteId,
            DocumentId    = dto.DocumentId,
            Descripcion   = dto.Descripcion?.Trim(),
            EmpresaId     = empresaId.Value,
            Activo        = true
        };

        await _repo.AddAsync(adjunto, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ComprobanteDocumentoDto>.Success(MapToDto(adjunto, documento));
    }

    // ?? Remover ???????????????????????????????????????????????????????????????

    public async Task<Result<bool>> RemoverAsync(long comprobanteId, int docId, CancellationToken ct)
    {
        var empresaId = _currentUser.EmpresaId;
        if (!empresaId.HasValue)
            return Result<bool>.Failure("No active company.");

        var adjuntos = await _repo.FindAsync(
            cd => cd.Id == docId
               && cd.ComprobanteId == comprobanteId
               && cd.EmpresaId     == empresaId.Value, ct);

        var adjunto = adjuntos.FirstOrDefault();
        if (adjunto is null)
            return Result<bool>.Failure("Adjunto no encontrado.");

        await _repo.DeleteAsync(adjunto, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    // ?? Helper ????????????????????????????????????????????????????????????????

    private static ComprobanteDocumentoDto MapToDto(ComprobanteDocumento cd, Document? doc) => new()
    {
        Id             = cd.Id,
        ComprobanteId  = cd.ComprobanteId,
        DocumentId     = cd.DocumentId,
        Descripcion    = cd.Descripcion,
        EmpresaId      = cd.EmpresaId,
        FileName       = doc?.FileName  ?? string.Empty,
        MimeType       = doc?.MimeType  ?? string.Empty,
        Url            = doc?.Url,
        SizeBytes      = doc?.SizeBytes ?? 0
    };
}
