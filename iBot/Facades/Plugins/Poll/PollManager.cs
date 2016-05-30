using System;
using System.Collections.Generic;
using IBot.Core;
using IBot.Facades.Core.Settings;
using IBot.Facades.Plugins.Poll.EventArgs;
using IBot.Plugins.Poll;
using PollLocale = IBot.Facades.Resources.Plugins.Poll.Poll;

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
            var res = new Result<Poll>(PollLocale.unknown_error, 400, null);

            if (options.Length <= 1)
                return new Result<Poll>(PollLocale.poll_no_sense, 400, null);

            var poll = PollPlugin.CreatePoll(title, options);
            if (poll != null)
            {
                res = new Result<Poll>(string.Empty, 0, new Poll(poll));
            }

            return res;
        }

        public static Result<bool> DeletePoll(Poll poll)
        {
            var res = new Result<bool>(PollLocale.unknown_error, 400, false);

            if (poll.State == State.Started)
                return new Result<bool>(PollLocale.poll_running_no_delete, 400, false);

            var resPoll = PollPlugin.DeletePoll(poll.Id);
            if (resPoll != null)
                res = new Result<bool>(string.Empty, 0, true);

            return res;
        }

        public static Result<bool> AbortPoll(Poll poll) {
            var res = new Result<bool>(PollLocale.unknown_error, 400, false);

            if(poll.State != State.Started)
                return new Result<bool>(PollLocale.poll_not_started_no_abort, 400, false);

            var resPoll = PollPlugin.AbortPoll(poll.Id);
            if(resPoll != null)
                res = new Result<bool>(string.Empty, 0, true);

            return res;
        }

        public static Result<bool> ResetPoll(Poll poll) {
            var res = new Result<bool>(PollLocale.unknown_error, 400, false);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if(poll.State == State.Started)
                return new Result<bool>(PollLocale.poll_running_no_reset, 400, false);

            if(poll.State == State.Created)
                return new Result<bool>(string.Empty, 0, true);

            var resPoll = PollPlugin.ResetPoll(poll.Id);
            if(resPoll != null)
                res = new Result<bool>(string.Empty, 0, true);

            return res;
        }

        public static Result<bool> StartPoll(Poll poll, string channel, int time = 5) 
        {
            var res = new Result<bool>(PollLocale.unknown_error, 400, false);

            if(poll.State == State.Started)
                return new Result<bool>(PollLocale.poll_running, 400, false);

            if(poll.State != State.Created)
                return new Result<bool>(PollLocale.poll_not_created, 400, false);

            if(time < 1)
                return new Result<bool>(PollLocale.poll_invalid_timespan, 400, false);

            if(!SettingsManager.GetSettings<ConnectionSettings>().ChannelList.Contains(channel))
                return new Result<bool>(PollLocale.invalid_channel, 400, false);

            var resPoll = PollPlugin.StartPoll(poll.Id, channel, time);
            if(resPoll != null)
                res = new Result<bool>(string.Empty, 0, true);

            return res;
        }

        public static Result<List<Poll>> GetPollList()
        {
            var res = new Result<List<Poll>>(PollLocale.unknown_error, 400, null);
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