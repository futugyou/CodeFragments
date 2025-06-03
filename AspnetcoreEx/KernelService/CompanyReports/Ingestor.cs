
namespace AspnetcoreEx.KernelService.CompanyReports;

public interface IIngestor
{
    Task ProcessReportsAsync(string allReportsDir, string outputDir);
}