using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using IRCConnectionTest.Events;
using IRCConnectionTest.Events.Commands;
using IRCConnectionTest.Events.CustomEventArgs;
using IRCConnectionTest.Misc;
using PollLocale = IRCConnectionTest.Resources.Plugins.Poll.poll;
using Timer = System.Timers.Timer;

namespace IRCConnectionTest.Plugins.Poll
{
    /**
     * Poll Plugin by Patrick Conrad
     * 
     * !poll
     * -----
     * Params:
     * _OPTIONS_    string[] with | as seperator
     * _TITLE_      string
     * _ID_         int 
     * _TIME_       int time in minutes
     * 
     * Commands:
     * !poll                                - Description
     * !poll create(_TITLE_) _OPTIONS_      - Creates a poll with _TITLE_ and given _OPTIONS_
     * !poll create _OPTIONS_               - Creates a poll with given _OPTIONS_
     * !poll list                           - Lists all created polls
     * !poll delete:_ID_                    - Delete poll with _ID_
     * !poll start:_ID_ _TIME_              - Opens poll with _ID_ for _TIME_ minutes
     * !poll start:_ID_                     - Opens poll with _ID_ for 5 minutes
     * !poll abort:_ID_                     - Stops poll with _ID_ directly
     * !poll reset:_ID_                     - Resets poll with _ID_ to not runned
     * !poll result:_ID_                    - Shows result of poll with _ID_
     * 
     * !vote
     * -----
     * Params:
     * _ID_     int
     * _OPTION_ int|string
     * 
     * Commands:
     * !vote:_ID_ _OPTION_  - Vote in poll with _ID_ for _OPTION_
     */

    internal class PollPlugin : IPlugin
    {
        private const string PollCreateTitlePattern = @"^\screate\((.*)\)\s(.*)";
        private const string PollCreatePattern = @"^\screate\s(.*)";
        private const string PollOptionsPattern = @"([^|]+)\|?";
        private const string PollListPattern = @"^\slist";
        private const string PollStartPattern = @"^\s(start|restart):([0-9]+)\s?([0-9]*)";
        private const string PollActionsPattern = @"^\s(delete|abort|result|reset):([0-9]+)";
        private const string VotePattern = @"^:([0-9]+)\s([0-9]+)";

        private static readonly Regex PollCreateTitleRegEx = new Regex(PollCreateTitlePattern);
        private static readonly Regex PollCreateRegEx = new Regex(PollCreatePattern);
        private static readonly Regex PollOptionsRegEx = new Regex(PollOptionsPattern);
        private static readonly Regex PollListRegEx = new Regex(PollListPattern);
        private static readonly Regex PollStartRegEx = new Regex(PollStartPattern);
        private static readonly Regex PollActionsRegEx = new Regex(PollActionsPattern);
        private static readonly Regex VoteRegEx = new Regex(VotePattern);

        private static readonly List<Poll> PollsStack = new List<Poll>();
        private static readonly Dictionary<Poll, Timer> TimerStack = new Dictionary<Poll, Timer>();

        public string PluginName => PollLocale.poll_plugin_name;

        public void Execute()
        {
            Logger.Write($"{PluginName} {PollLocale.poll_plugin_loaded}");
            CommandManager.RegisterPublicChannelCommand(new PublicChannelCommand
            {
                RegEx = "!poll(.*)",
                Name = PluginName,
                Action = PollAction
            });

            CommandManager.RegisterWhisperCommand(new WhisperCommand
            {
                RegEx = "!poll(.*)",
                Name = PluginName,
                Action = PollAction
            });

            CommandManager.RegisterPublicChannelCommand(new PublicChannelCommand
            {
                RegEx = "!vote(.*)",
                Name = "Vote",
                Action = VoteAction
            });
        }

        private static void SendMessage(string msg, AnswerType aType, string target)
        {
            IrcConnection.Write(ConnectionType.BotCon, aType, target, msg);
        }

        private static bool HandlePollBaseCommand(string pollParams, AnswerType answerT, string answerTarget)
        {
            if (pollParams != "") return false;

            SendMessage(Thread.CurrentThread.CurrentUICulture.Name, AnswerType.Public, "ipaat");
            SendMessage(PollLocale.poll_help, answerT, answerTarget);

            return true;
        }

