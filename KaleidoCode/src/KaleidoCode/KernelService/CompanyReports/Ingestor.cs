
namespace KaleidoCode.KernelService.CompanyReports;

public interface IIngestor
{
    Task ProcessReportsAsync(string allReportsDir, string outputDir, CancellationToken cancellationToken = default);
}