namespace SimplyDIWithScrutor.Writers
{
    public class ExcelWriter : IWriter
    {
        public string Write(string reportName)
        {
            var message = $"Successfully generated Excel report: {reportName}.xlsx";
            Console.WriteLine(message);
            return message;
        }
    }
}
