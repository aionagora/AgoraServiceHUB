using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

namespace AgoraHub360.ERP.Application.Interfaces;

public interface IAsientoExportService
{
    Task<byte[]> ExportarExcelAsync(IEnumerable<AsientoContableDto> asientos, CancellationToken ct = default);
    Task<string> GenerarCsvAsync(IEnumerable<AsientoContableDto> asientos);
    Task<string> GenerarJsonAsync(IEnumerable<AsientoContableDto> asientos);
    Task<string> GenerarXmlAsync(IEnumerable<AsientoContableDto> asientos);
    byte[] ExportarExcel(IReadOnlyList<AsientoContableDto> asientos);
    byte[] ExportarExcelIndividual(AsientoContableDto asiento);
    byte[] ExportarExcelPlano(IReadOnlyList<AsientoContableDto> asientos);
}
