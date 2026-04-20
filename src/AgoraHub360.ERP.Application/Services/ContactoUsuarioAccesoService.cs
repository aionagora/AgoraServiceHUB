namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Entities.MDM;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.MDM;

public class ContactoUsuarioAccesoService : IContactoUsuarioAccesoService
{
    private readonly IRepository<ContactoUsuarioAcceso> _repository;
    private readonly IRepository<Contacto> _contactoRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<PerfilAcceso> _perfilRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ContactoUsuarioAccesoService(
        IRepository<ContactoUsuarioAcceso> repository,
        IRepository<Contacto> contactoRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<PerfilAcceso> perfilRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _contactoRepository = contactoRepository;
        _usuarioRepository = usuarioRepository;
        _perfilRepository = perfilRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ContactoUsuarioAccesoDto>> GetByContactoAsync(int contactoId, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ContactoUsuarioAccesoDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var acceso = (await _repository.FindAsync(x => x.EmpresaId == empresaId && x.ContactoId == contactoId, ct)).FirstOrDefault();

        if (acceso is null)
            return Result<ContactoUsuarioAccesoDto>.Failure("El contacto no tiene acceso de usuario configurado.");

        return Result<ContactoUsuarioAccesoDto>.Success(MapToDto(acceso));
    }

    public async Task<Result<ContactoUsuarioAccesoDto>> UpsertAsync(int contactoId, UpsertContactoUsuarioAccesoDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<ContactoUsuarioAccesoDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;

        var contacto = await _contactoRepository.GetByIdAsync(contactoId, ct);
        if (contacto is null || contacto.EmpresaId != empresaId)
            return Result<ContactoUsuarioAccesoDto>.Failure("Contacto no encontrado para la empresa activa.");

        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId, ct);
        if (usuario is null || !usuario.Activo)
            return Result<ContactoUsuarioAccesoDto>.Failure("Usuario no encontrado o inactivo.");

        if (dto.PerfilAccesoId.HasValue)
        {
            var perfil = await _perfilRepository.GetByIdAsync(dto.PerfilAccesoId.Value, ct);
            if (perfil is null || perfil.EmpresaId != empresaId || !perfil.Activo)
                return Result<ContactoUsuarioAccesoDto>.Failure("Perfil de acceso no válido para la empresa activa.");
        }

        if (dto.Activo)
        {
            if (!dto.AccesoWeb && !dto.AccesoMovil)
                return Result<ContactoUsuarioAccesoDto>.Failure("Debe habilitar acceso web y/o móvil para activar el acceso.");

            if (!dto.PerfilAccesoId.HasValue)
                return Result<ContactoUsuarioAccesoDto>.Failure("Debe asignar un perfil para activar el acceso.");
        }

        var existingByContacto = (await _repository.FindAsync(x => x.EmpresaId == empresaId && x.ContactoId == contactoId, ct)).FirstOrDefault();
        var existingByUsuario = (await _repository.FindAsync(x => x.EmpresaId == empresaId && x.UsuarioId == dto.UsuarioId && x.ContactoId != contactoId, ct)).FirstOrDefault();
        if (existingByUsuario is not null)
            return Result<ContactoUsuarioAccesoDto>.Failure("El usuario ya está asociado a otro contacto en la empresa activa.");

        if (existingByContacto is null)
        {
            existingByContacto = new ContactoUsuarioAcceso
            {
                EmpresaId = empresaId,
                ContactoId = contactoId,
                UsuarioId = dto.UsuarioId,
                PerfilAccesoId = dto.PerfilAccesoId,
                AccesoWeb = dto.AccesoWeb,
                AccesoMovil = dto.AccesoMovil,
                Activo = dto.Activo
            };

            await _repository.AddAsync(existingByContacto, ct);
        }
        else
        {
            existingByContacto.UsuarioId = dto.UsuarioId;
            existingByContacto.PerfilAccesoId = dto.PerfilAccesoId;
            existingByContacto.AccesoWeb = dto.AccesoWeb;
            existingByContacto.AccesoMovil = dto.AccesoMovil;
            existingByContacto.Activo = dto.Activo;
            await _repository.UpdateAsync(existingByContacto, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ContactoUsuarioAccesoDto>.Success(MapToDto(existingByContacto));
    }

    public async Task<Result<bool>> SetActivoAsync(int contactoId, bool activo, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var acceso = (await _repository.FindAsync(x => x.EmpresaId == empresaId && x.ContactoId == contactoId, ct)).FirstOrDefault();
        if (acceso is null)
            return Result<bool>.Failure("El contacto no tiene acceso configurado.");

        if (activo && !acceso.AccesoWeb && !acceso.AccesoMovil)
            return Result<bool>.Failure("No se puede activar un acceso sin canal web ni móvil.");

        if (activo && !acceso.PerfilAccesoId.HasValue)
            return Result<bool>.Failure("No se puede activar un acceso sin perfil asignado.");

        acceso.Activo = activo;
        await _repository.UpdateAsync(acceso, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private Result<int> TryGetEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<int>.Failure("No se pudo determinar la empresa activa.");

        return Result<int>.Success(_currentUser.EmpresaId.Value);
    }

    private static ContactoUsuarioAccesoDto MapToDto(ContactoUsuarioAcceso e) => new()
    {
        Id = e.Id,
        ContactoId = e.ContactoId,
        EmpresaId = e.EmpresaId,
        UsuarioId = e.UsuarioId,
        PerfilAccesoId = e.PerfilAccesoId,
        AccesoWeb = e.AccesoWeb,
        AccesoMovil = e.AccesoMovil,
        Activo = e.Activo,
        FechaCreacion = e.FechaCreacion
    };
}
