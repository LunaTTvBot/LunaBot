using System;
using System.Text.RegularExpressions;
using IBot.Events.Args.Users;
using IBot.Events.Misc;
using IBot.Misc;

namespace IBot.Events
{
    internal static class UserEventManager
    {
        public const string UserJoinPartPattern =
            "^" + GlobalTwitchPatterns.TwichUserBasePattern + @"(JOIN|PART)\s" +
            @"\#(" + GlobalTwitchPatterns.TwitchChannelNamePattern + ")$";

        public const string UserPublicMessagePattern =
            @"^(.*)\s?" + GlobalTwitchPatterns.TwichUserBasePattern + @"PRIVMSG\s" +
            @"\#(" + GlobalTwitchPatterns.TwitchChannelNamePattern + @")\s:(.*)$";

        public const string UserWhisperMessagePattern =
            @"^(.*)\s?" + GlobalTwitchPatterns.TwichUserBasePattern + @"WHISPER\s" +
            @"(" + GlobalTwitchPatterns.TwitchUserNamePattern + @")\s:(.*)$";

        public const string UserMessageTagsPattern =
            @"^@color=([^;]*);display-name=([\d\w]*);emotes=(.*);mod=([0|1]);room-id=(\d+);subscriber=([0|1]);turbo=([0|1]);user-id=(\d+);user-type=(.*)$";

        public const string UserStatePattern =
            @"^(.*)\s:" + GlobalTwitchPatterns.TwitchHostNamePattern + @"\sUSERSTATE\s" +
            @"\#(" + GlobalTwitchPatterns.TwitchChannelNamePattern + @")$";

        public const string UserStateTagsPattern =
            @"^@color=([^;]*);display-name=([\d\w]*);emote-sets=(.*);mod=([0|1]);room-id=(\d+);subscriber=([0|1]);turbo=([0|1]);user-type=(.*)$";

        private static readonly Regex RegExJoinPart = new Regex(UserJoinPartPattern);
        private static readonly Regex RegExPubMsg = new Regex(UserPublicMessagePattern);
        private static readonly Regex RegExPrivMsg = new Regex(UserWhisperMessagePattern);
        private static readonly Regex RegExMsgTags = new Regex(UserMessageTagsPattern);
        private static readonly Regex RegExUsrState = new Regex(UserStatePattern);
        private static readonly Regex RegExUsrStTags = new Regex(UserStateTagsPattern);

        static UserEventManager()
        {
            IrcConnection.GetIrcConnection(ConnectionType.BotCon).RaiseMessageEvent += CheckAndRaiseMessageEvent;
        }

        public static void CheckAndRaiseMessageEvent(object sender, MessageEventArgs msgEvArgs)
        {
            RaiseUserJoinPartEvent(msgEvArgs.Message);
            RaiseUserPublicMessageEvent(msgEvArgs.Message);
            RaiseUserWhisperMessageEvent(msgEvArgs.Message);
            RaiseUserStateEvent(msgEvArgs.Message);
        }

        public static event EventHandler<UserEventArgs> UserJoinEvent;
        public static event EventHandler<UserEventArgs> UserPartEvent;
        public static event EventHandler<UserPublicMessageEventArgs> UserPublicMessageEvent;
        public static event EventHandler<UserWhisperMessageEventArgs> UserWhisperMessageEvent;
        public static event EventHandler<UserStateEventArgs> UserStateEvent;

        private static void OnUserJoinEvent(UserEventArgs e) => UserJoinEvent?.Invoke(typeof(EventManager), e);

        private static void OnUserPartEvent(UserEventArgs e) => UserPartEvent?.Invoke(typeof(EventManager), e);

        private static void OnUserPublicMessageEvent(UserPublicMessageEventArgs e)
            => UserPublicMessageEvent?.Invoke(null, e);

        private static void OnUserwhisperMessageEvent(UserWhisperMessageEventArgs e)
            => UserWhisperMessageEvent?.Invoke(null, e);

        private static void OnUserStateEvent(UserStateEventArgs e) => UserStateEvent?.Invoke(null, e);

        private static void RaiseUserStateEvent(string message)
        {
            var match = RegExUsrState.Match(message);
            if (!match.Success)
                return;

            var tags = ParseStateTags(match.Groups[1].Value);

            OnUserStateEvent(new UserStateEventArgs(tags, match.Groups[2].Value));
        }

        private static void RaiseUserJoinPartEvent(string message)
        {
            var match = RegExJoinPart.Match(message);
            if (!match.Success)
                return;

            if (match.Groups[2].Value == "JOIN")
                OnUserJoinEvent(new UserEventArgs(match.Groups[1].Value, match.Groups[3].Value, UserEventType.Join));
            else
                OnUserPartEvent(new UserEventArgs(match.Groups[1].Value, match.Groups[3].Value, UserEventType.Part));
        }

        private static void RaiseUserPublicMessageEvent(string message)
        {
            var match = RegExPubMsg.Match(message);
            if (!match.Success)
                return;

            var tags = ParsePublicMessageTags(match.Groups[1].Value);

            OnUserPublicMessageEvent(new UserPublicMessageEventArgs(tags,
                                                                    match.Groups[2].Value,
                                                                    match.Groups[3].Value,
                                                                    match.Groups[4].Value));
        }

        private static void RaiseUserWhisperMessageEvent(string message)
        {
            var match = RegExPrivMsg.Match(message);
            if (!match.Success)
                return;

            var tags = ParsePublicMessageTags(match.Groups[1].Value);

            OnUserwhisperMessageEvent(new UserWhisperMessageEventArgs(tags,
                                                                      match.Groups[2].Value,
                                                                      match.Groups[3].Value,
                                                                      match.Groups[4].Value));
        }

        private static UserStateTags ParseStateTags(string tags)
        {
            var match = RegExUsrStTags.Match(tags);
            return !match.Success
                       ? null
                       : new UserStateTags(match.Groups[1].Value,
                                           match.Groups[2].Value,
                                           match.Groups[3].Value,
                                           match.Groups[4].Value == "1",
                                           match.Groups[5].Value == "1",
                                           match.Groups[6].Value == "1",
                                           match.Groups[7].Value);
        }

        private static UserMessageTags ParsePublicMessageTags(string tags)
        {
            var match = RegExMsgTags.Match(tags);
            return !match.Success
                       ? null
                       : new UserMessageTags(match.Groups[1].Value,
                                             match.Groups[2].Value,
                                             match.Groups[3].Value,
                                             match.Groups[4].Value == "1",
                                             Convert.ToInt64(match.Groups[5].Value),
                                             match.Groups[6].Value == "1",
                                             match.Groups[7].Value == "1",
                                             Convert.ToInt64(match.Groups[8].Value),
                                             match.Groups[9].Value);
        }
    }
}