        private static bool HandlePollCreateTitleCommand(string pollParams, AnswerType answerType, string answerTarget)
        {
            var m = PollCreateTitleRegEx.Match(pollParams);
            if (!m.Success) return false;

            var optM = PollOptionsRegEx.Matches(m.Groups[2].Value);
            if (optM.Count <= 0) return false;

            if (optM.Count == 1)
            {
                SendMessage(PollLocale.poll_makes_no_sense, answerType, answerTarget);
                return true;
            }

            var list = (from Match match in optM select match.Groups[1].Value).ToList();
            var idx = 1;
            if (PollsStack.Count > 0)
            {
                var lIdx = PollsStack[PollsStack.Count - 1].Id;
                idx = lIdx + 1;
            }

            var optList = list.Select((t, i) => new PollOption(i + 1, t)).ToList();

            var p = new Poll(m.Groups[1].Value, optList, idx);
            PollsStack.Add(p);

            SendMessage(string.Format(PollLocale.poll_created, p.Id), answerType, answerTarget);

            return true;
        }

        private static bool HandlePollCreateCommand(string pollParams, AnswerType answerType, string answerTarget)
        {
            var m = PollCreateRegEx.Match(pollParams);
            if (!m.Success) return false;

            var optM = PollOptionsRegEx.Matches(m.Groups[1].Value);
            if (optM.Count <= 0) return true;

            if (optM.Count == 1)
            {
                SendMessage(PollLocale.poll_makes_no_sense, answerType, answerTarget);
                return true;
            }

            var list = (from Match match1 in optM select match1.Groups[1].Value).ToList();
            var idx = 1;
            if (PollsStack.Count > 0)
            {
                var lIdx = PollsStack[PollsStack.Count - 1].Id;
                idx = lIdx + 1;
            }

            var optList = list.Select((t, i) => new PollOption(i + 1, t)).ToList();

            var p = new Poll("", optList, idx);
            PollsStack.Add(p);

            SendMessage(string.Format(PollLocale.poll_created, p.Id), answerType, answerTarget);

            return true;
        }

        private static bool HandlePollListCommand(string pollParams, AnswerType answerType, string answerTarget)
        {
            var m = PollListRegEx.Match(pollParams);
            if (!m.Success) return false;

            if (PollsStack.Count == 0)
            {
                SendMessage(PollLocale.no_polls, answerType, answerTarget);
                return true;
            }

            var stringB = new StringBuilder();
            PollsStack.ForEach(poll =>
            {
                stringB.Clear();

                stringB.Append($"#{poll.Id}");
                if (poll.Title != "")
                    stringB.Append($" - {poll.Title}");

                stringB.Append($" ({poll.GetPollState().ToString()}) ");

                poll.Options.ForEach(option =>
                {
                    var idx = poll.Options.IndexOf(option);
                    if (idx != 0)
                        stringB.Append(", ");
                    stringB.Append($"{option.Id}:{option.Name}");
                });

                SendMessage(stringB.ToString(), answerType, answerTarget);
            });

            return true;
        }

        private static bool HandlePollStart(string pollParams, AnswerType answerType, string answerTarget)
        {
            var m = PollStartRegEx.Match(pollParams);
            if (!m.Success) return false;
            var restart = m.Groups[1].Value == "restart";

            // NOT Available on private chat because we don't know where to send the started message.
            // Because no channel is provided for private messages.
            if (answerType == AnswerType.Private)
            {
                SendMessage(
                    string.Format(PollLocale.command_not_available_private,
                        "!poll start:POLL-ID"), answerType,
                    answerTarget);
                return true;
            }

            var pIdx = Convert.ToInt32(m.Groups[2].Value);
            var time = m.Groups[3].Value == "" ? 5 : Convert.ToInt32(m.Groups[3].Value);
            if (!PollsStack.Exists(poll => poll.Id == pIdx))
            {
                SendMessage(string.Format(PollLocale.poll_not_found, pIdx), answerType,
                    answerTarget);
                return true;
            }

            var p = PollsStack.Find(poll => poll.Id == pIdx);
            if (p.GetPollState() == PollState.Started)
            {
                SendMessage(string.Format(PollLocale.poll_running, p.Id), answerType,
                    answerTarget);
                return true;
            }

            if (p.GetPollState() != PollState.Created)
            {
                if (!restart)
                {
                    SendMessage(string.Format(PollLocale.poll_wrong_state_restart, p.Id),
                        answerType,
                        answerTarget);
                    return true;
                }

                p.ResetPoll();
            }

            StartPoll(p, time, answerType, answerTarget);
            return true;
        }

        private static void HandlePollDelete(Poll p, AnswerType answerType, string answerTarget)
        {
            if (p.GetPollState() == PollState.Started)
            {
                SendMessage(string.Format(PollLocale.poll_running, p.Id), answerType,
                    answerTarget);
                return;
            }

            PollsStack.Remove(p);
            SendMessage(string.Format(PollLocale.poll_removed, p.Id), answerType, answerTarget);
        }

