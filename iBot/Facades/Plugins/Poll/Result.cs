using System.Collections.Generic;

namespace IBot.Facades.Plugins.Poll
{
    public class Result
    {
        public Result(int all, Dictionary<Option, int> votes)
        {
            All = all;
            Votes = votes;
        }

        public int All { get; }
        public Dictionary<Option, int> Votes { get; }
    }
}