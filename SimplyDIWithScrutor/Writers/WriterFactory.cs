using Microsoft.Extensions.DependencyInjection;

namespace SimplyDIWithScrutor.Writers
{
    public class WriterFactory : IWriterFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WriterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IWriter CreateWriter(string format)
        {
            return format.ToLower() switch
            {
                "excel" => _serviceProvider.GetRequiredService<ExcelWriter>(),
                "pdf" => _serviceProvider.GetRequiredService<PdfWriter>(),
                _ => throw new ArgumentException($"Unsupported format: {format}. Supported formats: {string.Join(", ", GetAvailableFormats())}")
            };
        }

        public IEnumerable<string> GetAvailableFormats()
        {
            return new[] { "excel", "pdf" };
        }
    }
}
