using Microsoft.Extensions.DependencyInjection;
using SimplyDIWithScrutor.Writers;

namespace SimplyDIWithScrutor.Services
{
    public class NamedWriterService
    {
        private readonly IServiceProvider _serviceProvider;

        public NamedWriterService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IWriter GetWriter(string name)
        {
            // This approach requires the named services configuration in Program.cs
            // Uncomment the Option 2 in Program.cs to use this service
            return name.ToLower() switch
            {
                "excel" => _serviceProvider.GetRequiredService<ExcelWriter>(),
                "pdf" => _serviceProvider.GetRequiredService<PdfWriter>(),
                _ => throw new ArgumentException($"Unknown writer name: {name}")
            };
        }

        public IEnumerable<string> GetAvailableWriterNames()
        {
            return new[] { "excel", "pdf" };
        }
    }
}
