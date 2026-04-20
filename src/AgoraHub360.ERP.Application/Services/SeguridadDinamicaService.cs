namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Seguridad;

public class SeguridadDinamicaService : ISeguridadDinamicaService
{
    private readonly IRepository<ModuloSistema> _moduloRepository;
    private readonly IRepository<FormularioSistema> _formularioRepository;
    private readonly IRepository<AccionSistema> _accionRepository;
    private readonly IRepository<PerfilAcceso> _perfilRepository;
    private readonly IRepository<PerfilPermiso> _permisoRepository;
    private readonly IRepository<UsuarioPerfil> _usuarioPerfilRepository;
    private readonly IRepository<UsuarioSucursalAcceso> _usuarioSucursalRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SeguridadDinamicaService(
        IRepository<ModuloSistema> moduloRepository,
        IRepository<FormularioSistema> formularioRepository,
        IRepository<AccionSistema> accionRepository,
        IRepository<PerfilAcceso> perfilRepository,
        IRepository<PerfilPermiso> permisoRepository,
        IRepository<UsuarioPerfil> usuarioPerfilRepository,
        IRepository<UsuarioSucursalAcceso> usuarioSucursalRepository,
        IRepository<Usuario> usuarioRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _moduloRepository = moduloRepository;
        _formularioRepository = formularioRepository;
        _accionRepository = accionRepository;
        _perfilRepository = perfilRepository;
        _permisoRepository = permisoRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _usuarioSucursalRepository = usuarioSucursalRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<SesionContextoDto>> GetSesionContextAsync(int? usuarioId = null, int? empresaId = null, CancellationToken ct = default)
    {
        var contextResult = ResolveUserAndEmpresa(usuarioId, empresaId);
        if (!contextResult.IsSuccess)
            return Result<SesionContextoDto>.Failure(contextResult.Error!);

        var userId = contextResult.Value.UsuarioId;
        var resolvedEmpresaId = contextResult.Value.EmpresaId;

        var perfilesActivos = await GetPerfilesUsuarioActivosAsync(userId, resolvedEmpresaId, ct);
        var perfiles = await BuildPerfilesSesionAsync(perfilesActivos, ct);

        var menuResult = await GetMenuUsuarioAsync(userId, resolvedEmpresaId, ct);
        if (!menuResult.IsSuccess)
            return Result<SesionContextoDto>.Failure(menuResult.Error!);

        var sucursales = (await _usuarioSucursalRepository.FindAsync(x =>
                x.UsuarioId == userId &&
                x.EmpresaId == resolvedEmpresaId &&
                x.Activo,
            ct))
            .OrderByDescending(x => x.EsPredeterminada)
            .ThenBy(x => x.SucursalId)
            .Select(x => new UsuarioSucursalAccesoDto
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                EmpresaId = x.EmpresaId,
                SucursalId = x.SucursalId,
                EsPredeterminada = x.EsPredeterminada,
                PuedeConsultar = x.PuedeConsultar,
                PuedeOperar = x.PuedeOperar,
                Activo = x.Activo
            })
            .ToList();

        var predeterminadaId = sucursales.FirstOrDefault(x => x.EsPredeterminada)?.SucursalId
                             ?? sucursales.FirstOrDefault()?.SucursalId;

        return Result<SesionContextoDto>.Success(new SesionContextoDto
        {
            UsuarioId = userId,
            EmpresaId = resolvedEmpresaId,
            Perfiles = perfiles,
            ModulosPermitidos = menuResult.Value!.ToList(),
            SucursalesPermitidas = sucursales,
            SucursalPredeterminadaId = predeterminadaId
        });
    }

