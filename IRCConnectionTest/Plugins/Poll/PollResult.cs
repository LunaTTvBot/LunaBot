using System.Collections.Generic;

namespace IRCConnectionTest.Plugins.Poll
{
    internal class PollResult
    {
        public PollResult(int all, Dictionary<PollOption, int> votesPerOption)
        {
            All = all;
            VotesPerOption = votesPerOption;
        }

        public int All { get; }

        public Dictionary<PollOption, int> VotesPerOption { get; }
    }
}