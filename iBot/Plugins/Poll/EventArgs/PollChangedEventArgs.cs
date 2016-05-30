namespace IBot.Plugins.Poll.EventArgs
{
    internal class PollChangedEventArgs
    {
        public Poll Poll { get; }
        public PollState NewState { get; }

        public PollChangedEventArgs(Poll poll, PollState newState)
        {
            Poll = poll;
            NewState = newState;
        }
    }
}