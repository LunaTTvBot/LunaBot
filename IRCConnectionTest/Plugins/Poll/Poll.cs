using System.Collections.Generic;

namespace IRCConnectionTest.Plugins.Poll
{
    internal class Poll
    {
        public Poll(string title, List<PollOption> options, int id)
        {
            Id = id;
            Title = title;
            Options = options;
            State = PollState.Created;
            Votes = new Dictionary<PollOption, List<string>>();
            Options.ForEach(o => Votes.Add(o, new List<string>()));
        }

        public string Title { get; }

        public int Id { get; }

        public List<PollOption> Options { get; }

        private PollState State { get; set; }

        private Dictionary<PollOption, List<string>> Votes { get; set; }

        public PollState GetPollState()
        {
            return State;
        }

        public void AbortPoll()
        {
            State = PollState.Aborted;
        }

        public void StopPoll()
        {
            State = PollState.Finished;
        }

        public void StartPoll()
        {
            State = PollState.Started;
        }

        public PollResult GetPollResults()
        {
            if (State == PollState.Created || State == PollState.Started)
                return null;
            var all = 0;
            var votes = new Dictionary<PollOption, int>();
            foreach (var keyValuePair in Votes)
            {
                votes.Add(keyValuePair.Key, keyValuePair.Value.Count);
                all += keyValuePair.Value.Count;
            }

            return new PollResult(all, votes);
        }

        public void ResetPoll()
        {
            if (State == PollState.Created)
                return;

            State = PollState.Created;
            Votes = new Dictionary<PollOption, List<string>>();
            Options.ForEach(o => Votes.Add(o, new List<string>()));
        }

        public void RegisterVote(PollOption option, string user)
        {
            if (State != PollState.Started)
                return;
            if (UserVoted(user))
                return;
            if (!Votes.ContainsKey(option))
                return;

            Votes[option].Add(user);
        }

        private bool UserVoted(string user)
        {
            var r = false;
            foreach (var keyValuePair in Votes)
            {
                if (r)
                    return true;

                r = keyValuePair.Value.Contains(user);
            }

            return r;
        }
    }

    internal class PollOption
    {
        public PollOption(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }

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

    internal enum PollState
    {
        Created,
        Started,
        Aborted,
        Finished
    }
}