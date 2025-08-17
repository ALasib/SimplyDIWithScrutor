using SimplyDIWithScrutor.Notifiers;
using SimplyDIWithScrutor.Writers;

namespace SimplyDIWithScrutor.Services
{
    public class ReportGenerator : IReportGenerator
    {
        private readonly IWriterFactory _writerFactory;
        private readonly IEnumerable<INotifier> _notifiers;

        public ReportGenerator(IWriterFactory writerFactory, IEnumerable<INotifier> notifiers)
        {
            _writerFactory = writerFactory;
            _notifiers = notifiers;
        }

        public string GenerateReport(string reportName, string format = "excel")
        {
            var writer = _writerFactory.CreateWriter(format);
            var result = writer.Write(reportName);

            // After generating, loop through all available notifiers and send.
            foreach (var notifier in _notifiers)
            {
                notifier.Send($"Report '{reportName}' was successfully generated in {format} format.");
            }

            return result;
        }

        public IEnumerable<string> GetAvailableFormats()
        {
            return _writerFactory.GetAvailableFormats();
        }
    }
}
