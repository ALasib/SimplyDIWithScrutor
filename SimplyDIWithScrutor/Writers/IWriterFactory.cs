namespace SimplyDIWithScrutor.Writers
{
    public interface IWriterFactory
    {
        IWriter CreateWriter(string format);
        IEnumerable<string> GetAvailableFormats();
    }
}
