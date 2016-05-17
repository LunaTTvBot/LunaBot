namespace IBot.Events.CustomEventArgs
{
    public class ErrorEventArgs
    {
        public ErrorEventArgs(string msg)
        {
            Message = msg;
        }

        public string Message { get; }
    }
}
