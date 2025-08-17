using Microsoft.AspNetCore.Mvc;
using SimplyDIWithScrutor.Archivers;
using SimplyDIWithScrutor.Services;
using System.Text;

namespace SimplyDIWithScrutor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportGenerator _reportGenerator;
        private readonly IReportArchiver _reportArchiver;

        // The DI container injects all the necessary top-level services.
        // It automatically resolves the entire dependency tree.
        public ReportController(IReportGenerator reportGenerator, IReportArchiver reportArchiver)
        {
            _reportGenerator = reportGenerator;
            _reportArchiver = reportArchiver;
        }

        [HttpGet("generate/{format}")]
        public IActionResult GetWithFormat(string format)
        {
            try
            {
                string reportName = "Monthly_Sales";
                var generationResult = _reportGenerator.GenerateReport(reportName, format);
                var archiveResult = _reportArchiver.Archive(reportName);

                var finalResult = new StringBuilder();
                finalResult.AppendLine(generationResult);
                finalResult.AppendLine(archiveResult);

                return Ok(finalResult.ToString());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("formats")]
        public IActionResult GetAvailableFormats()
        {
            var formats = _reportGenerator.GetAvailableFormats();
            return Ok(new { AvailableFormats = formats });
        }
    }
}
