using System;
using System.Collections.Generic;
using IBot.Plugins.Poll;
using CorePoll = IBot.Plugins.Poll.Poll;

namespace IBot.Facades.Plugins.Poll
{
    public class Poll
    {
        public Poll(string title, List<Option> options, int id)
        {
            Title = title;
            Options = options;
            Id = id;
            State = State.Created;
            Votes = new Dictionary<Option, List<string>>();
            Options.ForEach(o => Votes.Add(o, new List<string>()));
        }

        internal Poll(CorePoll poll)
        {
            Title = poll.Title;
            Id = poll.Id;

            Options = new List<Option>();
            poll.Options.ForEach(option => Options.Add(new Option(option)));            

            Votes = new Dictionary<Option, List<string>>();
            foreach (var keyValuePair in poll.Votes)
            {
                Votes.Add(new Option(keyValuePair.Key), keyValuePair.Value);
            }

            switch (poll.GetPollState())
            {
                case PollState.Created:
                    State = State.Created;
                    break;
                case PollState.Started:
                    State = State.Started;
                    break;
                case PollState.Aborted:
                    State = State.Aborted;
                    break;
                case PollState.Finished:
                    State = State.Finished;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string Title { get; }
        public List<Option> Options { get; }
        public int Id { get; }

        private Dictionary<Option, List<string>> Votes { get; }
        public State State { get; }

        public Result GetPollResults()
        {
            if (State == State.Created || State == State.Started)
                return null;
            var all = 0;
            var votes = new Dictionary<Option, int>();
            foreach (var keyValuePair in Votes)
            {
                votes.Add(keyValuePair.Key, keyValuePair.Value.Count);
                all += keyValuePair.Value.Count;
            }

            return new Result(all, votes);
        }
    }
}