namespace AgoraHub360.ERP.Application.Interfaces;

using AgoraHub360.ERP.Shared.DTOs.Contabilidad;

public interface IExportService
{
    byte[] ExportarBalanceGeneralExcel(BalanceGeneralDto dto, string nombreEmpresa);
    byte[] ExportarBalanceGeneralPdf(BalanceGeneralDto dto, string nombreEmpresa);

    byte[] ExportarEstadoResultadosExcel(EstadoResultadosDto dto, string nombreEmpresa);
    byte[] ExportarEstadoResultadosPdf(EstadoResultadosDto dto, string nombreEmpresa);

    byte[] ExportarLibroDiarioExcel(LibroDiarioDto dto, string nombreEmpresa);
    byte[] ExportarLibroDiarioPdf(LibroDiarioDto dto, string nombreEmpresa);

    byte[] ExportarSumasYSaldosExcel(SumasYSaldosDto dto, string nombreEmpresa);

    byte[] ExportarFlujoDEfectivoExcel(FlujoDEfectivoDto dto, string nombreEmpresa);
    byte[] ExportarFlujoDEfectivoPdf(FlujoDEfectivoDto dto, string nombreEmpresa);

    byte[] ExportarLibroMayorExcel(LibroMayorDto dto, string nombreEmpresa);
    byte[] ExportarLibroMayorPdf(LibroMayorDto dto, string nombreEmpresa);
}