        private static void HandlePollAbort(Poll p, AnswerType answerType, string answerTarget)
        {
            if (p.GetPollState() != PollState.Started)
            {
                SendMessage(string.Format(PollLocale.poll_not_running, p.Id), answerType,
                    answerTarget);
                return;
            }

            p.AbortPoll();
            if (TimerStack.ContainsKey(p))
            {
                TimerStack[p].Enabled = false;
                TimerStack[p].Stop();
                TimerStack.Remove(p);
            }

            SendMessage(string.Format(PollLocale.poll_aborted, p.Id), answerType, answerTarget);
        }

        private static void HandlePollReset(Poll p, AnswerType answerType, string answerTarget)
        {
            switch (p.GetPollState())
            {
                case PollState.Created:
                    SendMessage(string.Format(PollLocale.poll_is_resetted, p.Id), answerType,
                        answerTarget);
                    return;
                case PollState.Started:
                    SendMessage(string.Format(PollLocale.poll_running, p.Id), answerType,
                        answerTarget);
                    return;
                case PollState.Aborted:
                    break;
                case PollState.Finished:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            p.ResetPoll();
            SendMessage(string.Format(PollLocale.poll_resetted, p.Id), answerType, answerTarget);
        }

        private static void HandlePollResult(Poll p, AnswerType answerType, string answerTarget)
        {
            if (p.GetPollState() != PollState.Finished)
            {
                SendMessage(string.Format(PollLocale.poll_not_ran, p.Id), answerType,
                    answerTarget);
                return;
            }

            var result = p.GetPollResults();
            if (result.All == 0)
            {
                SendMessage(string.Format(PollLocale.poll_no_votes, p.Id), answerType,
                    answerTarget);
                return;
            }

            var stringBuilder = new StringBuilder();
            var i = 0;
            foreach (var item in result.VotesPerOption)
            {
                var percent = 0D;
                if (i > 0) stringBuilder.Append("; ");
                if (item.Value > 0) percent = item.Value/(double) result.All;
                stringBuilder.Append($"{item.Key.Name}: {item.Value} ({percent.ToString("0.00%")})");
                i++;
            }

            // {0} people voted for Poll {1}. The results are: {2}
            SendMessage(
                string.Format(PollLocale.poll_result,
                    result.All,
                    p.Id,
                    stringBuilder
                    ), answerType, answerTarget);
        }

        private static void HandlePollActions(string pollParams, AnswerType answerType, string answerTarget)
        {
            var m = PollActionsRegEx.Match(pollParams);
            if (!m.Success) return;

            var pIdx = Convert.ToInt32(m.Groups[2].Value);
            if (!PollsStack.Exists(poll => poll.Id == pIdx))
            {
                SendMessage(string.Format(PollLocale.poll_not_found, pIdx), answerType,
                    answerTarget);
                return;
            }

            var p = PollsStack.Find(poll => poll.Id == pIdx);

            switch (m.Groups[1].Value)
            {
                case "result":
                    HandlePollResult(p, answerType, answerTarget);
                    break;
                case "delete":
                    HandlePollDelete(p, answerType, answerTarget);
                    break;
                case "reset":
                    HandlePollReset(p, answerType, answerTarget);
                    break;
                case "abort":
                    HandlePollAbort(p, answerType, answerTarget);
                    break;
                default:
                    SendMessage(string.Format(PollLocale.poll_wrong_command, pIdx), answerType,
                        answerTarget);
                    break;
            }
        }

        private static void PollAction(WhisperCommand command, Match match, UserWhisperMessageEventArgs eArgs)
        {
            var pollParams = match.Groups[1].Value;

            try
            {
                HandlePollCommands(pollParams, AnswerType.Private, eArgs.UserName);
            }
            catch (Exception)
            {
                SendMessage(PollLocale.poll_base_error, AnswerType.Private,
                    eArgs.UserName);
            }
        }

        private static void PollAction(PublicChannelCommand command, Match match, UserPublicMessageEventArgs eArgs)
        {
            var pollParams = match.Groups[1].Value;

            try
            {
                // !poll
                // Always send private due to a lot of text.
                if (HandlePollBaseCommand(pollParams, AnswerType.Private, eArgs.UserName))
                    return;

                HandlePollCommands(pollParams, AnswerType.Public, eArgs.Channel);
            }
            catch (Exception)
            {
                SendMessage(PollLocale.poll_base_error, AnswerType.Public,
                    eArgs.Channel);
            }
        }

        private static void VoteAction(PublicChannelCommand command, Match match, UserPublicMessageEventArgs eArgs)
        {
            var voteParams = match.Groups[1].Value;

            try
            {
                HandleVoteCommand(voteParams, AnswerType.Public, eArgs.Channel, eArgs.UserName);
            }
            catch (Exception)
            {
                SendMessage(PollLocale.poll_base_error, AnswerType.Public,
                    eArgs.Channel);
            }
        }

        private static void HandlePollCommands(string pollParams, AnswerType aType, string target)
        {
            // !poll create(_TITLE_) _OPTIONS_ 
            if (HandlePollCreateTitleCommand(pollParams, aType, target))
                return;

            // !poll create _OPTIONS_ 
            if (HandlePollCreateCommand(pollParams, aType, target))
                return;

            // !poll list
            if (HandlePollListCommand(pollParams, aType, target))
                return;

            // !poll start:_ID_
            // !poll start:_ID_ _TIME_
            if (HandlePollStart(pollParams, aType, target))
                return;

            // !poll delete:_ID_
            // !poll abort:_ID_
            // !poll reset:_ID_
            // !poll result:_ID_
            HandlePollActions(pollParams, aType, target);
        }

        private static void HandleVoteCommand(string voteParams, AnswerType aType, string target, string userName)
        {
            var m = VoteRegEx.Match(voteParams);
            if (!m.Success) return;
            var pId = Convert.ToInt32(m.Groups[1].Value);
            var oId = Convert.ToInt32(m.Groups[2].Value);

            if (!PollsStack.Exists(poll => poll.Id == pId))
            {
                SendMessage(string.Format(PollLocale.poll_not_running_vote, pId), aType, target);
                return;
            }

            var p = PollsStack.Find(poll => poll.Id == pId);
            if (p.GetPollState() != PollState.Started)
            {
                SendMessage(string.Format(PollLocale.poll_running, p.Id), aType, target);
                return;
            }

            if (!p.Options.Exists(opt => opt.Id == oId))
            {
                SendMessage(string.Format(PollLocale.poll_no_such_option, p.Id, oId), aType, target);
                return;
            }
            var o = p.Options.Find(opt => opt.Id == oId);

            if (p.UserVoted(userName))
            {
                SendMessage(string.Format(PollLocale.poll_already_voted, p.Id), aType, target);
                return;
            }

            p.RegisterVote(o, userName);
            SendMessage(string.Format(PollLocale.poll_voted, p.Id), aType, target);
        }

        private static void StartPoll(Poll poll, int time, AnswerType answerType, string answerTarget)
        {
            if (!TimerStack.ContainsKey(poll))
                TimerStack.Add(poll, new Timer());

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var t = TimerStack[poll];
                t.Enabled = false;
                t.Interval = time*60*1000;
                t.Elapsed += (sender, args) =>
                {
                    TimerStack[poll].Enabled = false;
                    TimerStack[poll].Stop();
                    TimerStack.Remove(poll);

                    poll.StopPoll();
                    var prefix = $"{poll.Id}";
                    if (poll.Title != "")
                        prefix += $": '{poll.Title}'";
                    var res = poll.GetPollResults();

                    if (res.All == 0)
                    {
                        SendMessage(string.Format(PollLocale.poll_no_votes, poll.Id),
                            answerType, answerTarget);
                        return;
                    }

                    var max = res.VotesPerOption.Values.Max();
                    var percent = 0D;
                    if (max > 0) percent = max / (double) res.All;

                    // Poll {0} finished. {1} won with {2} ({3}%) from {4} votes. Enter !poll result:{5} for further information.
                    var text = string.Format(PollLocale.poll_finished, prefix,
                        res.VotesPerOption.FirstOrDefault(x => x.Value == max).Key.Name, max, percent.ToString("0.00%"), res.All, poll.Id);

                    SendMessage(text, answerType, answerTarget);
                };
                t.Enabled = true;
            });

            poll.StartPoll();

            string prefixB = $"{poll.Id}";
            if (poll.Title != "")
                prefixB += $": '{poll.Title}'";

            var stringB = new StringBuilder();
            // Poll {0} started for {1} minute(s). Vote now with !vote:{2} OPTION-ID. Options are: 
            stringB.AppendFormat(PollLocale.poll_started, prefixB, time, poll.Id);
            stringB.Append(@" ");
            poll.Options.ForEach(option =>
            {
                var idx = poll.Options.IndexOf(option);
                if (idx != 0)
                    stringB.Append(", ");
                stringB.Append($"{option.Id} = {option.Name}");
            });

            SendMessage(stringB.ToString(), answerType, answerTarget);
        }
    }
}