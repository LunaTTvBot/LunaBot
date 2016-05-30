namespace IBot.Facades.Plugins.Poll.EventArgs
{
    public class PollCreatedEventArgs
    {
        public PollCreatedEventArgs(Poll poll)
        {
            Poll = poll;
        }

        public Poll Poll { get; }
    }
}