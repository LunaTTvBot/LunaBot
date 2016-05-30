namespace IBot.Plugins.Poll.EventArgs
{
    internal class PollCreatedEventArgs
    {
        public PollCreatedEventArgs(Poll poll)
        {
            Poll = poll;
        }

        public Poll Poll { get; }
    }
}