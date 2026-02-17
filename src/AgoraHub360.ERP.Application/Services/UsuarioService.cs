namespace AgoraHub360.ERP.Application.Services;

using AgoraHub360.ERP.Application.Common;
using AgoraHub360.ERP.Application.Interfaces;
using AgoraHub360.ERP.Domain.Entities.Core;
using AgoraHub360.ERP.Domain.Interfaces;
using AgoraHub360.ERP.Shared.DTOs.Usuario;

public class UsuarioService : IUsuarioService
{
    private readonly IRepository<Usuario> _repository;
    private readonly IRepository<UsuarioEmpresa> _ueRepository;
    private readonly IRepository<Rol> _rolRepository;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UsuarioService(
        IRepository<Usuario> repository,
        IRepository<UsuarioEmpresa> ueRepository,
        IRepository<Rol> rolRepository,
        IRepository<Empresa> empresaRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _ueRepository = ueRepository;
        _rolRepository = rolRepository;
        _empresaRepository = empresaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<UsuarioDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var usuarios = await _repository.GetAllAsync(ct);
        var empresas = await _empresaRepository.GetAllAsync(ct);
        var empresaMap = empresas.ToDictionary(e => e.Id, e => e.Nombre);
        var dtos = new List<UsuarioDto>();

        foreach (var u in usuarios)
        {
            var ues = await _ueRepository.FindAsync(ue => ue.UsuarioId == u.Id, ct);
            dtos.Add(MapToDto(u, ues, empresaMap));
        }

        return Result<IReadOnlyList<UsuarioDto>>.Success(dtos.AsReadOnly());
    }

    public async Task<Result<UsuarioDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var usuario = await _repository.GetByIdAsync(id, ct);
        if (usuario is null)
            return Result<UsuarioDto>.Failure($"Usuario con Id {id} no encontrado.");