    public async Task<Result<IReadOnlyList<ModuloSistemaDto>>> GetMenuUsuarioAsync(int? usuarioId = null, int? empresaId = null, CancellationToken ct = default)
    {
        var contextResult = ResolveUserAndEmpresa(usuarioId, empresaId);
        if (!contextResult.IsSuccess)
            return Result<IReadOnlyList<ModuloSistemaDto>>.Failure(contextResult.Error!);

        var perfilesActivos = await GetPerfilesUsuarioActivosAsync(contextResult.Value.UsuarioId, contextResult.Value.EmpresaId, ct);
        if (perfilesActivos.Count == 0)
            return Result<IReadOnlyList<ModuloSistemaDto>>.Success(Array.Empty<ModuloSistemaDto>());

        var perfilIds = perfilesActivos.Select(x => x.PerfilAccesoId).Distinct().ToList();
        var permisos = await _permisoRepository.FindAsync(x => perfilIds.Contains(x.PerfilAccesoId) && x.Permitido && x.Activo, ct);
        var formularioIdsPermitidos = permisos.Select(x => x.FormularioSistemaId).Distinct().ToList();

        if (formularioIdsPermitidos.Count == 0)
            return Result<IReadOnlyList<ModuloSistemaDto>>.Success(Array.Empty<ModuloSistemaDto>());

        var formularios = (await _formularioRepository.FindAsync(x => formularioIdsPermitidos.Contains(x.Id) && x.Activo && x.VisibleEnMenu, ct))
            .OrderBy(x => x.Orden)
            .ToList();

        var moduloIds = formularios.Select(x => x.ModuloSistemaId).Distinct().ToList();
        var modulos = (await _moduloRepository.FindAsync(x => moduloIds.Contains(x.Id) && x.Activo, ct))
            .OrderBy(x => x.Orden)
            .ToList();

        var acciones = await _accionRepository.FindAsync(x => formularioIdsPermitidos.Contains(x.FormularioSistemaId) && x.Activo, ct);

        var menu = modulos.Select(m => new ModuloSistemaDto
        {
            Id = m.Id,
            Codigo = m.Codigo,
            Nombre = m.Nombre,
            Icono = m.Icono,
            Orden = m.Orden,
            Formularios = formularios
                .Where(f => f.ModuloSistemaId == m.Id)
                .Select(f => new FormularioSistemaDto
                {
                    Id = f.Id,
                    ModuloSistemaId = f.ModuloSistemaId,
                    Codigo = f.Codigo,
                    Nombre = f.Nombre,
                    Ruta = f.Ruta,
                    Icono = f.Icono,
                    Orden = f.Orden,
                    VisibleEnMenu = f.VisibleEnMenu,
                    Acciones = acciones
                        .Where(a => a.FormularioSistemaId == f.Id)
                        .OrderBy(a => a.Orden)
                        .Select(a => new AccionSistemaDto
                        {
                            Id = a.Id,
                            Codigo = a.Codigo,
                            Nombre = a.Nombre,
                            Orden = a.Orden
                        })
                        .ToList()
                })
                .OrderBy(f => f.Orden)
                .ToList()
        }).ToList().AsReadOnly();

        return Result<IReadOnlyList<ModuloSistemaDto>>.Success(menu);
    }

    public async Task<Result<bool>> HasPermissionAsync(string formularioCodigo, string? accionCodigo = null, int? usuarioId = null, int? empresaId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(formularioCodigo))
            return Result<bool>.Failure("El código de formulario es obligatorio.");

        var contextResult = ResolveUserAndEmpresa(usuarioId, empresaId);
        if (!contextResult.IsSuccess)
            return Result<bool>.Failure(contextResult.Error!);

        var formulario = (await _formularioRepository.FindAsync(x => x.Codigo == formularioCodigo && x.Activo, ct)).FirstOrDefault();
        if (formulario is null)
            return Result<bool>.Success(false);

        int? accionId = null;
        if (!string.IsNullOrWhiteSpace(accionCodigo))
        {
            var accion = (await _accionRepository.FindAsync(x => x.FormularioSistemaId == formulario.Id && x.Codigo == accionCodigo && x.Activo, ct)).FirstOrDefault();
            if (accion is null)
                return Result<bool>.Success(false);

            accionId = accion.Id;
        }

