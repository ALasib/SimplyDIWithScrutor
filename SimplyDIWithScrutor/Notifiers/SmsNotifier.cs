namespace SimplyDIWithScrutor.Notifiers
{
    public class SmsNotifier : INotifier
    {
        public void Send(string message)
        {
            Console.WriteLine($"SMS NOTIFICATION: {message}");
        }
    }
}
