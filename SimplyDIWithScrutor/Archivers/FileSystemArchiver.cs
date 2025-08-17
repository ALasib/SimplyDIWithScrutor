namespace SimplyDIWithScrutor.Archivers
{
    public class FileSystemArchiver : IReportArchiver
    {
        public string Archive(string reportName)
        {
            string archiveMessage = $"Archived '{reportName}' to the local file system.";
            Console.WriteLine(archiveMessage);
            return archiveMessage;
        }
    }
}