        var perfilesActivos = await GetPerfilesUsuarioActivosAsync(contextResult.Value.UsuarioId, contextResult.Value.EmpresaId, ct);
        var perfilIds = perfilesActivos.Select(x => x.PerfilAccesoId).Distinct().ToList();

        if (perfilIds.Count == 0)
            return Result<bool>.Success(false);

        var permisos = await _permisoRepository.FindAsync(x =>
            perfilIds.Contains(x.PerfilAccesoId) &&
            x.FormularioSistemaId == formulario.Id &&
            x.Permitido &&
            x.Activo, ct);

        var allowed = accionId.HasValue
            ? permisos.Any(x => x.AccionSistemaId == accionId || x.AccionSistemaId == null)
            : permisos.Any();

        return Result<bool>.Success(allowed);
    }

    public async Task<Result<PerfilAccesoDto>> CreatePerfilAsync(UpsertPerfilAccesoDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<PerfilAccesoDto>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var existing = await _perfilRepository.FindAsync(x => x.EmpresaId == empresaId && x.Codigo == dto.Codigo, ct);
        if (existing.Any())
            return Result<PerfilAccesoDto>.Failure($"Ya existe un perfil con código '{dto.Codigo}'.");

        var perfil = new PerfilAcceso
        {
            EmpresaId = empresaId,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            TipoUsuario = dto.TipoUsuario,
            Activo = dto.Activo
        };

        await _perfilRepository.AddAsync(perfil, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<PerfilAccesoDto>.Success(new PerfilAccesoDto
        {
            Id = perfil.Id,
            EmpresaId = perfil.EmpresaId,
            Codigo = perfil.Codigo,
            Nombre = perfil.Nombre,
            Descripcion = perfil.Descripcion,
            TipoUsuario = perfil.TipoUsuario,
            Activo = perfil.Activo
        });
    }

    public async Task<Result<bool>> SetPermisoPerfilAsync(UpsertPerfilPermisoDto dto, CancellationToken ct = default)
    {
        var perfil = await _perfilRepository.GetByIdAsync(dto.PerfilAccesoId, ct);
        if (perfil is null)
            return Result<bool>.Failure("Perfil de acceso no encontrado.");

        var formulario = await _formularioRepository.GetByIdAsync(dto.FormularioSistemaId, ct);
        if (formulario is null)
            return Result<bool>.Failure("Formulario no encontrado.");

        if (dto.AccionSistemaId.HasValue)
        {
            var accion = await _accionRepository.GetByIdAsync(dto.AccionSistemaId.Value, ct);
            if (accion is null || accion.FormularioSistemaId != formulario.Id)
                return Result<bool>.Failure("La acción no pertenece al formulario indicado.");
        }

        var existentes = await _permisoRepository.FindAsync(x =>
            x.PerfilAccesoId == dto.PerfilAccesoId &&
            x.FormularioSistemaId == dto.FormularioSistemaId &&
            x.AccionSistemaId == dto.AccionSistemaId, ct);

        var permiso = existentes.FirstOrDefault();
        if (permiso is null)
        {
            permiso = new PerfilPermiso
            {
                EmpresaId = perfil.EmpresaId,
                PerfilAccesoId = dto.PerfilAccesoId,
                FormularioSistemaId = dto.FormularioSistemaId,
                AccionSistemaId = dto.AccionSistemaId,
                Permitido = dto.Permitido,
                Activo = true
            };

            await _permisoRepository.AddAsync(permiso, ct);
        }
        else
        {
            permiso.Permitido = dto.Permitido;
            permiso.Activo = true;
            await _permisoRepository.UpdateAsync(permiso, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> AsignarPerfilUsuarioAsync(AsignarUsuarioPerfilDto dto, CancellationToken ct = default)
    {
        var empresaIdResult = TryGetEmpresaId();
        if (!empresaIdResult.IsSuccess)
            return Result<bool>.Failure(empresaIdResult.Error!);

        var empresaId = empresaIdResult.Value;
        var usuario = await _usuarioRepository.GetByIdAsync(dto.UsuarioId, ct);
        if (usuario is null)
            return Result<bool>.Failure("Usuario no encontrado.");

        var perfil = await _perfilRepository.GetByIdAsync(dto.PerfilAccesoId, ct);
        if (perfil is null || perfil.EmpresaId != empresaId)
            return Result<bool>.Failure("Perfil no encontrado para la empresa activa.");

        var existing = (await _usuarioPerfilRepository.FindAsync(x =>
            x.UsuarioId == dto.UsuarioId &&
            x.PerfilAccesoId == dto.PerfilAccesoId &&
            x.EmpresaId == empresaId, ct)).FirstOrDefault();

        if (existing is null)
        {
            var asignacion = new UsuarioPerfil
            {
                EmpresaId = empresaId,
                UsuarioId = dto.UsuarioId,
                PerfilAccesoId = dto.PerfilAccesoId,
                VigenteDesde = dto.VigenteDesde,
                VigenteHasta = dto.VigenteHasta,
                Activo = dto.Activo
            };

            await _usuarioPerfilRepository.AddAsync(asignacion, ct);
        }
        else
        {
            existing.VigenteDesde = dto.VigenteDesde;
            existing.VigenteHasta = dto.VigenteHasta;
            existing.Activo = dto.Activo;
            await _usuarioPerfilRepository.UpdateAsync(existing, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private Result<int> TryGetEmpresaId()
    {
        if (!_currentUser.EmpresaId.HasValue)
            return Result<int>.Failure("No se pudo determinar la empresa activa.");

        return Result<int>.Success(_currentUser.EmpresaId.Value);
    }

    private Result<(int UsuarioId, int EmpresaId)> ResolveUserAndEmpresa(int? usuarioId, int? empresaId)
    {
        var resolvedUsuario = usuarioId ?? _currentUser.UserIdInt;
        if (!resolvedUsuario.HasValue)
            return Result<(int UsuarioId, int EmpresaId)>.Failure("No se pudo determinar el usuario.");

        var resolvedEmpresa = empresaId ?? _currentUser.EmpresaId;
        if (!resolvedEmpresa.HasValue)
            return Result<(int UsuarioId, int EmpresaId)>.Failure("No se pudo determinar la empresa.");

        return Result<(int UsuarioId, int EmpresaId)>.Success((resolvedUsuario.Value, resolvedEmpresa.Value));
    }

    private async Task<IReadOnlyList<UsuarioPerfil>> GetPerfilesUsuarioActivosAsync(int usuarioId, int empresaId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return await _usuarioPerfilRepository.FindAsync(x =>
            x.UsuarioId == usuarioId &&
            x.EmpresaId == empresaId &&
            x.Activo &&
            (!x.VigenteDesde.HasValue || x.VigenteDesde <= now) &&
            (!x.VigenteHasta.HasValue || x.VigenteHasta >= now), ct);
    }

    private async Task<List<PerfilUsuarioSesionDto>> BuildPerfilesSesionAsync(IReadOnlyList<UsuarioPerfil> perfilesActivos, CancellationToken ct)
    {
        var result = new List<PerfilUsuarioSesionDto>();
        var ids = perfilesActivos.Select(x => x.PerfilAccesoId).Distinct().ToList();
        if (ids.Count == 0)
            return result;

        var perfiles = await _perfilRepository.FindAsync(x => ids.Contains(x.Id) && x.Activo, ct);
        foreach (var perfil in perfiles.OrderBy(x => x.Nombre))
        {
            result.Add(new PerfilUsuarioSesionDto
            {
                PerfilAccesoId = perfil.Id,
                Codigo = perfil.Codigo,
                Nombre = perfil.Nombre,
                TipoUsuario = perfil.TipoUsuario
            });
        }

        return result;
    }
}
