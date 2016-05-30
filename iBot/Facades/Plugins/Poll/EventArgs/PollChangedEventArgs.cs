namespace IBot.Facades.Plugins.Poll.EventArgs
{
    public class PollChangedEventArgs
    {
        public Poll Poll { get; }
        public State NewState { get; }

        public PollChangedEventArgs(Poll poll, State newState)
        {
            Poll = poll;
            NewState = newState;
        }
    }
}