using System;
using System.Collections.Generic;
using IBot.Facades.Plugins.Poll.EventArgs;
using IBot.Plugins.Poll;
using CorePoll = IBot.Plugins.Poll.Poll;

namespace IBot.Facades.Plugins.Poll
{
    public class PollManager
    {
        static PollManager()
        {
            PollPlugin.PollAbortedEvent +=
                (sender, args) =>
                    OnPollAbortedEvent(new PollChangedEventArgs(new Poll(args.Poll), TranslatePollState(args.NewState)));

            PollPlugin.PollCreatedEvent +=
                (sender, args) =>
                    OnPollCreatedEvent(new PollCreatedEventArgs(new Poll(args.Poll)));

            PollPlugin.PollDeletedEvent +=
                (sender, args) =>
                    OnPollDeletedEvent(new PollChangedEventArgs(new Poll(args.Poll), TranslatePollState(args.NewState)));

            PollPlugin.PollFinishedEvent +=
                (sender, args) =>
                    OnPollFinishedEvent(new PollChangedEventArgs(new Poll(args.Poll), TranslatePollState(args.NewState)));

            PollPlugin.PollResettedEvent +=
                (sender, args) =>
                    OnPollResettedEvent(new PollChangedEventArgs(new Poll(args.Poll), TranslatePollState(args.NewState)));

            PollPlugin.PollStartedEvent +=
                (sender, args) =>
                    OnPollStartedEvent(new PollChangedEventArgs(new Poll(args.Poll), TranslatePollState(args.NewState)));
        }

        public static event EventHandler<PollCreatedEventArgs> PollCreatedEvent;
        private static void OnPollCreatedEvent(PollCreatedEventArgs e) => PollCreatedEvent?.Invoke(null, e);

        public static event EventHandler<PollChangedEventArgs> PollDeletedEvent;
        private static void OnPollDeletedEvent(PollChangedEventArgs e) => PollDeletedEvent?.Invoke(null, e);

        public static event EventHandler<PollChangedEventArgs> PollStartedEvent;
        private static void OnPollStartedEvent(PollChangedEventArgs e) => PollStartedEvent?.Invoke(null, e);

        public static event EventHandler<PollChangedEventArgs> PollFinishedEvent;
        private static void OnPollFinishedEvent(PollChangedEventArgs e) => PollFinishedEvent?.Invoke(null, e);

        public static event EventHandler<PollChangedEventArgs> PollResettedEvent;
        private static void OnPollResettedEvent(PollChangedEventArgs e) => PollResettedEvent?.Invoke(null, e);

        public static event EventHandler<PollChangedEventArgs> PollAbortedEvent;
        private static void OnPollAbortedEvent(PollChangedEventArgs e) => PollAbortedEvent?.Invoke(null, e);

        public static Result<Poll> CreatePoll(string title, string[] options)
        {
            var res = new Result<Poll>("Unknown Error.", 400, null);

            var poll = PollPlugin.CreatePoll(title, options);
            if(poll != null) {
                res = new Result<Poll>(string.Empty, 0, new Poll(poll));
            }

            return res;
        }

        public static Result<List<Poll>> GetPollList()
        {
            var res = new Result<List<Poll>>("Unknown Error.", 400, null);
            var list = PollPlugin.GetPollList();

            if (list == null) return res;

            var retList = new List<Poll>();
            list.ForEach(poll => retList.Add(new Poll(poll)));
            res = new Result<List<Poll>>(string.Empty, 0, retList);

            return res;
        }

        internal static State TranslatePollState(PollState state)
        {
            switch (state)
            {
                case PollState.Created:
                    return State.Created;
                case PollState.Started:
                    return State.Started;
                case PollState.Aborted:
                    return State.Aborted;
                case PollState.Finished:
                    return State.Finished;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum State
    {
        Created,
        Started,
        Aborted,
        Finished
    }
}