        var ues = await _ueRepository.FindAsync(ue => ue.UsuarioId == id, ct);
        var empresas = await _empresaRepository.GetAllAsync(ct);
        var empresaMap = empresas.ToDictionary(e => e.Id, e => e.Nombre);
        return Result<UsuarioDto>.Success(MapToDto(usuario, ues, empresaMap));
    }

    public async Task<Result<UsuarioDto>> CreateAsync(CreateUsuarioDto dto, CancellationToken ct = default)
    {
        var byUsername = await _repository.FindAsync(u => u.NombreUsuario == dto.NombreUsuario, ct);
        if (byUsername.Count > 0)
            return Result<UsuarioDto>.Failure($"Ya existe un usuario con nombre '{dto.NombreUsuario}'.");

        var byEmail = await _repository.FindAsync(u => u.Email == dto.Email, ct);
        if (byEmail.Count > 0)
            return Result<UsuarioDto>.Failure($"Ya existe un usuario con email '{dto.Email}'.");

        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            NombreCompleto = dto.NombreCompleto,
            Activo = true
        };

        await _repository.AddAsync(usuario, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Asignar a empresa si se especifico
        var ueList = new List<UsuarioEmpresa>();
        if (dto.EmpresaId.HasValue && dto.EmpresaId.Value > 0)
        {
            var empresa = await _empresaRepository.GetByIdAsync(dto.EmpresaId.Value, ct);
            if (empresa is not null)
            {
                var ue = new UsuarioEmpresa
                {
                    UsuarioId = usuario.Id,
                    EmpresaId = dto.EmpresaId.Value,
                    Rol = string.IsNullOrEmpty(dto.Rol) ? "Viewer" : dto.Rol
                };
                await _ueRepository.AddAsync(ue, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                ueList.Add(ue);
            }
        }

        var empresas = await _empresaRepository.GetAllAsync(ct);
        var empresaMap = empresas.ToDictionary(e => e.Id, e => e.Nombre);
        return Result<UsuarioDto>.Success(MapToDto(usuario, ueList, empresaMap));
    }

    public async Task<Result<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto, CancellationToken ct = default)
    {
        var usuario = await _repository.GetByIdAsync(id, ct);
        if (usuario is null)
            return Result<UsuarioDto>.Failure($"Usuario con Id {id} no encontrado.");

        var byUsername = await _repository.FindAsync(u => u.NombreUsuario == dto.NombreUsuario && u.Id != id, ct);
        if (byUsername.Count > 0)
            return Result<UsuarioDto>.Failure($"Ya existe otro usuario con nombre '{dto.NombreUsuario}'.");

        var byEmail = await _repository.FindAsync(u => u.Email == dto.Email && u.Id != id, ct);
        if (byEmail.Count > 0)
            return Result<UsuarioDto>.Failure($"Ya existe otro usuario con email '{dto.Email}'.");

        usuario.NombreUsuario = dto.NombreUsuario;
        usuario.Email = dto.Email;
        usuario.NombreCompleto = dto.NombreCompleto;
        usuario.Activo = dto.Activo;

        await _repository.UpdateAsync(usuario, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var ues = await _ueRepository.FindAsync(ue => ue.UsuarioId == id, ct);
        var allEmpresas = await _empresaRepository.GetAllAsync(ct);
        var empresaMap = allEmpresas.ToDictionary(e => e.Id, e => e.Nombre);
        return Result<UsuarioDto>.Success(MapToDto(usuario, ues, empresaMap));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var usuario = await _repository.GetByIdAsync(id, ct);
        if (usuario is null)
            return Result<bool>.Failure($"Usuario con Id {id} no encontrado.");

        await _repository.DeleteAsync(usuario, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> AsignarRolAsync(int usuarioId, AsignarRolDto dto, CancellationToken ct = default)
    {
        var usuario = await _repository.GetByIdAsync(usuarioId, ct);
        if (usuario is null)
            return Result<bool>.Failure($"Usuario con Id {usuarioId} no encontrado.");

        var empresa = await _empresaRepository.GetByIdAsync(dto.EmpresaId, ct);
        if (empresa is null)
            return Result<bool>.Failure($"Empresa con Id {dto.EmpresaId} no encontrada.");

        // Validar que el rol existe en el catalogo
        var roles = await _rolRepository.FindAsync(r => r.Nombre == dto.Rol && r.Activo, ct);
        if (roles.Count == 0)
            return Result<bool>.Failure($"Rol '{dto.Rol}' no existe o esta inactivo.");

        var existing = await _ueRepository.FindAsync(
            ue => ue.UsuarioId == usuarioId && ue.EmpresaId == dto.EmpresaId, ct);

        if (existing.Count > 0)
        {
            // Actualizar rol existente
            var ue = existing[0];
            ue.Rol = dto.Rol;
            await _ueRepository.UpdateAsync(ue, ct);
        }
        else
        {
            // Nueva asignacion
            var ue = new UsuarioEmpresa
            {
                UsuarioId = usuarioId,
                EmpresaId = dto.EmpresaId,
                Rol = dto.Rol
            };
            await _ueRepository.AddAsync(ue, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RemoverDeEmpresaAsync(int usuarioId, int empresaId, CancellationToken ct = default)
    {
        var existing = await _ueRepository.FindAsync(
            ue => ue.UsuarioId == usuarioId && ue.EmpresaId == empresaId, ct);

        if (existing.Count == 0)
            return Result<bool>.Failure("El usuario no esta asignado a esa empresa.");

        await _ueRepository.DeleteAsync(existing[0], ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private static UsuarioDto MapToDto(Usuario u, IEnumerable<UsuarioEmpresa> ues, Dictionary<int, string> empresaMap) => new()
    {
        Id = u.Id,
        NombreUsuario = u.NombreUsuario,
        Email = u.Email,
        NombreCompleto = u.NombreCompleto,
        Activo = u.Activo,
        EmpresaActivaId = u.EmpresaActivaId,
        FechaCreacion = u.FechaCreacion,
        EmpresasAsignadas = ues.Select(ue => new UsuarioEmpresaRolDto
        {
            EmpresaId = ue.EmpresaId,
            EmpresaNombre = empresaMap.TryGetValue(ue.EmpresaId, out var nombre) ? nombre : $"Empresa #{ue.EmpresaId}",
            Rol = ue.Rol
        }).ToList()
    };

    // Hash simple para desarrollo; reemplazar con BCrypt/Argon2 en produccion
    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
