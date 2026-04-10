namespace AgoraHub360.ERP.Application.Validators.Workflow;

using AgoraHub360.ERP.Shared.DTOs.Workflow;
using FluentValidation;

// ?????????????????????????????????????????????????????????????????????????????
// VALIDADORES FLUENT VALIDATION — Módulo Workflow
// ?????????????????????????????????????????????????????????????????????????????

/// <summary>Tipos de documento reconocidos por el motor de workflow.</summary>
file static class EntityTypes
{
    internal static readonly string[] Validos =
        ["OrdenPedido", "OrdenCompra", "OrdenVenta", "HojaRuta", "Embarque", "Recepcion"];
}

// ?? Validador 1: TareaCreateValidator ????????????????????????????????????????

/// <summary>
/// Valida el DTO de creación manual de una tarea.
/// Garantiza que EntityType sea reconocido y que Codigo use solo
/// mayúsculas, números y guiones (evita valores free-text incoherentes).
/// </summary>
public sealed class TareaCreateValidator : AbstractValidator<TareaCreateDto>
{
    public TareaCreateValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .MaximumLength(50)
            .Must(EntityTypes.Validos.Contains)
            .WithMessage("EntityType no reconocido en el sistema");

        RuleFor(x => x.EntityId)
            .GreaterThan(0);

        RuleFor(x => x.Orden)
            .GreaterThan(0);

        RuleFor(x => x.Codigo)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[A-Z0-9\-]+$")
            .WithMessage("Código solo puede contener mayúsculas, números y guiones");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.FechaPlan)
            .Must(f => !f.HasValue || f.Value >= DateOnly.FromDateTime(DateTime.Today.AddYears(-1)))
            .WithMessage("FechaPlan no puede ser anterior a hace un año");

        When(x => x.Observaciones is not null, () =>
            RuleFor(x => x.Observaciones)
                .MaximumLength(500));
    }
}

// ?? Validador 2: CompletarTareaValidator ?????????????????????????????????????

/// <summary>
/// Valida el DTO para completar un hito.
/// Impide registrar una FechaReal futura (no se puede confirmar algo que aún no ocurrió).
/// </summary>
public sealed class CompletarTareaValidator : AbstractValidator<CompletarTareaDto>
{
    public CompletarTareaValidator()
    {
        RuleFor(x => x.TareaId)
            .GreaterThan(0);

        RuleFor(x => x.FechaReal)
            .Must(f => !f.HasValue || f.Value <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("La fecha real no puede ser futura");
    }
}

// ?? Validador 3: GenerarHitosValidator ???????????????????????????????????????

/// <summary>
/// Valida el DTO para instanciar las tareas de un documento
/// desde su plantilla de workflow correspondiente.
/// </summary>
public sealed class GenerarHitosValidator : AbstractValidator<GenerarHitosDto>
{
    public GenerarHitosValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.EntityId)
            .GreaterThan(0);

        RuleFor(x => x.EmpresaId)
            .GreaterThan(0);

        When(x => x.SubTipo is not null, () =>
            RuleFor(x => x.SubTipo)
                .MaximumLength(50));
    }
}
