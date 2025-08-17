namespace SimplyDIWithScrutor.Notifiers
{
    public class EmailNotifier : INotifier
    {
        public void Send(string message)
        {
            Console.WriteLine($"EMAIL NOTIFICATION: {message}");
        }
    }
}
