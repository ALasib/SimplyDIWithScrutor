namespace SimplyDIWithScrutor.Services
{
    public interface IReportGenerator
    {
        string GenerateReport(string reportName, string format = "excel");
        IEnumerable<string> GetAvailableFormats();
    }
}
