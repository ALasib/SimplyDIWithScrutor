namespace SimplyDIWithScrutor.Writers
{
    public class PdfWriter : IWriter
    {
        public string Write(string reportName)
        {
            var message = $"Successfully generated PDF report: {reportName}.pdf";
            Console.WriteLine(message);
            return message;
        }
    }
}
